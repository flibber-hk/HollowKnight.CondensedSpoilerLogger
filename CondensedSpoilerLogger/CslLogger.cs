using RandomizerMod.Logging;
using System.Collections.Generic;
using System.IO;

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

        internal void MakeLogRequests(LogArguments args)
        {
            foreach ((string text, string filename) in CreateLogTexts(args))
            {
                if (!CanWrite(filename)) return;

                LogManager.Write(tw => tw.Write(text), filename);
            }
        }

        /// <summary>
        /// Write the log, subject to it not having been disabled in the global settings.
        /// </summary>
        private void WriteLog(string text, string fileName)
        {
            if (!CanWrite(fileName)) return;

            LogManager.Write(text, fileName);
        }

        private static bool CanWrite(string fileName)
        {
            if (!CondensedSpoilerLogger.GS.WrittenLogs.ContainsKey(fileName))
            {
                CondensedSpoilerLogger.GS.WrittenLogs.Add(fileName, true);
            }

            if (!CondensedSpoilerLogger.GS.WrittenLogs[fileName])
            {
                return false;
            }

            return true;
        }
    }
}
