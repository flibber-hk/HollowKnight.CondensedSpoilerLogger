using CondensedSpoilerLogger.Util;
using RandomizerMod.Logging;
using System.Collections.Generic;
using System.Linq;

namespace CondensedSpoilerLogger
{
    internal class CslLogWriter : RandoLogger
    {
        public override void Log(LogArguments args)
        {
            IEnumerator<(string text, string filename)> logData = CondensedSpoilerLogger.CreateLoggers()
                .Select(log => log.GetLogTexts(args).GetEnumerator())
                .Chain();

            if (!logData.MoveNext()) return;
            MakeLogRequest(logData.Current.text, logData.Current.filename, logData);
        }

        private void MakeLogRequest(string text, string filename, IEnumerator<(string text, string filename)> remaining)
        {
            LogManager.Write(
                tw =>
                {
                    CondensedSpoilerLogger.instance.LogDebug($"Writing Csl log to {filename}");
                    tw.Write(text);
                    if (!remaining.MoveNext())
                    {
                        CondensedSpoilerLogger.instance.Log($"Completed logging");
                        remaining?.Dispose();
                        return;
                    }
                    MakeLogRequest(remaining.Current.text, remaining.Current.filename, remaining);
                }, filename);
        }
    }
}
