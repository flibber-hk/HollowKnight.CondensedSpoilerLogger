using ItemChanger;
using ItemChanger.Modules;
using MenuChanger;
using CacheDict = System.Collections.Generic.Dictionary<(string, CondensedSpoilerLogger.SpecificProgression.QueryType), string>;

namespace CondensedSpoilerLogger.SpecificProgression
{
    public class QueryCache : Module
    {
        public override void Initialize() { }
        public override void Unload() { }

        public CacheDict Cache;

        /// <summary>
        /// Thread safe - record the given query result
        /// </summary>
        public static void Record(string query, QueryType queryType, string message)
        {
            ThreadSupport.BeginInvoke(() =>
            {
                if (ItemChangerMod.Modules?.GetOrAdd<QueryCache>() is QueryCache qc)
                {
                    qc.Cache[(query, queryType)] = message;
                }
            });
        }

        public static CacheDict GetCache()
        {
            CacheDict cache = ThreadSupport.BlockUntilInvoked<CacheDict>(() =>
            {
                if (ItemChangerMod.Modules?.GetOrAdd<QueryCache>() is QueryCache qc)
                {
                    return new(qc.Cache);
                }

                return null;
            });

            return cache;
        }
    }
}
