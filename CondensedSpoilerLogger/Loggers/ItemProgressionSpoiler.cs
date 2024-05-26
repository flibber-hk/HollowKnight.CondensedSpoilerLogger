using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Randomization;
using CondensedSpoilerLogger.Util;
using System;
using Modding;
using RandomizerCore.LogicItems;

namespace CondensedSpoilerLogger.Loggers
{
    public class ItemProgressionSpoiler : CslLogger
    {
        private static readonly ILogger _logger = new SimpleLogger("CondensedSpoilerLogger.ItemProgressionSpoiler");

        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            List<List<ItemPlacement>> spheredPlacements = CreateSpheredPlacements(args.ctx);
            if (spheredPlacements is null) yield break;

            yield return (LogSpheres(spheredPlacements, args), "OrderedItemProgressionSpoilerLog.txt");
            
            List<List<ItemPlacement>> importantPlacements = ComputeImportantPlacements(spheredPlacements, args.ctx);
#if DEBUG
            yield return (LogSpheres(importantPlacements, args), "ReducedItemProgressionSpoilerLog.txt");
#endif
            
            List<List<ItemPlacement>> filteredPlacements = FilterImportantPlacements(importantPlacements, args.ctx);
            yield return (LogSpheres(filteredPlacements, args), "FilteredItemProgressionSpoilerLog.txt");

            List<List<ItemPlacement>> skiploverPlacements = ComputeSkiploverPlacements(filteredPlacements, args.ctx);
            yield return (LogSpheres(skiploverPlacements, args), "SkiploverItemProgressionSpoilerLog.txt");
        }


        public static List<List<ItemPlacement>> CreateSpheredPlacements(RandoModContext ctx)
        {
            RCUtil.SetupPM(ctx, out _, out ProgressionManager pm, out MainUpdater mu);

            List<ItemPlacement> itemPlacements = ctx.itemPlacements;
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
                    _logger.LogError($"Unable to create {nameof(ItemProgressionSpoiler)}: no reachable locations ");
                    foreach (ItemPlacement pmt in itemPlacements)
                    {
                        _logger.LogError($"- {pmt.Item.Name} @ {pmt.Location.Name}");
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

        /// <summary>
        /// Creates a log with the sphered placements.
        /// </summary>
        /// <param name="loggedPlacements">All the placements. Any placement called <see cref="DummyItemName"/> will not be displayed.</param>
        /// <param name="args">The log arguments.</param>
        public string LogSpheres(List<List<ItemPlacement>> spheredPlacements, LogArguments args)
        {
            SpoilerReader sr = new(args.ctx);
            StringBuilder sb = new();
            sb.AppendLine($"Important item progression with seed: {args.gs.Seed}");
            sb.AppendLine();
            sb.AppendLine();
            for (int i = 0; i < spheredPlacements.Count; i++)
            {
                sb.AppendLine($"PROGRESSION SPHERE {i} ({spheredPlacements[i].Count} items)");

                foreach (ItemPlacement pmt in spheredPlacements[i])
                {
                    if (pmt.Item.Name == DummyItemName) continue;

                    sr.AddPlacementToStringBuilder(sb, pmt);
                }

                sb.AppendLine();
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private const string DummyItemName = "csl_empty_item";
        private static RandoModItem MakeDummyItem()
            => new() { item = new EmptyItem(DummyItemName) };

        public static List<List<ItemPlacement>> ComputeImportantPlacements(List<List<ItemPlacement>> spheredPlacements, RandoModContext ctx)
        {
            List<List<ItemPlacement>> importantPlacements = new();

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

            for (int i = 0; i < spheredPlacements.Count; i++)
            {
                List<ItemPlacement> sphere = new();
                importantPlacements.Add(sphere);
                HashSet<Term> terms = GetTerms(i);

                foreach (ItemPlacement pmt in spheredPlacements[i])
                {
                    if (!pmt.Item.GetAffectedTerms().Any(x => terms.Contains(x)))
                    {
                        sphere.Add(new ItemPlacement(MakeDummyItem(), pmt.Location));
                    }
                    else
                    {
                        sphere.Add(pmt);
                        pm.Add(pmt.Item, pmt.Location);
                    }
                }

            }

            return importantPlacements;
        }

        /// <summary>
        /// Given a collection of sphered placements, finds a minimal subset of them wrt inclusion such that
        /// all placements in the collection are reachable.
        /// </summary>
        public static List<List<ItemPlacement>> FilterImportantPlacements(List<List<ItemPlacement>> importantPlacements, RandoModContext ctx)
        {
            List<List<ItemPlacement>> result = new();
            foreach (List<ItemPlacement> sphere in importantPlacements)
            {
                result.Add(new(sphere));
            }

            RCUtil.SetupPM(ctx, out LogicManager lm, out ProgressionManager pm, out MainUpdater mu);

            if (!RCUtil.ValidateReachable(result.SelectMany(x => x), pm, true))
            {
                _logger.LogError($"{nameof(FilterImportantPlacements)}: Validation failure");
                return importantPlacements;
            }

            pm.Reset();

            foreach (List<ItemPlacement> sphere in ((IEnumerable<List<ItemPlacement>>)result).Reverse())
            {
                for (int i = sphere.Count - 1; i >= 0; i--)
                {
                    ItemPlacement current = sphere[i];
                    sphere[i] = new ItemPlacement(MakeDummyItem(), current.Location);

                    if (!RCUtil.ValidateReachable(result.SelectMany(x => x), pm, true))
                    {
                        sphere[i] = current;
                    }
                    pm.Reset();
                }
            }

            return result;
        }

        public static List<List<ItemPlacement>> ComputeSkiploverPlacements(List<List<ItemPlacement>> spheredPlacements, RandoModContext ctx)
        {
            // TODO: figure out how to enable All skips
            RCUtil.SetupPM(ctx, out LogicManager lm, out ProgressionManager pm, out MainUpdater mu);
            
            List<List<ItemPlacement>> skiploverPlacements = new();
            
            for (int present = 0; present < spheredPlacements.Count; present++)
            {
                List<ItemPlacement> skiploverSphere = new();

                // placements from present & future spheres that are reachable with skips
                for (int future = present; future < spheredPlacements.Count; future++)
                {
                    foreach (ItemPlacement pmt in spheredPlacements[future])
                    {
                        bool inPastSpheres = false;
                        for (int past = 0; past < present; past++)
                        {
                            if (skiploverPlacements[past].Contains(pmt))
                            {
                                inPastSpheres = true;
                                break;
                            }
                        }
                        if (!inPastSpheres && pmt.Location.CanGet(pm))
                        {
                            skiploverSphere.Add(pmt);
                        }
                    }
                }

                // add present sphere to skiploverPlacements and pm last
                skiploverPlacements.Add(skiploverSphere);
                foreach (ItemPlacement pmt in spheredPlacements[present])
                {
                    pm.Add(pmt.Item, pmt.Location);
                }
            }
            
            return skiploverPlacements;
        }

        public static ItemPlacement[] GetOrderedPlacements(RandoModContext ctx)
        {
            List<List<ItemPlacement>> spheredPlacements = CreateSpheredPlacements(ctx);
            List<List<ItemPlacement>> importantPlacements = ComputeImportantPlacements(spheredPlacements, ctx);
            List<List<ItemPlacement>> filteredPlacements = FilterImportantPlacements(importantPlacements, ctx);

            return filteredPlacements.SelectMany(x => x).Where(x => x.Item.Name != DummyItemName).ToArray();
        }
    }
}
