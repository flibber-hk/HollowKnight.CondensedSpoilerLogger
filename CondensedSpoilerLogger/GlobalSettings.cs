using System.Collections.Generic;

namespace CondensedSpoilerLogger
{
    public class GlobalSettings
    {
        public Dictionary<string, bool> WrittenLogs = new();

        public void LoadFrom(GlobalSettings s)
        {
            if (s.WrittenLogs != null)
            {
                WrittenLogs = s.WrittenLogs;
            }
        }
    }
}
