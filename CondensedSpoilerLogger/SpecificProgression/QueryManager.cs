using Modding;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CacheDict = System.Collections.Generic.Dictionary<(string, CondensedSpoilerLogger.SpecificProgression.QueryType), string>;

namespace CondensedSpoilerLogger.SpecificProgression
{
    public static class QueryManager
    {
        private static readonly ILogger _logger = new SimpleLogger("CondensedSpoilerLogger.QueryManager");

        public static void MakeQueryFileLog()
        {
            List<(string, QueryType)> queries = LoadQueriesFromFile();
            if (queries is null)
            {
                _logger.LogWarn("File not found");
                return;
            }

            _logger.Log($"Found {queries.Count} queries");

            LogManager.Write(tw =>
            {
                List<string> logs = MakeQueries(queries);
                if (logs is null)
                {
                    _logger.LogError("Error performing queries");
                    return;
                }

                tw.Write($"Query Log for seed {RandomizerMod.RandomizerMod.RS.GenerationSettings.Seed}\n\n\n");
                foreach (string block in logs)
                {
                    if (block is null) continue;
                    tw.Write(block);
                    tw.Write("\n\n");
                }
            }, "QueryLog.txt");
        }

        public static List<(string, QueryType)> LoadQueriesFromFile(string filename = "CslQuery.txt")
        {
            string path = DeterminePath(filename);

            if (path == null) return null;


            List<(string, QueryType)> queries = new();
            foreach (string rawLine in File.ReadLines(path))
            {
                string line = Regex.Replace(rawLine, @"(#|//).*", "").Trim();
                if (!string.IsNullOrEmpty(line)) continue;

                if (!line.Contains(" "))
                {
                    queries.Add((line, QueryType.Unknown));
                    continue;
                }

                string[] pieces = line.Split(' ');
                if (pieces.Length != 2)
                {
                    _logger.LogWarn($"Ignoring line \"{line}\" because it has too many parts");
                    continue;
                }
                string query = pieces[0];
                if (!Enum.TryParse(pieces[1], out QueryType queryType))
                {
                    _logger.LogWarn($"Ignoring line \"{line}\": unrecognised query type");
                    continue;
                }
                queries.Add((query, queryType));
            }

            return queries;
        }

        private static string DeterminePath(string filename)
        {
            string path;

            path = Path.Combine(LogManager.UserDirectory, filename);
            if (File.Exists(path))
            {
                return path;
            }

            path = Path.Combine(LogManager.RecentDirectory, filename);
            if (File.Exists(path))
            {
                return path;
            }

            return null;
        }

        public static List<string> MakeQueries(List<(string query, QueryType queryType)> queries)
        {
            RandoModContext ctx = RandomizerMod.RandomizerMod.RS?.Context;
            if (ctx is null) return null;

            for (int i = 0; i < queries.Count; i++)
            {
                (string query, QueryType queryType) = queries[i];
                
                if (queryType == QueryType.Unknown)
                {
                    queryType = CalcWriter.InferQueryType(query, ctx);
                    queries[i] = (query, queryType);
                }
            }

            List<string> output = new();
            CacheDict cache = QueryCache.GetCache();

            CalcWriter writer = null;

            foreach (var key in queries)
            {
                if (cache.TryGetValue(key, out string cachedResult))
                {
                    output.Add(cachedResult);
                    continue;
                }

                writer ??= new(ctx);
                string result = writer.ComputeProgressionString(key.query, key.queryType);
                output.Add(result);
                QueryCache.Record(key.query, key.queryType, result);
            }

            return output;
        }
    }
}
