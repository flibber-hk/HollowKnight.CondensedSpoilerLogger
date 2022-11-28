using Modding;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CondensedSpoilerLogger
{
    public static class RBDebug
    {
        private static readonly ILogger _log = new SimpleLogger("CondensedSpoilerLogger.RBDebug");
        private static int _indent = 0;
        private static void AddIndent() => _indent++;
        private static void DeIndent() => _indent--;
        private static string IndentString
        {
            get
            {
                if (_indent == 0) return "";

                return new string(' ', 2*(_indent-1)) + "- ";
            }
        }
        private static void Log() => _log.LogDebug("");
        private static void Log(string msg) => _log.LogDebug($"{IndentString}{msg}");
        private static void Log(object msg) => _log.LogDebug($"{IndentString}{msg}");

        private static readonly Random rng = new();
        private static string SelectAny(this Bucket<string> bucket)
        {
            try
            {
                return bucket.ToWeightedArray().Next(rng);
            }
            catch (IndexOutOfRangeException)
            {
                return "BUCKET EMPTY";
            }
        }


        internal static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(float.MaxValue, InvokeLogRBState);
        }

        private static void InvokeLogRBState(RequestBuilder rb)
        {
            if (ModHooks.GlobalSettings.LoggingLevel > LogLevel.Debug) return;

            if (_indent != 0)
            {
                _log.LogWarn($"indent = {_indent}; should be 0");
                _indent = 0;
            }

            try
            {
                LogRBState(rb);
            }
            catch (Exception ex)
            {
                _log.LogError(ex);
            }
            
        }

        private static void LogRBState(RequestBuilder rb)
        {
            AddIndent();

            foreach (StageBuilder sb in rb.Stages)
            {
                Log($"STAGE {sb.label}");
                foreach (GroupBuilder gb in sb.Groups)
                {
                    AddIndent();
                    Log($"GROUP {gb.label} OF TYPE {gb.GetType().Name}");
                    LogInfoForGroup(gb);
                    DeIndent();
                }
            }

            DeIndent();

        }

        private static void LogInfoForGroup(GroupBuilder gb)
        {
            AddIndent();

            if (gb is ItemGroupBuilder igb)
            {
                Log($"{igb.Items.GetTotal()} items ({igb.Items.EnumerateDistinct().Count()} distinct)");
                Log($"Sample item: {igb.Items.SelectAny()}");
                Log($"{igb.Locations.GetTotal()} locations ({igb.Locations.EnumerateDistinct().Count()} distinct)");
                Log($"Sample location: {igb.Locations.SelectAny()}");
            }
            else if (gb is TransitionGroupBuilder tgb)
            {
                Log($"{tgb.Sources.GetTotal()} sources");
                Log($"Sample source: {tgb.Sources.SelectAny()}");
                Log($"{tgb.Targets.GetTotal()} targets");
                Log($"Sample target: {tgb.Targets.SelectAny()}");
            }
            else if (gb is SelfDualTransitionGroupBuilder sdtgb)
            {
                Log($"{sdtgb.Transitions.GetTotal()} transitions");
                Log($"Sample transition: {sdtgb.Transitions.SelectAny()}");
                Log($"Coupled: {sdtgb.coupled}");
            }
            else if (gb is SymmetricTransitionGroupBuilder stgb)
            {
                Log($"Reverse Label {stgb.reverseLabel}");
                Log($"{stgb.Group1.GetTotal()} in group 1");
                Log($"Sample member: {stgb.Group1.SelectAny()}");
                Log($"{stgb.Group2.GetTotal()} in group 2");
                Log($"Sample member: {stgb.Group2.SelectAny()}");
                Log($"Coupled: {stgb.coupled}");
            }

            DeIndent();
        }
    }
}
