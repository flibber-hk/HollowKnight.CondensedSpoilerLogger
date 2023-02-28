using HutongGames.PlayMaker.Actions;
using RandomizerMod.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CondensedSpoilerLogger
{
    public abstract class CslLogger
    {
        protected abstract IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args);

        public IEnumerable<(string text, string filename)> GetLogTexts(LogArguments args) => CanWrite() 
            ? CreateLogTexts(args) 
            : Enumerable.Empty<(string, string)>();

        public string Name { get; }
        public CslLogger()
        {
            Name = GetType().Name;
        }

        internal bool CanWrite()
        {
            if (!CondensedSpoilerLogger.GS.WrittenLogs.ContainsKey(Name))
            {
                CondensedSpoilerLogger.GS.WrittenLogs.Add(Name, true);
            }

            if (!CondensedSpoilerLogger.GS.WrittenLogs[Name])
            {
                return false;
            }

            return true;
        }
    }
}
