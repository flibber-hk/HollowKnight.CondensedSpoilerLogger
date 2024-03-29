﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Logging;

namespace CondensedSpoilerLogger.Loggers
{
    public class ItemGroupSpoiler : CslLogger
    {
        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            SpoilerReader sr = new(args.ctx);
            StringBuilder sb = new();
            sb.AppendLine($"Collected item spoiler log for seed {args.gs.Seed}");
            sb.AppendLine();

            List<string> repeatedItems = new();
            List<string> singleItems = new();

            foreach (string item in sr.EnumerateItems())
            {
                if (sr.LocationsForItem(item).Count() > 1)
                {
                    repeatedItems.Add(item);
                }
                else
                {
                    singleItems.Add(item);
                }
            }

            repeatedItems.Sort();
            singleItems.Sort();

            foreach (string item in repeatedItems)
            {
                sr.AddItemToStringBuilder(sb, item);
                sb.AppendLine();
            }
            foreach (string item in singleItems)
            {
                sr.AddItemToStringBuilder(sb, item);
            }

            yield return (sb.ToString(), "CollectedItemSpoiler.txt");
        }
    }
}
