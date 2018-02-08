using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace kinkaudio
{
    class LexDict
    {
        public readonly string[] notenames = new string[12] { "CN", "CS", "DN", "DS",
        "E", "FN", "FS", "GN", "GS", "AN", "AS", "B" };
        public readonly float[] notevalues = new float[12] { 16.35f, 17.32f, 18.35f,
        19.45f, 20.60f, 21.83f, 23.12f, 24.50f, 25.96f, 27.50f, 29.14f, 30.87f };
        public readonly string[] commandnames = new string[7] { "p=", "P=", "o=",
        "va=", "vs=", "(", ")"};
        public readonly string[] commandqualities = new string[7] { "pitchEnv", 
        "pitchVibrato", "octaveSet", "vibSpeed", "vibAmplitude", "loopStart",
        "loopEnd" };
        public List<string> envnames { get; set; }
        public List<string> envalues { get; set; }
        public List<string> wavnames { get; set; }
        public List<string> wavalues { get; set; }
        public LexDict (List<string> field1, List<string>field2,
        List<string> field3, List<string> field4)
        {
            envnames = field1;
            envalues = field2;
            wavnames = field3;
            wavalues = field4;
        }
    }
    public class Lexer
    {
        public static List<string> StripComments ( List<string> inputList )
        {
            var inputListCopy = inputList;
            foreach ( var line in inputList )
            {
                if ( line.StartsWith("#") )
                {
                    inputListCopy.Remove( line );
                }
            }
            return inputListCopy;
        }

        private void GetMacroBlock ( List<string> inputList, string[] blocks, 
        out List<string> outputNames, out List<string> outputValues )
        {
            bool currentBlock = false;
            string currentMacro;
            outputNames = new List<string>();
            outputValues = new List<string>();
            foreach ( var line in inputList )
            {
                if (!currentBlock)
                {
                    if ( line.Contains(blocks[0]) ) currentBlock = true;
                }
                else
                {
                    if ( line.Contains(blocks[1]) ) currentBlock = false;
                    else
                    {
                        currentMacro = line.Split( new [] { ' ' } )[0];
                        outputNames.Add(currentMacro);
                        currentMacro = String.Join( " ", line.Split(
                            new [] { ' ' } ).ToList().GetRange(1, (line.Split(
                                new [] { ' ' } ).Length) - 1));
                        outputValues.Add(currentMacro);
                    }
                }
            }
        }
        private void GetMusicBlock ( List<string> inputList, 
        LexDict dictionary, out List<List<string>> musicCommands)
        {
            bool currentBlock = false;
            List<string> currentCommand = new List<string>();
            musicCommands = new List<List<string>>();
            int channel = 0;
            int totalChannels = 0;
            foreach ( var line in inputList )
            {
                if (!currentBlock)
                {
                    if ( line.StartsWith("/mu") ) currentBlock = true;
                }
                else
                {
                    if ( line.StartsWith("END") ) currentBlock = false;
                    else if ( line.StartsWith("/mu") ) currentBlock = true;
                    else
                    {
                        string[] currentLine = line.Split(new [] { ' ' });
                        if ( currentLine[0].Contains("c") )
                        {
                            channel = Convert.ToInt16(
                                currentLine[0].TrimStart(new []{ 'c' }));
                            if ( channel > totalChannels ) totalChannels = channel;
                        }
                        foreach ( var command in currentLine )
                        {
                            for ( int i = 0; i < dictionary.notenames.Length; i++ )
                            {
                                if ( command.Contains(dictionary.notenames[i]) )
                                {
                                    string newCommand = ( dictionary.notevalues[i]
                                    + " "
                                    + command.TrimStart('>').TrimStart(
                                        dictionary.notenames[i].ToCharArray()) );
                                    if ( !command.Contains(">") )
                                    {
                                        currentCommand.Add("retrig " + newCommand);
                                        for ( int r = 1;
                                        r < Convert.ToInt16(command.TrimStart(
                                        dictionary.notenames[i].ToCharArray())); r++ )
                                        {
                                            currentCommand.Add("noRetrig " 
                                            + newCommand);
                                        }
                                    }
                                    else
                                    {
                                        for ( int r = 0;
                                        r < Convert.ToInt16(
                                            command.TrimStart('>').TrimStart(
                                        dictionary.notenames[i].ToCharArray())); r++ )
                                        {
                                            currentCommand.Add("noRetrig " 
                                            + newCommand);
                                        }
                                    }

                                }
                            }
                            for ( int i = 0; i < dictionary.commandnames.Length; i++ )
                            {
                                if ( command.Contains( dictionary.commandnames[i]) )
                                {
                                    string newCommand = ( 
                                        dictionary.commandqualities[i] + " "
                                        + command.TrimStart(
                                            dictionary.commandnames[i].ToCharArray())
                                         );
                                    currentCommand.Add(newCommand);
                                }
                            }
                            for ( int i = 0; i < dictionary.envnames.Count; i++)
                            {
                                if ( command.Equals(dictionary.envnames[i]) )
                                {
                                    string newCommand = "envSet"
                                    + dictionary.envalues[i];
                                    currentCommand.Add(newCommand);
                                }
                            }
                            for ( int i = 0; i < dictionary.wavnames.Count; i++)
                            {
                                if ( command.Equals(dictionary.wavnames[i]) )
                                {
                                    string newCommand = "wavSet"
                                    + dictionary.wavalues[i];
                                    currentCommand.Add(newCommand);
                                }
                            }
                        }
                        musicCommands[channel].AddRange(currentCommand);
                    }
                }
            }
            musicCommands[0].Add("{totalChannels}");
        }
    }
}