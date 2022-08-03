using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace CondensedSpoilerLogger
{
    public static class Translation
    {
        private static Dictionary<string, string> _translationData;

        static Translation()
        {
            string modDirectory = Path.GetDirectoryName(typeof(Translation).Assembly.Location);
            string translationFile = Path.Combine(modDirectory, "translation.json");

            if (!File.Exists(translationFile))
            {
                _translationData = new();
                return;
            }

            using FileStream fs = File.OpenRead(translationFile);
            using StreamReader sr = new(fs);
            using JsonTextReader jtr = new(sr);
            JsonSerializer serializer = new()
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };
            _translationData = serializer.Deserialize<Dictionary<string, string>>(jtr);
        }

        public static string Translate(string orig) => _translationData.TryGetValue(orig, out string trans) ? trans : orig;
    }
}
