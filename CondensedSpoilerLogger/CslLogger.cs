using RandomizerMod.Logging;
using System.Collections.Generic;

namespace CondensedSpoilerLogger
{
    public abstract class CslLogger : RandoLogger
    {

        protected abstract IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args);

        public sealed override void Log(LogArguments args)
        {
            foreach ((string text, string filename) in CreateLogTexts(args))
            {
                WriteLog(text, filename);
            }
        }

        /// <summary>
        /// Write the log, subject to it not having been disabled in the global settings.
        /// </summary>
        private void WriteLog(string text, string fileName)
        {
            if (!CondensedSpoilerLogger.GS.WrittenLogs.ContainsKey(fileName))
            {
                CondensedSpoilerLogger.GS.WrittenLogs.Add(fileName, true);
            }

            if (!CondensedSpoilerLogger.GS.WrittenLogs[fileName])
            {
                return;
            }

            LogManager.Write(text, fileName);
        }
    }
}
