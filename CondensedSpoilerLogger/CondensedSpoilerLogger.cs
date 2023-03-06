using Modding;
using MonoMod.ModInterop;
using RandomizerMod.Logging;
using CondensedSpoilerLogger.Loggers;
using System.Collections.Generic;
using RandomizerMod.RC;
using CondensedSpoilerLogger.Interop;

namespace CondensedSpoilerLogger
{
    public class CondensedSpoilerLogger : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static CondensedSpoilerLogger instance;

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings s)
        {
            GS = s;
            GS.WritableLogs ??= new();
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
        
        public static IEnumerable<CslLogger> CreateLoggers()
        {
            yield return new CondensedSpoilerLog();
            yield return new NotchCostSpoiler();
            yield return new AreaSpoilerLog();
            yield return new ItemGroupSpoiler();
            yield return new AreaTransitionSpoiler();
            yield return new ItemProgressionSpoiler();
#if DEBUG
            yield return new SpecificProgression.TestProgressionLogger();
#endif
        }

        public static void WriteCslLogs()
        {
            if (RandomizerMod.RandomizerMod.RS?.Context is not RandoModContext ctx)
            {
                DebugMod.LogToConsole("No rando context found");
                return;
            }

            LogArguments args = new()
            {
                randomizer = default,
                ctx = ctx,
                gs = ctx.GenerationSettings
            };

            new CslLogWriter().Log(args);
        }

        public override void Initialize()
        {
            Log("Initializing Mod...");

            ModHooks.FinishedLoadingModsHook += () => LogManager.AddLogger(new CslLogWriter());

            CslMenu.Hook();

            RBDebug.Hook();

            DebugMod.AddActionToKeyBindList(WriteCslLogs, "Write Csl Logs");
            DebugMod.AddActionToKeyBindList(SpecificProgression.QueryManager.MakeQueryFileLog, "Write Query Log");
        }
    }
}