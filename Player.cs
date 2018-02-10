using kinkaudio;
using System;
using System.IO;
using System.Collections.Generic;

namespace kinkaudiorender
{
    public class kinkaudiorender
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
            Compiler.Compile(args[0], out var Envelops, out var Wavcomms,
            out var musicCommands, out string metadata, out int tickrate);

            int samplerate = Convert.ToInt32(args[1]);
            if ( samplerate % tickrate != 0 )
            {
                Console.WriteLine("BAD SAMPLERATE FOR THAT TICKRATE, BAKA");
                return 1;
            }

            //Console.WriteLine(metadata + " at " + tickrate + "hz.");

            int channelCount = Convert.ToInt32(musicCommands[0][0]);

            List<List<float>> unMixedChannels = new List<List<float>>();

            for ( int i = 0; i < channelCount; i++ )
            {
                string currentWav = string.Empty;
                string currentEnv = string.Empty;
                string currentPitchEnv = string.Empty;
                string currentPitchWav = string.Empty;
                float currentPitchWavPeriod = 1;
                float currentPitchWavAmp = 1;
                int currentDuty = 200;
                float currentAmp = 1;

                List<float> currentChannel = new List<float>();

                bool isLoop = false;

                int channelOctave = 4;

                int channelTime = 0;
                int channelFakeTime = 0;
                foreach ( var command in musicCommands[i] )
                {
                    float pitchShift = 1f;
                    if ( command.Contains("envSet") )
                    {
                        currentEnv = command.Split(new [] { ' ' })[1];
                    }
                    else if ( command.Contains("wavSet") )
                    {
                        currentWav = command.Split(new [] { ' ' })[1];
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
                    else if ( command.Contains("pitchEnv") )
                    {
                        currentPitchEnv = command.Split(new [] { ' ' })[1];
                    }
                    else if ( command.Contains("octaveSet") )
                    {
                        channelOctave = Convert.ToInt32(Math.Pow(2, Convert.ToDouble(
                            command.Split(new [] { ' ' })[1])));
                    }
                    else if ( command.Contains("pitchVibrato") )
                    {
                        currentPitchWav = command.Split(new [] { ' ' })[1];
                    }
                    else if ( command.Contains("vibSpeed") )
                    {
                        currentPitchWavPeriod = Convert.ToSingle(
                            command.Split(new [] { ' ' })[1]);
                    }
                    else if ( command.Contains("vibAmplitude") )
                    {
                        currentPitchWavAmp = Convert.ToSingle(
                            command.Split(new [] { ' ' })[1]);
                    }
                    else if ( command.Contains("retrig") )
                    {
                        channelFakeTime = 0;
                        for ( int r = 0; r < (samplerate / tickrate); r++)
                        {
                            channelFakeTime++;
                            channelTime++;
                            foreach ( var envelope in Envelops )
                            {
                                if ( currentPitchEnv.Contains(envelope.envName) )
                                {
                                    pitchShift = pitchShift +
                                    RenderEnv(envelope.envType,
                                    channelFakeTime, envelope.envValues[0], 
                                    envelope.envValues[1], envelope.envValues[2],
                                    Convert.ToInt32(envelope.envValues[3]));
                                }
                            }
                            foreach ( var wave in Wavcomms )
                            {
                                if ( currentPitchWav.Contains(wave.wavName) )
                                {
                                    pitchShift = pitchShift +
                                    RenderWav(wave.wavType,
                                    currentPitchWavPeriod,
                                    currentPitchWavAmp * wave.wavValues[0],
                                    Convert.ToInt32(wave.wavValues[1]),
                                    channelFakeTime);
                                }
                            }
                            foreach ( var env in Envelops )
                            {
                                if ( currentEnv.Contains(env.envName) )
                                {
                                    currentAmp = currentAmp + (
                                        RenderEnv(env.envType, channelFakeTime,
                                        env.envValues[0], env.envValues[1],
                                        env.envValues[2],
                                        Convert.ToInt32(env.envValues[3])) - 1
                                    );
                                }
                            }
                            foreach ( var wave in Wavcomms )
                            {
                                if ( currentWav.Contains(wave.wavName) )
                                {
                                    currentChannel.Add(
                                        RenderWav(wave.wavType,
                                        (samplerate / (Convert.ToSingle(
                                            command.Split(new [] { ' ' })[1])
                                            * channelOctave)),
                                            4, currentDuty,
                                            channelFakeTime));
                                }
                            }
                        }
                    }
                    else if ( command.Contains("noRetrig") )
                    {
                        for ( int r = 0; r < (samplerate / tickrate); r++)
                        {
                            channelFakeTime++;
                            channelTime++;
                            foreach ( var envelope in Envelops )
                            {
                                if ( currentPitchEnv.Contains(envelope.envName) )
                                {
                                    pitchShift = RenderEnv(envelope.envType,
                                    channelFakeTime, envelope.envValues[0], 
                                    envelope.envValues[1], envelope.envValues[2],
                                    Convert.ToInt32(envelope.envValues[3]));
                                }
                            }
                            foreach ( var wave in Wavcomms )
                            {
                                if ( currentPitchWav.Contains(wave.wavName) )
                                {
                                    pitchShift = RenderWav(wave.wavType,
                                    currentPitchWavPeriod,
                                    currentPitchWavAmp * wave.wavValues[0],
                                    Convert.ToInt32(wave.wavValues[1]),
                                    channelFakeTime);
                                }
                            }
                            foreach ( var env in Envelops )
                            {
                                if ( currentEnv.Contains(env.envName) )
                                {
                                    currentAmp = currentAmp + (
                                        RenderEnv(env.envType, channelFakeTime,
                                        env.envValues[0], env.envValues[1],
                                        env.envValues[2],
                                        Convert.ToInt32(env.envValues[3])) - 1
                                    );
                                }
                            }
                            foreach ( var wave in Wavcomms )
                            {
                                if ( currentWav.Contains(wave.wavName) )
                                {
                                    currentChannel.Add(
                                        RenderWav(wave.wavType,
                                        (samplerate / (Convert.ToSingle(
                                            command.Split(new [] { ' ' })[1])
                                            * channelOctave)),
                                            4, currentDuty,
                                            channelFakeTime));
                                }
                            }
                        }
                    }
                }
                unMixedChannels.Add(currentChannel);
            }
            List<float> outputFloats = new List<float>();
            for ( int i = 0; i < unMixedChannels[0].Count; i++)
            {
                float avg = 0f;
                for ( int r = 0; r < unMixedChannels.Count; r++)
                {
                    avg = avg + unMixedChannels[r][i] / channelCount;
                }
                outputFloats.Add(avg);
            }
            List<int> outputInts = new List<int>();
            foreach ( var sample in outputFloats )
            {
                outputInts.Add(Convert.ToInt32(sample * 40));
            }
            foreach ( var outputsample in outputInts )
            {
                Console.Out.Write(outputsample);
            }
            return 1;
        }
    }
}