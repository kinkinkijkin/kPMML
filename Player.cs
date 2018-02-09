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
            else return 0f;
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

            Console.WriteLine(metadata + " at " + tickrate + "hz.");

            int channelCount = Convert.ToInt32(musicCommands[0][0]);

            List<List<float>> unMixedChannels = new List<List<float>>();

            for ( int i = 0; i < channelCount; i++ )
            {
                unMixedChannels.Add(new List<float>());
                string currentWav = string.Empty;
                string currentEnv = string.Empty;
                string currentPitchEnv = string.Empty;
                string currentPitchWav = string.Empty;
                float currentPitchWavPeriod = 0f;
                float currentPitchWavAmp = 1f;
                int currentDuty = 200;
                float currentAmp = 1;

                bool isLoop = false;

                int channelOctave = 0;

                int channelTime = 0;
                int channelFakeTime = 0;
                foreach ( var command in musicCommands[i] )
                {
                    float pitchShift = 0f;
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
                                Console.Write(".");
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
                            if ( !currentPitchEnv.Equals(string.Empty) )
                            {
                                foreach ( var envelope in Envelops )
                                {
                                    if ( !currentPitchEnv.Equals(envelope.envName) )
                                    {}
                                    else
                                    {
                                        pitchShift = pitchShift +
                                        RenderEnv(envelope.envType,
                                        channelFakeTime, envelope.envValues[0], 
                                        envelope.envValues[1], envelope.envValues[2],
                                        Convert.ToInt32(envelope.envValues[3]));
                                    }
                                }
                            }
                            if ( !currentPitchWav.Equals(string.Empty) )
                            {
                                foreach ( var wave in Wavcomms )
                                {
                                    if ( !currentPitchWav.Equals(wave.wavName) )
                                    {}
                                    else
                                    {
                                        pitchShift = pitchShift +
                                        RenderWav(wave.wavType,
                                        currentPitchWavPeriod,
                                        currentPitchWavAmp * wave.wavValues[0],
                                        Convert.ToInt32(wave.wavValues[1]),
                                        channelFakeTime);
                                    }
                                }
                            }
                            if ( !currentEnv.Equals(string.Empty) )
                            {
                                foreach ( var env in Envelops )
                                {
                                    if ( !currentEnv.Equals(env.envName) )
                                    {}
                                    else
                                    {
                                        currentAmp = currentAmp + (
                                            RenderEnv(env.envType, channelFakeTime,
                                            env.envValues[0], env.envValues[1],
                                            env.envValues[2],
                                            Convert.ToInt32(env.envValues[3])) - 1
                                        );
                                    }
                                }
                            }
                            if ( !currentWav.Equals(string.Empty) )
                            {
                                foreach ( var wave in Wavcomms )
                                {
                                    if ( !currentWav.Equals(wave.wavName) )
                                    {}
                                    else
                                    {
                                        unMixedChannels[i].Add(
                                            RenderWav(wave.wavType,
                                            (samplerate / (Convert.ToSingle(
                                                command.Split(new [] { ' ' })[1])
                                                * channelOctave)
                                                * pitchShift), currentAmp, currentDuty,
                                                channelFakeTime));
                                    }
                                }
                            }
                        }
                    }
                    else if ( command.Contains("noretrig") )
                    {
                        for ( int r = 0; r < (samplerate / tickrate); r++)
                        {
                            channelFakeTime++;
                            channelTime++;
                            if ( !currentPitchEnv.Equals(string.Empty) )
                            {
                                foreach ( var envelope in Envelops )
                                {
                                    if ( !currentPitchEnv.Equals(envelope.envName) )
                                    {}
                                    else
                                    {
                                        pitchShift = RenderEnv(envelope.envType,
                                        channelFakeTime, envelope.envValues[0], 
                                        envelope.envValues[1], envelope.envValues[2],
                                        Convert.ToInt32(envelope.envValues[3]));
                                    }
                                }
                            }
                            if ( !currentPitchWav.Equals(string.Empty) )
                            {
                                foreach ( var wave in Wavcomms )
                                {
                                    if ( !currentPitchWav.Equals(wave.wavName) )
                                    {}
                                    else
                                    {
                                        pitchShift = RenderWav(wave.wavType,
                                        currentPitchWavPeriod,
                                        currentPitchWavAmp * wave.wavValues[0],
                                        Convert.ToInt32(wave.wavValues[1]),
                                        channelFakeTime);
                                    }
                                }
                            }
                            if ( !currentEnv.Equals(string.Empty) )
                            {
                                foreach ( var env in Envelops )
                                {
                                    if ( !currentEnv.Equals(env.envName) )
                                    {}
                                    else
                                    {
                                        currentAmp = currentAmp + (
                                            RenderEnv(env.envType, channelFakeTime,
                                            env.envValues[0], env.envValues[1],
                                            env.envValues[2],
                                            Convert.ToInt32(env.envValues[3])) - 1
                                        );
                                    }
                                }
                            }
                            if ( !currentWav.Equals(string.Empty) )
                            {
                                foreach ( var wave in Wavcomms )
                                {
                                    if ( !currentWav.Equals(wave.wavName) )
                                    {}
                                    else
                                    {
                                        unMixedChannels[i].Add(
                                            RenderWav(wave.wavType,
                                            (samplerate / (Convert.ToSingle(
                                                command.Split(new [] { ' ' })[1])
                                                * channelOctave)
                                                * pitchShift), currentAmp, currentDuty,
                                                channelFakeTime));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }
    }
}