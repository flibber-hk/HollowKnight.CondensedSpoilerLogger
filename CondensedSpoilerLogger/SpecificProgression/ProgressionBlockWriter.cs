using RandomizerMod.RC;
using System.Text;

namespace CondensedSpoilerLogger.SpecificProgression
{
    public class ProgressionBlockWriter
    {
        private readonly SpoilerReader _sr;
        
        public ProgressionBlockWriter(RandoModContext ctx)
        {
            _sr = new(ctx);
        }

        public string WriteProgression(ItemPlacement[] progression, string query, QueryType queryType)
        {
            if (progression is null)
            {
                return $"Progression for {query} of type {queryType}: Unknown\n";
            }

            StringBuilder sb = new();

            string header = queryType switch
            {
                QueryType.Term => $"Progression for term {query}",
                QueryType.TermValue => $"Progression for term {query}",
                QueryType.LogicDef => $"Progression for logic def {query}",
                _ => $"Progression for {query}"
            };
            string line = new('-', header.Length);
            sb.AppendLine(header);
            sb.AppendLine(line);

            foreach (ItemPlacement pmt in progression)
            {
                _sr.AddPlacementToStringBuilder(sb, pmt);
            }

            return sb.ToString();
        }
    }
}
