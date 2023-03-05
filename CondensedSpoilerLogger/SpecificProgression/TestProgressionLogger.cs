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

            SpoilerReader sr = new(args.ctx);
            StringBuilder sb = new();

            sb.AppendLine($"Test progression log for seed {args.gs.Seed}");
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("LEFTCLAW");
            foreach (ItemPlacement pmt in calc.GetProgressionForTerm("LEFTCLAW"))
            {
                sr.AddPlacementToStringBuilder(sb, pmt);
            }
            sb.AppendLine();

            sb.AppendLine("RIGHTDASH");
            foreach (ItemPlacement pmt in calc.GetProgressionForTerm("RIGHTDASH"))
            {
                sr.AddPlacementToStringBuilder(sb, pmt);
            }
            sb.AppendLine();

            sb.AppendLine("King_Fragment");
            foreach (ItemPlacement pmt in calc.GetProgressionForLogicDef("King_Fragment"))
            {
                sr.AddPlacementToStringBuilder(sb, pmt);
            }
            sb.AppendLine();

            yield return new(sb.ToString(), "TestLog.txt");
        }
    }
}
