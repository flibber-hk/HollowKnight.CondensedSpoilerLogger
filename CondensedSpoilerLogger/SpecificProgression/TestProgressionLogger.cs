using RandomizerMod.Logging;
using System.Collections.Generic;
using System.Text;

namespace CondensedSpoilerLogger.SpecificProgression
{
    internal class TestProgressionLogger : CslLogger
    {

        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            CalcWriter cw = new(args.ctx);

            StringBuilder sb = new();

            sb.AppendLine($"Test progression log for seed {args.gs.Seed}");
            sb.AppendLine();
            sb.AppendLine();

            foreach ((string query, QueryType queryType) in new List<(string, QueryType)>()
            {
                ("LEFTCLAW", QueryType.Term),
                ("RIGHTDASH", QueryType.Term),
                ("LEFTDASH", QueryType.LogicDef),
                ("King_Fragment", QueryType.LogicDef),
                ("Mines_20[right1]", QueryType.LogicDef)
            })
            {
                sb.Append(cw.ComputeProgressionString(query, queryType));
                sb.AppendLine();
                sb.AppendLine();
            }

            yield return new(sb.ToString(), "TestLog.txt");
        }
    }
}
