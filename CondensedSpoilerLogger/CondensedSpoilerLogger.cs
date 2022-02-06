using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using MonoMod.ModInterop;
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
            typeof(API.Export).ModInterop();
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        
        public override void Initialize()
        {
            Log("Initializing Mod...");

            LogManager.AddLogger(new CondensedSpoilerLog());
            LogManager.AddLogger(new NotchCostSpoiler());
            LogManager.AddLogger(new AreaSpoilerLog());
        }
    }
}