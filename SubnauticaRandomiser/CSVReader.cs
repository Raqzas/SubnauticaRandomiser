﻿using System;
using System.Collections.Generic;
using System.IO;
using SMLHelper.V2.Crafting;
using UnityEngine;

namespace SubnauticaRandomiser
{
    internal static class CSVReader
    {
        private static string[] s_csvLines;
        internal static List<RandomiserRecipe> s_csvParsedList;
        internal static List<Databox> s_csvDataboxList;

        private static readonly int s_expectedColumns = 8;

        internal static List<RandomiserRecipe> ParseRecipeFile(string fileName)
        {
            // First, try to find and grab the file containing recipe information
            string path = InitMod.s_modDirectory + "\\" + fileName;
            LogHandler.Debug("Looking for recipe CSV as " + path);

            try
            {
                s_csvLines = File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                LogHandler.MainMenuMessage("Failed to read recipe CSV!");
                LogHandler.Error(ex.Message);
                return null;
            }

            // Second, read each line and try to parse that into a list of
            // RandomiserRecipe objects, for later use.
            // For now, this system is not robust and expects the CSV to be read
            // pretty much as it was distributed with the mod.
            s_csvParsedList = new List<RandomiserRecipe>();

            foreach (string line in s_csvLines)
            {
                if (line.StartsWith("TechType", StringComparison.InvariantCulture))
                {
                    // This is the header line. Skip.
                    continue;
                }

                // This might very well fail if the user messed with the CSV
                try
                {
                    s_csvParsedList.Add(ParseRecipeFileLine(line));
                }
                catch (Exception ex)
                {
                    LogHandler.Error("Failed to parse information from CSV!");
                    LogHandler.Error(ex.Message);
                }
            }

            return s_csvParsedList;
        }

        // Parse one line of a CSV file and attempt to create a RandomiserRecipe
        private static RandomiserRecipe ParseRecipeFileLine(string line)
        {
            RandomiserRecipe recipe = null;

            TechType type = TechType.None;
            List<RandomiserIngredient> ingredientList = new List<RandomiserIngredient>();
            ETechTypeCategory category = ETechTypeCategory.None;
            int depth = 0;
            List<TechType> prereqList = new List<TechType>();
            int value = 0;
            int maxUses = 0;

            Blueprint blueprint = null;
            List<TechType> blueprintUnlockConditions = new List<TechType>();
            TechType blueprintFragment = TechType.None;
            bool blueprintDatabox = false;
            int blueprintUnlockDepth = 0;

            string[] cells = line.Split(',');

            if (cells.Length != s_expectedColumns)
            {
                throw new InvalidDataException("Unexpected number of columns: " + cells.Length);
            }
            
            // Now to convert the data in each cell to an object we can use
            // Column 1: TechType
            if (String.IsNullOrEmpty(cells[0]))
            {
                throw new InvalidDataException("TechType is null or empty.");
            }
            type = StringToTechType(cells[0]);

            // Column 2: Category
            category = StringToETechTypeCategory(cells[1]);

            // Column 3: Depth Difficulty
            if (!String.IsNullOrEmpty(cells[2]))
            {
                depth = int.Parse(cells[2]);
            }

            // Column 4: Prerequisites
            if (!String.IsNullOrEmpty(cells[3]))
            {
                prereqList = ProcessMultipleTechTypes(cells[3].Split(';'));
            }

            // Column 5: Value
            if (!String.IsNullOrEmpty(cells[4]))
            {
                value = int.Parse(cells[4]);
            }

            // Column 6: Max Uses Per Game
            if (!String.IsNullOrEmpty(cells[5]))
            {
                maxUses = int.Parse(cells[5]);
            }

            // Column 7: Blueprint Unlock Conditions
            if (!String.IsNullOrEmpty(cells[6]))
            {
                string[] conditions = cells[6].Split(';');

                foreach (string str in conditions)
                {
                    if (str.ToLower().Contains("fragment"))
                    {
                        // HACK This code as-is will not handle the Cyclops properly
                        // but I feel like that one needs special care anyways.
                        blueprintFragment = StringToTechType(str);
                    } 
                    else if (str.ToLower().Contains("databox"))
                    {
                        blueprintDatabox = true;
                    }
                    else
                    {
                        blueprintUnlockConditions.Add(StringToTechType(str));
                    }
                }
            }

            // Column 8: Blueprint Unlock Depth
            if (!String.IsNullOrEmpty(cells[7]))
            {
                blueprintUnlockDepth = int.Parse(cells[7]);
            }
            
            // Only if any of the blueprint components yielded anything,
            // ship the recipe with a blueprint.
            if (blueprintUnlockConditions != null || blueprintUnlockDepth != 0 || !blueprintDatabox || !blueprintFragment.Equals(TechType.None))
            {
                blueprint = new Blueprint(type, blueprintUnlockConditions, blueprintFragment, blueprintDatabox, blueprintUnlockDepth);
            }
            
            LogHandler.Debug("Registering recipe: " + type.AsString() +" "+ category.ToString() +" "+ depth +" ... "+ value);
            recipe = new RandomiserRecipe(type, category, depth, prereqList, value, maxUses, blueprint);
            return recipe;
        }

