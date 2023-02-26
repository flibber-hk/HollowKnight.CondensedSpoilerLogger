﻿using System.Collections.Generic;
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
            List<List<ItemPlacement>> spheredPlacements = CreateSpheredPlacements(args);
            if (spheredPlacements is null) yield break;

            yield return (LogFullSpheres(spheredPlacements, args), "OrderedItemProgressionSpoilerLog.txt");
            yield return (LogImportantItems(spheredPlacements, args), "ReducedItemProgressionSpoilerLog.txt");
        }


        public static List<List<ItemPlacement>> CreateSpheredPlacements(LogArguments args)
        {
            RCUtil.SetupPM(args.ctx, out _, out ProgressionManager pm, out MainUpdater mu);

            List<ItemPlacement> itemPlacements = args.ctx.itemPlacements;
            List<List<ItemPlacement>> spheredPlacements = new();

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
                    return null;
                }

                foreach (ItemPlacement pmt in reachable)
                {
                    pm.Add(pmt.Item, pmt.Location);
                }
                // Add a clone of the current sphere to the list
                spheredPlacements.Add(new(reachable));

                itemPlacements = nonReachable;
            }

            return spheredPlacements;
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
                    sr.AddPlacementToStringBuilder(sb, pmt);
                }

                sb.AppendLine();
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public string LogImportantItems(List<List<ItemPlacement>> spheredPlacements, LogArguments args)
        {
            List<List<ItemPlacement>> importantPlacements = ComputeImportantPlacements(spheredPlacements, args);

            SpoilerReader sr = new(args);
            StringBuilder sb = new();
            sb.AppendLine($"Important item progression with seed: {args.gs.Seed}");
            sb.AppendLine();
            sb.AppendLine();
            for (int i = 0; i < importantPlacements.Count; i++)
            {
                sb.AppendLine($"PROGRESSION SPHERE {i} ({importantPlacements[i].Count} items)");

                // Write placements that might unlock something later
                foreach (ItemPlacement pmt in importantPlacements[i])
                {
                    sr.AddPlacementToStringBuilder(sb, pmt);
                }

                sb.AppendLine();
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public List<List<ItemPlacement>> ComputeImportantPlacements(List<List<ItemPlacement>> spheredPlacements, LogArguments args)
        {
            List<List<ItemPlacement>> importantPlacements = new();

            RCUtil.SetupPM(args.ctx, out LogicManager lm, out ProgressionManager pm, out MainUpdater mu);

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
                foreach (GeneralizedPlacement vpmt in args.ctx.Vanilla)
                {
                    if (vpmt.Location.CanGet(pm)) continue;
                    foreach (Term term in vpmt.Location.GetTerms()) terms.Add(term);
                }
                foreach (TransitionPlacement tpmt in args.ctx.transitionPlacements ?? Enumerable.Empty<TransitionPlacement>())
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

            for (int i = 0; i < spheredPlacements.Count; i++)
            {
                List<ItemPlacement> sphere = new();
                importantPlacements.Add(sphere);
                HashSet<Term> terms = GetTerms(i);

                foreach (ItemPlacement pmt in spheredPlacements[i])
                {
                    if (!pmt.Item.GetAffectedTerms().Any(x => terms.Contains(x)))
                    {
                        continue;
                    }

                    sphere.Add(pmt);
                    pm.Add(pmt.Item, pmt.Location);
                }

            }

            return importantPlacements;
        }
    }
}
