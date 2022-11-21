using Modding;
using MonoMod.ModInterop;
using RandomizerMod.Logging;
using CondensedSpoilerLogger.Loggers;

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
            LogManager.AddLogger(new ItemGroupSpoiler());
            LogManager.AddLogger(new ItemProgressionSpoiler());
            LogManager.AddLogger(new AreaTransitionSpoiler());
        }
    }
}