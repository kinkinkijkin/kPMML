using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace kinkaudio
{
    class LexDict
    {
        public readonly string[] notenames = new string[13] { "CN", "CS", "DN", "DS",
        "E", "FN", "FS", "GN", "GS", "AN", "AS", "B", "R" };
        public readonly float[] notevalues = new float[13] { 16.35f, 17.32f, 18.35f,
        19.45f, 20.60f, 21.83f, 23.12f, 24.50f, 25.96f, 27.50f, 29.14f, 30.87f, 0f };
        public readonly string[] commandnames = new string[7] { "p=", "P=", "o=",
        "va=", "vs=", "(", ")"};
        public readonly string[] commandqualities = new string[7] { "pitchEnv", 
        "pitchVibrato", "octaveSet", "vibSpeed", "vibAmplitude", "loopStart",
        "loopEnd" };
        public readonly char[] integers = new char[10] { '1', '2', '3', '4', '5',
        '6', '7', '8', '9', '0' };
        public List<string> envnames { get; set; }
        public List<string> envtypes { get; set; }
        public List<string> envalues { get; set; }
        public List<string> wavnames { get; set; }
        public List<string> wavtypes { get; set; }
        public List<string> wavalues { get; set; }
        public LexDict (List<string> envNames, List<string> envTypes,
        List<string> envValues, List<string> wavNames, List<string> wavTypes,
        List<string> wavValues)
        {
            envnames = envNames;
            envtypes = envTypes;
            envalues = envValues;
            wavnames = wavNames;
            wavtypes = wavTypes;
            wavalues = wavValues;
        }
    }
    public class Compiler
    {
        static List<string> StripComments ( List<string> inputList )
        {
            List<string> inputListCopy = new List<string>();
            inputListCopy.AddRange(inputList);
            foreach ( var line in inputList )
            {
                if ( line.StartsWith("#") )
                {
                    inputListCopy.Remove( line );
                }
            }
            return inputListCopy;
        }

        static void GetMacroBlock ( List<string> inputList, string[] blocks, 
        out List<string> outputNames, out List<string> outputTypes,
        out List<string> outputValues )
        {
            bool currentBlock = false;
            string currentMacro;
            outputNames = new List<string>();
            outputTypes = new List<string>();
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
                        currentMacro = line.Split( new [] { ' ' } )[1];
                        outputTypes.Add(currentMacro);
                        currentMacro = String.Join( " ", line.Split(
                            new [] { ' ' } ).ToList().GetRange(2, (line.Split(
                                new [] { ' ' } ).Length) - 2));
                        outputValues.Add(currentMacro);
                    }
                }
            }
        }
        static void GetMusicBlock ( List<string> inputList, 
        LexDict dictionary, out List<List<string>> musicCommands)
        {
            bool currentBlock = false;
            List<string> currentCommand = new List<string>();
            musicCommands = new List<List<string>>();
            musicCommands.Add(new List<string>());
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
                            channel = Convert.ToInt32(
                                currentLine[0].TrimStart(new []{ 'c' }));
                            if ( channel > totalChannels ) 
                            {
                                totalChannels = channel;
                                musicCommands.Add(new List<string>());
                            }
                        }
                        bool loop = false;
                        List<string> loopRange = new List<string>();
                        foreach ( var command in currentLine )
                        {
                            for ( int i = 0; i < dictionary.notenames.Length; i++ )
                            {
                                if ( command.Contains(dictionary.notenames[i]) )
                                {
                                    string newCommand = ( 
                                        Convert.ToString(dictionary.notevalues[i]) );
                                    if ( !command.Contains(">") )
                                    {
                                        currentCommand.Add("retrig " + newCommand);
                                        for ( int r = 1;
                                        r < Convert.ToInt32(command.TrimStart(
                                        dictionary.notenames[i].ToCharArray())); r++ )
                                        {
                                            currentCommand.Add("noRetrig " 
                                            + newCommand);
                                        }
                                    }
                                    else
                                    {
                                        for ( int r = 0;
                                        r < Convert.ToInt32(
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
                                    string newCommand = "envSet "
                                    + dictionary.envnames[i];
                                    currentCommand.Add(newCommand);
                                }
                            }
                            for ( int i = 0; i < dictionary.wavnames.Count; i++)
                            {
                                if ( command.Equals(dictionary.wavnames[i]) )
                                {
                                    string newCommand = "wavSet "
                                    + dictionary.wavnames[i];
                                    currentCommand.Add(newCommand);
                                }
                            }
                            if ( currentCommand.Contains("loopStart") )
                            {
                                loop = true;
                            }
                            else if ( currentCommand.Contains("loopEnd") )
                            {
                                for ( int i = 0; i < Convert.ToInt32(
                                    currentCommand[1]); i++)
                                {
                                    musicCommands[channel].AddRange(loopRange);
                                }
                                loop = false;
                            }
                            else if (loop)
                            {
                                loopRange.AddRange(currentCommand);
                            }
                            
                        }
                        musicCommands[channel].AddRange(currentCommand);
                    }
                }
            }
            musicCommands[0].Add(Convert.ToString(totalChannels));
        }
        static string SetMetadata ( List<string> inputList, string metadataName )
        {
            foreach ( var line in inputList )
            {
                if ( line.Contains(metadataName) )
                {
                    return line.Trim(metadataName.ToCharArray());
                }
                else {}
            }
            return "empty";
        }
        public static void Compile ( string inputFileName, out List<ChanEnv> envInfo,
        out List<ChanWav> wavInfo, out List<List<string>> commands,
        out string metadata, out int tickrate)
        {
            List<string> inputFile = new List<string>(
                File.ReadAllLines(inputFileName));
            inputFile = StripComments(inputFile);

            metadata = ( SetMetadata(inputFile, "artist=") + " - " 
            + SetMetadata(inputFile, "name=") );
            tickrate = Convert.ToInt32(SetMetadata(inputFile, "hz"));

            List<string> envMacros;
            List<string> envMacrosValues;
            List<string> envTypes;
            List<string> wavMacros;
            List<string> wavMacrosValues;
            List<string> wavTypes;

            GetMacroBlock(inputFile, new [] { "/env", "/wav" }, out envMacros,
            out envTypes, out envMacrosValues);

            GetMacroBlock(inputFile, new [] { "/wav", "/mu" }, out wavMacros,
            out wavTypes, out wavMacrosValues);

            LexDict dictionary = new LexDict(envMacros, envTypes, envMacrosValues,
            wavMacros, wavTypes, wavMacrosValues);

            GetMusicBlock(inputFile, dictionary, out commands);

            envInfo = new List<ChanEnv>();

            for ( int i = 0; i < dictionary.envnames.Count; i++ )
            {
                List<float> newEnvValues = new List<float>();
                foreach ( var value in dictionary.envalues[i].Split(
                    new [] { ' ' }) )
                {
                    newEnvValues.Add(Convert.ToSingle(value));
                }
                if ( newEnvValues.Count < 4 ) newEnvValues.Add(0f);
                ChanEnv newEnvInfo = new ChanEnv(dictionary.envnames[i], 
                dictionary.envtypes[i], newEnvValues);
                envInfo.Add(newEnvInfo);
            }

            wavInfo = new List<ChanWav>();

            for ( int i = 0; i < dictionary.wavnames.Count; i++ )
            {
                List<float> newWavValues = new List<float>();
                foreach ( var value in dictionary.wavalues[i].Split(
                    new [] { ' ' }) )
                {
                    newWavValues.Add(Convert.ToSingle(value));
                }
                if ( newWavValues.Count < 2 ) newWavValues.Add(0f);
                ChanWav newWavInfo = new ChanWav(dictionary.wavnames[i], 
                dictionary.wavtypes[i], newWavValues);
                wavInfo.Add(newWavInfo);
            }
        }
    }
    public struct ChanEnv
    {
        public string envName { get; set; }
        public string envType { get; set; }
        public List<float> envValues { get; set; }
        public ChanEnv ( string envname, string envtype, List<float> envvalues)
        {
            envName = envname;
            envType = envtype;
            envValues = envvalues;
        }
    }
    public struct ChanWav
    {
        public string wavName { get; set; }
        public string wavType { get; set; }
        public List<float> wavValues { get; set; }
        public ChanWav ( string wavname, string wavtype, List<float> wavvalues )
        {
            wavName = wavname;
            wavType = wavtype;
            wavValues = wavvalues;
        }
    }
}