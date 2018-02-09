using kinkaudio;
using System;
using System.IO;
using System.Collections.Generic;

namespace kinkaudiorender
{
    public class kinkaudiorender
    {

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
            int samplerate = Convert.ToInt32(args[1]);
            if (!samplerate % tickrate == 0 )
            {
                Console.WriteLine("BAD SAMPLERATE FOR THAT TICKRATE, BAKA");
                return 1;
            }
            Compiler.Compile(args[0], out var Envelopes, out var Wavcomms,
            out var musicCommands, out string metadata, out int tickrate);

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
                        for ( int i = 0; i < samplerate / tickrate; i++)
                        {
                            channelFakeTime++;
                            channelTime++;
                            if (!currentPitchEnv.Equals(string.Empty))
                            {
                                
                            }
                            if (!currentPitchWav.Equals(string.Empty))
                        }
                    }
                    else if ( command.Contains("retrig") )
                    {
                        for ( int i = 0; i < samplerate / tickrate; i++)
                        {
                            channelFakeTime++;
                            channelTime++;
                        }
                    }
                }
            }
        }
    }
}