using System;
using System.Text;
using RandomizerMod.Logging;
using Modding;
using UnityEngine;
using System.Collections.Generic;

namespace CondensedSpoilerLogger.Loggers
{
    public class NotchCostSpoiler : CslLogger
    {
        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            if (!args.gs.MiscSettings.RandomizeNotchCosts) yield break;

            StringBuilder sb = new();

            sb.AppendLine($"Notch costs with seed: {args.gs.Seed}");
            sb.AppendLine();

            int tot = 0;
            foreach ((string charmName, int charmNum) in CharmIdList.CharmIdMap)
            {
                int cost = args.ctx.notchCosts[charmNum - 1];
                tot += cost;
                sb.AppendLine($"{charmName}: {cost}");
            }

            sb.AppendLine();
            int perc = Mathf.RoundToInt(tot / 90f * 100f);
            sb.AppendLine($"Total: {tot}. This is {perc}% of the vanilla total.");

            yield return (sb.ToString(), "NotchCostSpoiler.txt");
        }
    }
}