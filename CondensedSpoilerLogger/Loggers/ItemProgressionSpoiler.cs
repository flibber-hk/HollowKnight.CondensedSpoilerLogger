using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Randomization;
using CondensedSpoilerLogger.Util;

namespace CondensedSpoilerLogger.Loggers
{
    public class ItemProgressionSpoiler : CslLogger
    {
        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            Randomizer randomizer = (Randomizer)args.randomizer;
            RandoModContext ctx = (RandoModContext)randomizer.ctx;

            RCUtil.SetupPM(ctx, out _, out ProgressionManager pm, out MainUpdater mu);

            // Take item placements from the LogArguments
            List<ItemPlacement> itemPlacements = args.ctx.itemPlacements;
            List<List<ItemPlacement>> SpheredPlacements = new();

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

                if (reachable.Count == 0)
                {
                    CondensedSpoilerLogger.instance.LogError($"Unable to create {nameof(ItemProgressionSpoiler)}: no reachable locations ");
                    foreach (ItemPlacement pmt in itemPlacements)
                    {
                        CondensedSpoilerLogger.instance.LogError($"- {pmt.Item.Name} @ {pmt.Location.Name}");
                    }
                    yield break;
                }

                foreach (ItemPlacement pmt in reachable)
                {
                    pm.Add(pmt.Item, pmt.Location);
                }
                // Add a clone of the current sphere to the list
                SpheredPlacements.Add(new(reachable));

                itemPlacements = nonReachable;
            }

            yield return (LogFullSpheres(SpheredPlacements, args), "OrderedItemProgressionSpoilerLog.txt");
            yield return (LogImportantItems(SpheredPlacements, args), "ReducedItemProgressionSpoilerLog.txt");
        }

        public string LogFullSpheres(List<List<ItemPlacement>> spheredPlacements, LogArguments args)
        {
            SpoilerReader sr = new(args);
            StringBuilder sb = new();
            sb.AppendLine($"Logical item progression with seed: {args.gs.Seed}");
            sb.AppendLine();
            sb.AppendLine();
            for (int i = 0; i < spheredPlacements.Count; i++)
            {
                sb.AppendLine($"PROGRESSION SPHERE {i}");
                foreach (ItemPlacement pmt in spheredPlacements[i])
                {
                    sr.AddPlacementToStringBuilder(sb, pmt.Location.Name, pmt.Item.Name, SpoilerReader.GetCostText(pmt));
                }

                sb.AppendLine();
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public string LogImportantItems(List<List<ItemPlacement>> spheredPlacements, LogArguments args)
        {
            Randomizer randomizer = (Randomizer)args.randomizer;
            RandoModContext ctx = (RandoModContext)randomizer.ctx;

            RCUtil.SetupPM(ctx, out LogicManager lm, out ProgressionManager pm, out MainUpdater mu);

            // All terms that might unlock something in a later sphere - either a location from a later sphere, or
            // a waypoint/vanilla placement/transition that the pm doesn't have yet
            HashSet<Term> GetTerms(int i)
            {
                HashSet<Term> terms = new();
                foreach (LogicWaypoint wp in lm.Waypoints)
                {
                    if (wp.CanGet(pm)) continue;
                    foreach (Term term in wp.GetTerms()) terms.Add(term);
                }
                foreach (GeneralizedPlacement vpmt in ctx.Vanilla)
                {
                    if (vpmt.Location.CanGet(pm)) continue;
                    foreach (Term term in vpmt.Location.GetTerms()) terms.Add(term);
                }
                foreach (TransitionPlacement tpmt in ctx.transitionPlacements ?? Enumerable.Empty<TransitionPlacement>())
                {
                    if (tpmt.Source.CanGet(pm)) continue;
                    foreach (Term term in tpmt.Source.GetTerms()) terms.Add(term);
                }
                for (int j = i + 1; j < spheredPlacements.Count; j++)
                {
                    foreach (ItemPlacement ipmt in spheredPlacements[j])
                    {
                        foreach (Term term in ipmt.Location.GetTerms()) terms.Add(term);
                    }
                }
                return terms;
            }

            SpoilerReader sr = new(args);
            StringBuilder sb = new();
            sb.AppendLine($"Important item progression with seed: {args.gs.Seed}");
            sb.AppendLine();
            sb.AppendLine();
            for (int i = 0; i < spheredPlacements.Count; i++)
            {
                HashSet<Term> terms = GetTerms(i);

                sb.AppendLine($"PROGRESSION SPHERE {i} ({spheredPlacements[i].Count} items)");

                // Write placements that might unlock something later
                foreach (ItemPlacement pmt in spheredPlacements[i])
                {
                    if (!pmt.Item.GetAffectedTerms().Any(x => terms.Contains(x)))
                    {
                        continue;
                    }

                    sr.AddPlacementToStringBuilder(sb, pmt.Location.Name, pmt.Item.Name, SpoilerReader.GetCostText(pmt));
                    pm.Add(pmt.Item);
                }

                sb.AppendLine();
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void SetupPM(RandoModContext ctx, out LogicManager lm, out ProgressionManager pm, out MainUpdater mu)
        {
            // Clone LM for thread safety reasons
            lm = new(new LogicManagerBuilder(ctx.LM));


            pm = new(lm, ctx);
            mu = pm.mu;

            mu.AddWaypoints(lm.Waypoints);
            mu.AddTransitions(lm.TransitionLookup.Values);
            mu.AddPlacements(ctx.Vanilla);
            if (ctx.transitionPlacements is not null)
            {
                mu.AddEntries(ctx.transitionPlacements.Select(t => new PrePlacedItemUpdateEntry(t)));
            }
            mu.StartUpdating();
        }
    }
}
