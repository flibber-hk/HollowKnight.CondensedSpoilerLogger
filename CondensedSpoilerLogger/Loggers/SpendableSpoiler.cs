using CondensedSpoilerLogger.Util;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CondensedSpoilerLogger.Loggers
{
    /// <summary>
    /// Logs geo/essence
    /// </summary>
    internal class SpendableSpoiler : CslLogger
    {
        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            SpoilerReader sr = new(args.ctx);
            StringBuilder sb = new();

            sb.AppendLine($"Currency spoiler log for seed: {args.gs.Seed}");
            sb.AppendLine();

            int count = 0;

            count += WriteItems("GEO", sr, sb, "----------Geo Items:----------", args);
            count += WriteItems("ESSENCE", sr, sb, "----------Essence Items:----------", args);

            if (count == 0) yield break;

            yield return (sb.ToString(), "CurrencySpoilerLog.txt");
        }

        private int WriteItems(string term, SpoilerReader sr, StringBuilder sb, string title, LogArguments args)
        {
            if (!FindItems(args.ctx, term, out List<(ItemPlacement pmt, int amount)> data))
            {
                return 0;
            }

            data = data.OrderByDescending(pair => pair.amount).ToList();

            sb.AppendLine(title);
            foreach ((ItemPlacement pmt, int amount) in data)
            {
                sb.Append($"({amount}) ");
                sr.AddPlacementToStringBuilder(sb, pmt);
            }
            sb.AppendLine();

            return data.Count;
        }

        private bool FindItems(RandoModContext ctx, string term, out List<(ItemPlacement pmt, int amount)> data)
        {
            data = new();

            // Set up a single PM to use to determine item effects
            RCUtil.SetupPM(ctx, out _, out ProgressionManager pm, out _);

            foreach (ItemPlacement pmt in ctx.itemPlacements)
            {
                Logger.Log(pmt.Item.Name);

                LogicItem item = pmt.Item.item;
                if (item is null) continue;  // shouldn't happen
                if (!item.GetAffectedTerms().Any(x => x.Name == term)) continue;

                int initialValue = pm.Get(term);
                pm.Add(item);
                int amount = pm.Get(term) - initialValue;
                pm.Reset();

                data.Add((pmt, amount));
            }

            return data.Count > 0;
        }
    }
}
