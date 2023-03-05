using RandomizerMod.Logging;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CondensedSpoilerLogger.SpecificProgression
{
    internal class TestProgressionLogger : CslLogger
    {

        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            Calculator calc = new(args.ctx, out _);
            ProgressionBlockWriter writer = new(args.ctx);

            StringBuilder sb = new();

            sb.AppendLine($"Test progression log for seed {args.gs.Seed}");
            sb.AppendLine();
            sb.AppendLine();

            foreach ((string query, QueryType queryType) in new List<(string, QueryType)>()
            {
                ("LEFTCLAW", QueryType.Term),
                ("RIGHTDASH", QueryType.Term),
                ("King_Fragment", QueryType.LogicDef),
                ("Mines_20[right1]", QueryType.LogicDef)
            })
            {
                ItemPlacement[] pmts = calc.GetProgression(query, queryType);
                sb.Append(writer.WriteProgression(pmts, query, queryType));
                sb.AppendLine();
                sb.AppendLine();
            }

            yield return new(sb.ToString(), "TestLog.txt");
        }
    }
}
