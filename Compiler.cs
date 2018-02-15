using System;
using System.IO;
using System.Collections;
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
        public readonly string[] commandnames = new string[5] { "p=", "P=", "o=",
        "va=", "vs="};
        public readonly string[] commandqualities = new string[5] { "pitchEnv", 
        "pitchVibrato", "octaveSet", "vibSpeed", "vibAmplitude"};
        public readonly char[] integers = new char[10] { '1', '2', '3', '4', '5',
        '6', '7', '8', '9', '0' };
        public List<string> envnames { get; set; }
        public List<string> envtypes { get; set; }
        public List<string> envalues { get; set; }
        public List<string> wavnames { get; set; }
        public List<string> wavtypes { get; set; }
        public List<string> wavalues { get; set; }
        public List<string> fmnames { get; set; }
        public List<string> fmtypes { get; set; }
        public List<string> fmvalues { get; set; }
        public LexDict (List<string> envNames, List<string> envTypes,
        List<string> envValues, List<string> wavNames, List<string> wavTypes,
        List<string> wavValues, List<string> fmNames, List<string> fmTypes,
        List<string> fmValues)
        {
            envnames = envNames;
            envtypes = envTypes;
            envalues = envValues;
            wavnames = wavNames;
            wavtypes = wavTypes;
            wavalues = wavValues;
            fmnames = fmNames;
            fmtypes = fmTypes;
            fmvalues = fmValues;
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
            musicCommands = new List<List<string>>();
            musicCommands.Add(new List<string>());
            int channel = 0;
            int totalChannels = 0;
            List<string[]> arbitraryMacros = new List<string[]>();
            foreach ( var line in inputList )
            {
                List<string> currentCommand = new List<string>();
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
                            channel = Int32.Parse(
                                currentLine[0].TrimStart(new []{ 'c' }));
                            if ( channel > totalChannels ) 
                            {
                                totalChannels = channel;
                                musicCommands.Add(new List<string>());
                            }
                        }
                        else
                        {
                            arbitraryMacros.Add(line.Split(new [] { ' ' }, 2));
                        }
                        bool loop = false;
                        List<string> loopRange = new List<string>();
                        List<string> loopTotal = new List<string>();
                        int loopEndIndex = 0;
                        foreach ( var command in currentLine )
                        {
                            if ( command.Contains ("(") )
                            {
                                loop = true;
                            }
                            if ( command.Contains (")") )
                            {
                                loop = false;
                                for ( int i = 0; i < Int32.Parse(
                                    command.TrimStart(')')); i++)
                                {
                                    loopTotal.AddRange(loopRange);
                                }
                            }
                            else if ( loop )
                            {
                                loopRange.Add(command);
                            }
                            else loopEndIndex++;
                        }
                        List<string> newCurrentLine = new List<string>();

                        newCurrentLine.AddRange(currentLine);
                        newCurrentLine.InsertRange(loopEndIndex, loopTotal);

                        int macroIndex = 0;
                        foreach ( var command in currentLine )
                        {
                            foreach ( var macro in arbitraryMacros )
                            {
                                if ( command.Contains(macro[0]) )
                                {
                                    newCurrentLine.InsertRange(macroIndex, macro[1]
                                        .Split(' '));
                                }
                            }
                            macroIndex++;
                        }
                        foreach ( var command in newCurrentLine )
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
                            for ( int i = 0; i < dictionary.fmnames.Count; i++)
                            {
                                if ( command.Equals(dictionary.fmnames[i]) )
                                {
                                    string newCommand = "fmSet "
                                    + dictionary.fmnames[i];
                                    currentCommand.Add(newCommand);
                                }
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
        out List<ChanWav> wavInfo, out List<ChanFM> fmInfo,
        out List<List<string>> commands, out string metadata, out int tickrate)
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
            List<string> fmMacros;
            List<string> fmMacrosValues;
            List<string> fmTypes;

            GetMacroBlock(inputFile, new [] { "/env", "/wav" }, out envMacros,
            out envTypes, out envMacrosValues);

            GetMacroBlock(inputFile, new [] { "/wav", "/fm" }, out wavMacros,
            out wavTypes, out wavMacrosValues);

            GetMacroBlock(inputFile, new [] { "/fm", "/mu" }, out fmMacros,
            out fmTypes, out fmMacrosValues);

            LexDict dictionary = new LexDict(envMacros, envTypes, envMacrosValues,
            wavMacros, wavTypes, wavMacrosValues, fmMacros, fmMacrosValues, fmTypes);

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
                    newWavValues.Add(Single.Parse(value));
                }
                if ( newWavValues.Count < 2 ) newWavValues.Add(0f);
                ChanWav newWavInfo = new ChanWav(dictionary.wavnames[i], 
                dictionary.wavtypes[i], newWavValues);
                wavInfo.Add(newWavInfo);
            }

            fmInfo = new List<ChanFM>();

            for ( int i = 0; i < dictionary.fmnames.Count; i++ )
            {
                string newEnvName = dictionary.fmvalues[i].Split(' ')[0];
                float newMult = Single.Parse(dictionary.fmvalues[i].Split(' ')[1]);
                int newTruncMod = Int32.Parse(dictionary.fmvalues[i].Split(' ')[2]);
                int newTruncCar = Int32.Parse(dictionary.fmvalues[i].Split(' ')[3]);
                int newInputChannel = Int32.Parse(
                    dictionary.fmvalues[i].Split(' ')[4]);
                fmInfo.Add(new ChanFM(dictionary.fmnames[i], dictionary.fmtypes[i],
                newEnvName, newMult, newTruncMod, newTruncCar, newInputChannel));
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
    public struct ChanFM
    {
        public string fmName { get; set; }
        public string fmType { get; set; }
        public string fmEnvName { get; set; }
        public float fmMult { get; set; }
        public int fmTruncMod { get; set; }
        public int fmTruncCar { get; set; }
        public int fmInputChannel { get; set; }
        public ChanFM ( string fmname, string type, string envname, float mult,
        int truncmod, int trunccar, int inputchannel)
        {
            fmName = fmname;
            fmType = type;
            fmEnvName = envname;
            fmMult = mult;
            fmTruncMod = truncmod;
            fmTruncCar = trunccar;
            fmInputChannel = inputchannel;
        }
    }
}