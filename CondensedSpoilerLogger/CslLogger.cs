using RandomizerMod.Logging;

namespace CondensedSpoilerLogger
{
    public abstract class CslLogger : RandoLogger
    {
        /// <summary>
        /// Write the log, subject to it not having been disabled in the global settings.
        /// </summary>
        protected void WriteLog(string text, string fileName)
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
