using System;
using System.Collections.Generic;
using System.Linq;
using SubnauticaRandomiser.RandomiserObjects;
using Random = System.Random;

namespace SubnauticaRandomiser.Logic
{
    internal class AlternateStartLogic
    {
        private readonly Dictionary<EBiomeType, List<float[]>> _alternateStarts;
        private readonly CoreLogic _logic;

        private RandomiserConfig _config => _logic._config;
        private EntitySerializer _masterDict => _logic._masterDict;
        private Random _random => _logic._random;

        internal AlternateStartLogic(CoreLogic logic, Dictionary<EBiomeType, List<float[]>> alternateStarts)
        {
            _logic = logic;
            _alternateStarts = alternateStarts;
        }

        internal void Randomise()
        {
            _masterDict.StartPoint = GetRandomStart();
        }

        /// <summary>
        /// Convert the config value to a usable biome.
        /// </summary>
        /// <returns>The biome.</returns>
        private EBiomeType GetBiome()
        {
            switch (_config.sSpawnPoint)
            {
                case "Random":
                    return _logic.GetRandom(_alternateStarts.Keys.ToList());
                case "BulbZone":
                    return EBiomeType.KooshZone;
                case "Floating Island":
                    return EBiomeType.FloatingIsland;
                case "Void":
                    return EBiomeType.None;
            }

            return (EBiomeType)Enum.Parse(typeof(EBiomeType), _config.sSpawnPoint);
        }

        /// <summary>
        /// Find a suitable random spawn point for the lifepod.
        /// </summary>
        /// <returns>The new spawn point.</returns>
        private RandomiserVector GetRandomStart()
        {
            if (_config.sSpawnPoint.StartsWith("Vanilla"))
                return null;
            
            EBiomeType biome = GetBiome();
            if (!_alternateStarts.ContainsKey(biome))
            {
                LogHandler.Error("No information found on chosen starting biome " + biome);
                return new RandomiserVector(0, 0, 0);
            }

            // Choose one of the possible spawning boxes within the biome.
            int boxIdx = _random.Next(0, _alternateStarts[biome].Count);
            float[] box = _alternateStarts[biome][boxIdx];
            // Choose the specific spawn point within the box.
            int x = _random.Next((int)box[0], (int)box[2] + 1);
            int z = _random.Next((int)box[3], (int)box[1] + 1);

            LogHandler.Debug("Chosen new lifepod spawnpoint at x:" + x + " y:0" + " z:" + z);
            return new RandomiserVector(x, 0, z);
        }
    }
}