﻿using System;
using BepInEx.Logging;
using SubnauticaRandomiser.Interfaces;
namespace SubnauticaRandomiser
{
    /// A class for handling all the logging that the main program might ever want to do.
    /// Also includes main menu messages for relaying information to the user directly.
    [Serializable]
    internal class LogHandler : ILogHandler
    {
        private readonly ManualLogSource _log;

        public LogHandler()
        {
            _log = Logger.CreateLogSource("Randomiser");
        }
        
        public void Debug(string message)
        {
            _log.LogDebug(message);
        }
        
        public void Info(string message)
        {
            _log.LogInfo(message);
        }

        public void Warn(string message)
        {
            _log.LogWarning(message);
        }

        public void Error(string message)
        {
            _log.LogError(message);
        }

        public void Fatal(string message)
        {
            _log.LogFatal(message);
        }

        /// Send a message through QModManager's main menu system.
        public void MainMenuMessage(string message)
        {
            _log.LogMessage("Main Menu Message: " + message);
            //QModServices.Main.AddCriticalMessage(message);
        }
    }
}
