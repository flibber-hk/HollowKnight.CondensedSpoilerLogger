using ItemChanger;
using RandomizerCore;
using RandomizerCore.LogicItems;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            List<(ItemPlacement pmt, int amount)> data = new();

            foreach (ItemPlacement pmt in args.ctx.itemPlacements)
            {
                string itemName = pmt.Item.Name;
                if (args.ctx.LM.GetItem(itemName) is not SingleItem item) continue;
                if (item.Effect.Term.Name != term) continue;

                data.Add((pmt, item.Effect.Value));
            }

            if (data.Count == 0) return 0;

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
    }
}
