using RandomizerMod.RC;

namespace CondensedSpoilerLogger.SpecificProgression
{
    /// <summary>
    /// Helper class that constructs a Calculator and a ProgressionBlockWriter from a RandoModContext
    /// for writing queries to a log.
    /// </summary>
    public class CalcWriter
    {
        private readonly RandoModContext _ctx;
        private readonly Calculator _calc;
        private readonly ProgressionBlockWriter _writer;

        public CalcWriter(RandoModContext ctx)
        {
            _calc = new(ctx, out _);
            _writer = new ProgressionBlockWriter(ctx);
        }

        public string ComputeProgressionString(string query, QueryType queryType)
        {
            if (queryType == QueryType.Unknown)
            {
                queryType = InferQueryType(query, _ctx);
            }
            ItemPlacement[] pmts = _calc.GetProgression(query, queryType);
            return _writer.WriteProgression(pmts, query, queryType);
        }

        public static QueryType InferQueryType(string query, RandoModContext ctx)
        {
            if (query.Contains("<"))
            {
                return QueryType.TermValue;
            }

            bool isTerm = ctx.LM.GetTerm(query) != null;
            bool isLogicDef = ctx.LM.GetLogicDef(query) != null;

            if (isTerm && !isLogicDef) return QueryType.Term;
            if (isLogicDef && !isTerm) return QueryType.LogicDef;

            return QueryType.Unknown;
        }
    }
}
