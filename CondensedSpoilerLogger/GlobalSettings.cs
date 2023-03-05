using System.Collections.Generic;

namespace CondensedSpoilerLogger
{
    public class GlobalSettings
    {
        public Dictionary<string, bool> WritableLogs = new();
        public bool DisplayWriteLogsButton = true;
    }
}
