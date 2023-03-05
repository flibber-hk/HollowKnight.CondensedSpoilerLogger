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
            if (!CondensedSpoilerLogger.GS.WritableLogs.ContainsKey(Name))
            {
                CondensedSpoilerLogger.GS.WritableLogs.Add(Name, true);
            }

            if (!CondensedSpoilerLogger.GS.WritableLogs[Name])
            {
                return false;
            }

            return true;
        }
    }
}
