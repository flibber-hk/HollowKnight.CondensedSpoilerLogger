using RandomizerMod.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CondensedSpoilerLogger
{
    public abstract class CslLogger
    {
        protected abstract IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args);

        public IEnumerable<(string text, string filename)> GetLogTexts(LogArguments args) => CreateLogTexts(args).Where(pair => CanWrite(pair.filename));

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
        internal static void CreateLogAction(string text, string fileName)
        {
            if (!CanWrite(fileName)) return;

            LogManager.Write(text, fileName);
        }

        internal static bool CanWrite(string fileName)
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
