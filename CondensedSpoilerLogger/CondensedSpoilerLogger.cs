using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using UnityEngine;
using RandomizerMod.Logging;

namespace CondensedSpoilerLogger
{
    public class CondensedSpoilerLogger : Mod
    {
        internal static CondensedSpoilerLogger instance;
        
        public CondensedSpoilerLogger() : base(null)
        {
            instance = this;
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();;
        }
        
        public override void Initialize()
        {
            Log("Initializing Mod...");

            List<RandoLogger> loggers = (List<RandoLogger>)typeof(LogManager)
                .GetField("loggers", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);

            loggers.Add(new CondensedSpoilerLog());
        }
    }
}