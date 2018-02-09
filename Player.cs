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

            for ( int i = 1; i < channelCount + 1; i++ )
            {
                string currentWav = string.Empty;
                string currentEnv = string.Empty;
                string currentPitchEnv = string.Empty;
                string currentPitchWav = string.Empty;
                float currentPitchWavPeriod = 0f;
                float currentPitchWavAmp = 0f;

                bool isLoop = false;

                float pitchShift = 0f;

                int channelOctave = 0;

                int channelTime = 0;
                int channelFakeTime = 0;
                foreach ( var command in musicCommands[i] )
                {
                    if ( command.Contains("envSet") )
                    {
                        currentEnv = command.Split(new [] { ' ' })[1];
                    }
                    else if ( command.Contains("wavSet") )
                    {
                        currentWav = command.Split(new [] { ' ' })[1];
                    }
                    else if ( command.Contains("pitchEnv") )
                    {
                        currentPitchEnv = command.Split(new [] { ' ' })[1];
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
                        for ( int r = 0; r < samplerate / tickrate; r++)
                        {
                            channelFakeTime++;
                            channelTime++;
                            if (!currentPitchEnv.Equals(string.Empty))
                            {
                                foreach ( var envelope in Envelops )
                                {
                                    if ( currentPitchEnv.Split(
                                        new [] { ' ' })[1].Equals(envelope.envName) )
                                    {

                                    }
                                }
                            }
                            if (!currentPitchWav.Equals(string.Empty))
                            {

                            }
                        }
                    }
                    else if ( command.Contains("retrig") )
                    {
                        for ( int r = 0; r < samplerate / tickrate; r++)
                        {
                            channelFakeTime++;
                            channelTime++;
                        }
                    }
                }
            }
            return 0;
        }
    }
}