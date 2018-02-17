using kinkaudio;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace kPMML
{
    public class kPMML
    {
        static float RenderEnv ( string envType, float atk, float hld, float dcy,
        float sus, int time )
        {
            if ( envType.Equals("AHD") )
            {
                return Envelopes.AHD(atk, hld, dcy, time);
            }
            else if ( envType.Equals("AHDS") )
            {
                return Envelopes.AHDS(atk, hld, dcy, sus, time);
            }
            else return 1f;
        }
        static float RenderWav ( string wavType, float per, float amp, int duty, 
        int time )
        {
            if ( wavType.Equals("SAWT") )
            {
                return Generators.GenSawtooth(time, amp, per);
            }
            else if ( wavType.Equals("PULS") )
            {
                return Generators.GenPulse(time, amp, per, duty);
            }
            else if ( wavType.Equals("TRIA") )
            {
                return Generators.GenTriangle(time, amp, per);
            }
            else if ( wavType.Equals("PCYC") )
            {
                return Generators.GenPCycloid(time, amp, per);
            }
            else if ( wavType.Equals("SINE") )
            {
                return Generators.GenSin(time, amp, per);
            }
            else return 0.1f;
        }
        static float RenderFM ( float inOp, float amp, float mult, int time, int trm,
        int trc )
        {
            return FM.FM2opMergeTrunc(inOp, amp, mult, time, trm, trc);
        }
        static int Main (string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(
                    "ENTER AN INPUT FILE FOLLOWED BY A SAMPLERATE, LARDFUCK");
                return 1;
            }
            else if (args.Length > 2)
            {
                Console.WriteLine("INVALID INPUT, QUEEFJESUS");
                return 1;
            }
            Compiler.Compile(args[0], out var Envelops, out var Wavcomms, out var FMC,
            out var musicCommands, out string metadata, out int tickrate);

            int samplerate = Int32.Parse(args[1]);
            if ( samplerate % tickrate != 0 )
            {
                Console.WriteLine("BAD SAMPLERATE FOR THAT TICKRATE, BAKA");
                return 1;
            }

            //Console.WriteLine(metadata + " at " + tickrate + "hz.");

            int channelCount = Int32.Parse(musicCommands[0][0]);

            long lengthInSamples = 0;
            int lengthInFrames = 0;

            foreach ( var channel in musicCommands )
            {
                if ( channel.Count > lengthInFrames )
                {
                    lengthInSamples = (samplerate / tickrate) * channel.Count;
                    lengthInFrames = channel.Count;
                }
            }

            float[,] currentChannel = new float[channelCount,lengthInSamples];

            Parallel.For ( 0, channelCount + 1, i =>
            {
                string currentFM = string.Empty;
                string currentWav = string.Empty;
                string currentEnv = string.Empty;
                string currentPitchEnv = string.Empty;
                string currentPitchWav = string.Empty;
                float currentPitchWavPeriod = 1;
                float currentPitchWavAmp = 1;
                int currentDuty = 200;
                float currentAmp = 2;
                float currentAmpOffset = 1;

                int channelOctave = 4;

                int channelTime = 0;
                int channelFakeTime = 0;

                foreach ( var command in musicCommands[i] )
                {
                    float pitchShift = 1f;
                    if ( command.Contains("envSet") )
                    {
                        currentEnv = command.Split(' ')[1];
                    }
                    else if ( command.Contains("wavSet") )
                    {
                        currentWav = command.Split(' ')[1];
                        foreach ( var wave in Wavcomms )
                        {
                            if ( !currentWav.Equals(wave.wavName) )
                            {}
                            else
                            {
                                currentDuty = Convert.ToInt32(wave.wavValues[1]);
                                currentAmp = wave.wavValues[0];
                            }
                        }
                    }
                    else if ( command.Contains("fmSet") )
                    {
                        currentFM = command.Split(' ')[1];
                    }
                    else if ( command.Contains("ampSet") )
                    {
                        currentAmpOffset = Single.Parse(command.Split(' ')[1]);
                    }
                    else if ( command.Contains("pitchEnv") )
                    {
                        currentPitchEnv = command.Split(' ')[1];
                    }
                    else if ( command.Contains("octaveSet") )
                    {
                        channelOctave = Convert.ToInt32(
                            Math.Pow(2, 
                                Double.Parse(
                                    command.Split(' ')[1])));
                    }
                    else if ( command.Contains("octaveInc") )
                    {
                        channelOctave = channelOctave * Convert.ToInt32(
                            Math.Pow(2, 
                                Double.Parse(
                                    command.Split(' ')[1])));
                    }
                    else if ( command.Contains("octaveDec") )
                    {
                        channelOctave = channelOctave / Convert.ToInt32(
                            Math.Pow(2,
                                Double.Parse(
                                    command.Split(' ')[1])));
                    }
                    else if ( command.Contains("pitchVibrato") )
                    {
                        currentPitchWav = command.Split(new [] { ' ' })[1];
                    }
                    else if ( command.Contains("vibSpeed") )
                    {
                        currentPitchWavPeriod = Single.Parse(
                            command.Split(' ')[1]);
                    }
                    else if ( command.Contains("vibAmplitude") )
                    {
                        currentPitchWavAmp = Single.Parse(
                            command.Split(' ')[1]);
                    }
                    else if ( command.Contains("retrig") ||
                    command.Contains("noRetrig") )
                    {
                        if ( command.Contains("retrig") )
                        {
                            channelFakeTime = 0;
                        }
                        for ( int r = 0; r < (samplerate / tickrate); r++)
                        {
                            foreach ( var envelope in Envelops )
                            {
                                if ( currentPitchEnv.Contains(envelope.envName) )
                                {
                                    pitchShift = pitchShift + 
                                    RenderEnv(envelope.envType, 
                                    envelope.envValues[0], envelope.envValues[1],
                                    envelope.envValues[2], envelope.envValues[3],
                                    channelFakeTime);
                                }
                            }
                            foreach ( var wave in Wavcomms )
                            {
                                if ( currentPitchWav.Contains(wave.wavName) )
                                {
                                    pitchShift = RenderWav(wave.wavType,
                                    currentPitchWavPeriod,
                                    currentPitchWavAmp + wave.wavValues[0],
                                    Convert.ToInt32(wave.wavValues[1]),
                                    channelFakeTime);
                                }
                            }
                            foreach ( var env in Envelops )
                            {
                                if ( currentEnv.Contains(env.envName) )
                                {
                                    currentAmp = currentAmpOffset +
                                    RenderEnv(env.envType, env.envValues[0],
                                    env.envValues[1], env.envValues[2], env.envValues[3],
                                    channelFakeTime) + 1;
                                }
                            }
                            foreach ( var wave in Wavcomms )
                            {
                                if ( currentWav.Contains(wave.wavName) )
                                {
                                    float f = RenderWav(wave.wavType,
                                        (Convert.ToSingle(samplerate) / (Single.Parse(
                                            command.Split(' ')[1])
                                            * channelOctave) + pitchShift),
                                            currentAmp + wave.wavValues[0], currentDuty,
                                            channelFakeTime);
                                    currentChannel[i - 1,channelTime] = f;
                                        
                                }
                            }
                            foreach ( var fm in FMC )
                            {
                                float carAmp = currentAmp;
                                if ( currentFM.Contains(fm.fmName) )
                                {
                                    foreach ( var env in Envelops )
                                    {
                                        if ( fm.fmEnvName.Equals(env.envName) )
                                        {
                                            carAmp = currentAmpOffset + RenderEnv(
                                                env.envType, env.envValues[0],
                                                env.envValues[1], env.envValues[2],
                                                env.envValues[3], channelFakeTime);
                                        }
                                    }
                                    float f = RenderFM(currentChannel
                                    [fm.fmInputChannel,channelTime],
                                        carAmp, (Convert.ToSingle(samplerate) / (
                                        Single.Parse(command.Split(' ')[1])
                                        * channelOctave
                                        * fm.fmMult)), channelFakeTime, fm.fmTruncMod,
                                        fm.fmTruncCar);
                                    currentChannel[i - 1,channelTime] = f;

                                }
                            }
                            channelFakeTime++;
                            channelTime++;
                        }
                    }
                }
            } );
            float[] mixedChannels = new float[lengthInSamples];
            for ( int i = 0; i < currentChannel.GetLength(1); i++)
            {
                float f = 0f;
                for ( int r = 0; r < currentChannel.GetLength(0); r++)
                {
                    f = f + currentChannel[r,i];
                }
                mixedChannels[i] = f / 3;
            }

            List<byte> outputBytes = new List<byte>();
            foreach ( var sample in mixedChannels )
            {
                outputBytes.AddRange(BitConverter.GetBytes(sample));
            }

            var ffmpeg = new Process
            {
                StartInfo =
                {
                    FileName = "ffmpeg",
                    Arguments = String.Format(
                        "-y -f f32le -ac 1 -i - -c:a libmp3lame -b:a 320k '{0}'.mp3",
                        metadata),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                }
            };

            ffmpeg.Start();

            var ffmpegIn = ffmpeg.StandardInput.BaseStream;

            ffmpegIn.Write(outputBytes.ToArray(), 0, outputBytes.Count);
            
            ffmpegIn.Flush();
            ffmpegIn.Close();

            ffmpeg.WaitForExit();
            return 0;
        }
    }
}