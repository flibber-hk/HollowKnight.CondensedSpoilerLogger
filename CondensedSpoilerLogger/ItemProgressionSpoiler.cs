using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Randomization;
using ItemChanger;

namespace CondensedSpoilerLogger
{
    public class ItemProgressionSpoiler : RandoLogger
    {
        public override void Log(LogArguments args)
        {
            SpoilerReader sr = new(args);
            StringBuilder sb = new();

            sb.AppendLine($"Logical item progression with seed: {args.gs.Seed}");
            sb.AppendLine();
            sb.AppendLine();

            Randomizer randomizer = (Randomizer)args.randomizer;
            RandoModContext ctx = (RandoModContext)randomizer.ctx;

            ProgressionManager pm = new(ctx.LM, ctx);
            MainUpdater mu = new(ctx.LM);

            mu.AddPlacements(ctx.LM.Waypoints);
            mu.AddPlacements(ctx.Vanilla);
            if (ctx.transitionPlacements is not null)
            {
                mu.AddEntries(ctx.transitionPlacements.Select(t => new PrePlacedItemUpdateEntry(t)));
            }
            mu.Hook(pm);

            // Take item placements from the LogArguments
            List<ItemPlacement> itemPlacements = args.ctx.itemPlacements;

            int groupCount = 0;

            while (itemPlacements.Count > 0)
            {
                List<ItemPlacement> reachable = new();
                List<ItemPlacement> nonReachable = new();
                foreach (ItemPlacement pmt in itemPlacements)
                {
                    if (pmt.Location.CanGet(pm))
                    {
                        reachable.Add(pmt);
                    }
                    else
                    {
                        nonReachable.Add(pmt);
                    }
                }

                itemPlacements = nonReachable;

                if (reachable.Count == 0)
                {
                    CondensedSpoilerLogger.instance.LogError($"Unable to create {nameof(ItemProgressionSpoiler)}: no reachable locations ");
                    return;
                }

                sb.AppendLine($"PROGRESSION SPHERE {groupCount++}");
                foreach (ItemPlacement pmt in reachable)
                {
                    pm.Add(pmt.Item);
                    sr.AddPlacementToStringBuilder(sb, pmt.Location.Name, pmt.Item.Name, SpoilerReader.GetCostText(pmt));
                }

                sb.AppendLine();
                sb.AppendLine();
                mu.Hook(pm);
            }

            LogManager.Write(sb.ToString(), "OrderedItemProgressionSpoilerLog.txt");
        }
    }
}