        internal static List<Databox> ParseWreckageFile(string fileName)
        {
            string path = InitMod.s_modDirectory + "\\" + fileName;
            LogHandler.Debug("Looking for wreckage CSV as " + path);

            try
            {
                s_csvLines = File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                LogHandler.MainMenuMessage("Failed to read wreckage CSV!");
                LogHandler.Error(ex.Message);
                return null;
            }

            s_csvDataboxList = new List<Databox>();

            foreach (string line in s_csvLines)
            {
                if (line.StartsWith("TechType", StringComparison.InvariantCulture))
                {
                    // This is the header line. Skip.
                    continue;
                }

                // This might very well fail if the user messed with the CSV
                try
                {
                    Databox databox = ParseWreckageFileLine(line);
                    if (databox != null)
                        s_csvDataboxList.Add(databox);
                }
                catch (Exception ex)
                {
                    LogHandler.Error("Failed to parse information from CSV!");
                    LogHandler.Error(ex.Message);
                }
            }

            return s_csvDataboxList;
        }

        private static Databox ParseWreckageFileLine(string line)
        {
            Databox databox = null;

            TechType type = TechType.None;
            Vector3 coordinates = Vector3.zero;
            EWreckage wreck = EWreckage.None;
            bool laserCutter = false;
            bool propulsionCannon = false;

            string[] cells = line.Split(',');

            if (cells.Length != 6)
                throw new InvalidDataException("Unexpected number of columns: " + cells.Length);

            // Column 1: TechType
            if (String.IsNullOrEmpty(cells[0]))
                throw new InvalidDataException("TechType is null or empty.");
            type = StringToTechType(cells[0]);

            // Column 2: Coordinates
            if (!String.IsNullOrEmpty(cells[1]))
            {
                string[] str = cells[1].Split(';');
                if (str.Length != 3)
                    throw new InvalidDataException("Coordinates are invalid.");

                float x = float.Parse(str[0]);
                float y = float.Parse(str[1]);
                float z = float.Parse(str[2]);
                coordinates = new Vector3(x, y, z);
            }
            else
            {
                // The only reason this should be empty is if it is a fragment
                // For now, skip those.
                return null;
            }

            // Column 3: General location
            if (!String.IsNullOrEmpty(cells[2]))
            {
                wreck = StringToEWreckage(cells[2]);
            }

            // Column 4: Is it a databox?
            // Redundant until fragments are implemented.

            // Column 5: Does it need a laser cutter?
            if (!String.IsNullOrEmpty(cells[4]))
            {
                laserCutter = (int.Parse(cells[4]) == 1 ? true : false);
            }

            // Column 6: Does it need a propulsion cannon?
            if (!String.IsNullOrEmpty(cells[5]))
            {
                propulsionCannon = (int.Parse(cells[5]) == 1 ? true : false);
            }

            LogHandler.Debug("Registering databox: " + type + ", " + coordinates.ToString() + ", " + wreck.ToString() + ", " + laserCutter + ", " + propulsionCannon);
            databox = new Databox(type, coordinates, wreck, laserCutter, propulsionCannon);

            return databox;
        }

        internal static List<TechType> ProcessMultipleTechTypes(string[] str)
        {
            List<TechType> output = new List<TechType>();

            foreach (string s in str)
            {
                if (!String.IsNullOrEmpty(s))
                {
                    TechType t = StringToTechType(s);
                    output.Add(t);
                }
            }

            return output;
        }

        internal static TechType StringToTechType(string str)
        {
            TechType type;

            try
            {
                type = (TechType)Enum.Parse(typeof(TechType), str, true);
            }
            catch (Exception ex)
            {
                LogHandler.Error("Failed to parse string to TechType: " + str);
                LogHandler.Error(ex.Message);
                type = TechType.None;
            }

            return type;
        }

        internal static ETechTypeCategory StringToETechTypeCategory(string str)
        {
            ETechTypeCategory type;

            try
            {
                type = (ETechTypeCategory)Enum.Parse(typeof(ETechTypeCategory), str, true);
            }
            catch (Exception ex)
            {
                LogHandler.Error("Failed to parse string to ETechTypeCategory: " + str);
                LogHandler.Error(ex.Message);
                type = ETechTypeCategory.None;
            }

            return type;
        }

        internal static EProgressionNode StringToEProgressionNode(string str)
        {
            EProgressionNode node;

            try
            {
                node = (EProgressionNode)Enum.Parse(typeof(EProgressionNode), str, true);
            }
            catch (Exception ex)
            {
                LogHandler.Error("Failed to parse string to EProgressionNode: " + str);
                LogHandler.Error(ex.Message);
                node = EProgressionNode.None;
            }

            return node;
        }

        internal static EWreckage StringToEWreckage(string str)
        {
            EWreckage wreck;

            try
            {
                wreck = (EWreckage)Enum.Parse(typeof(EWreckage), str, true);
            }
            catch (Exception ex)
            {
                LogHandler.Error("Failed to parse string to EWreckage: " + str);
                LogHandler.Error(ex.Message);
                wreck = EWreckage.None;
            }

            return wreck;
        }
    }
}
