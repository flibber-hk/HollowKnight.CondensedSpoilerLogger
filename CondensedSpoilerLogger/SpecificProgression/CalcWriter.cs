using RandomizerMod.RC;

namespace CondensedSpoilerLogger.SpecificProgression
{
    /// <summary>
    /// Helper class that constructs a Calculator and a ProgressionBlockWriter from a RandoModContext
    /// for writing queries to a log.
    /// </summary>
    public class CalcWriter
    {
        private readonly Calculator _calc;
        private readonly ProgressionBlockWriter _writer;

        public CalcWriter(RandoModContext ctx)
        {
            _calc = new(ctx, out _);
            _writer = new ProgressionBlockWriter(ctx);
        }

        public string ComputeProgressionString(string query, QueryType queryType)
        {
            ItemPlacement[] pmts = _calc.GetProgression(query, queryType);
            return _writer.WriteProgression(pmts, query, queryType);
        }
    }
}
