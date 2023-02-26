using Modding;
using MonoMod.ModInterop;
using RandomizerMod.Logging;
using CondensedSpoilerLogger.Loggers;

namespace CondensedSpoilerLogger
{
    public class CondensedSpoilerLogger : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static CondensedSpoilerLogger instance;

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings s)
        {
            GS = s;
            GS.WrittenLogs ??= new();
        }
        public GlobalSettings OnSaveGlobal() => GS;


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
            LogManager.AddLogger(new AreaTransitionSpoiler());
            // We really want this logger to be run last, because it's the slowest
            ModHooks.FinishedLoadingModsHook += () => LogManager.AddLogger(new ItemProgressionSpoiler());

            CslMenu.Hook();

            RBDebug.Hook();
        }
    }
}