using System.Collections.Generic;

namespace CondensedSpoilerLogger
{
    public class GlobalSettings
    {
        public Dictionary<string, bool> WrittenLogs = new();
        public bool DisplayWriteLogsButton = true;
    }
}
