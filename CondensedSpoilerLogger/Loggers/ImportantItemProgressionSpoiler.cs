using RandomizerCore.Logic;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CondensedSpoilerLogger.Loggers
{
    internal class ImportantItemProgressionSpoiler : CslLogger
    {
        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            Stopwatch sw = new();
            sw.Start();

            ItemProgressionSpoiler.SetupPM(args.ctx, out LogicManager lm, out ProgressionManager pm, out MainUpdater mu);

            List<List<ItemPlacement>> spheredPlacements = ItemProgressionSpoiler.CreateSpheredPlacements(args);
            if (spheredPlacements is null) yield break;
            
            
            List<ItemPlacement?> orderedPlacements = spheredPlacements.SelectMany(x => x).Cast<ItemPlacement?>().ToList();
            Term claw = lm.GetTermStrict("LEFTCLAW");

            // Remove terms from the back that don't give claw because claw must've been given earlier in the procedure
            int i = 0;
            while (!orderedPlacements[i].Value.Item.GetAffectedTerms().Any(x => x == claw))
            {
                i++;
            }
            for (int j = i+1; j < orderedPlacements.Count; j++)
            {
                orderedPlacements[j] = null;
            }

            mu.SetLongTermRevertPoint();

            for (; i >= 0; i--)
            {
                ItemPlacement? current = orderedPlacements[i];
                orderedPlacements[i] = null;

                if (!Validate(orderedPlacements, pm, lm.GetTermStrict("LEFTCLAW")))
                {
                    orderedPlacements[i] = current;
                }

                pm.Reset();
            }

            List<ItemPlacement> selectedPlacements = orderedPlacements.Where(x => x is not null).Cast<ItemPlacement>().ToList();

            sw.Stop();
            CondensedSpoilerLogger.instance.Log($"Generated IIPSL in {sw.Elapsed.TotalSeconds} seconds.");

            SpoilerReader sr = new(args);
            StringBuilder sb = new();

            sb.AppendLine($"Minimal progression with seed {args.gs.Seed}");
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine($"Progression for LEFTCLAW");
            foreach (ItemPlacement pmt in selectedPlacements)
            {
                sr.AddPlacementToStringBuilder(sb, pmt);
            }

            yield return (sb.ToString(), "LeftClawProgressionSpoiler.txt");
        }

        public static bool Validate(List<ItemPlacement?> activePlacements, ProgressionManager pm, Term term)
        {
            foreach (ItemPlacement? rawPmt in activePlacements)
            {
                if (rawPmt is not ItemPlacement pmt) continue;
                pm.mu.AddEntry(new PrePlacedItemUpdateEntry(pmt));
            }

            pm.mu.StartUpdating();

            return pm.Has(term);
        }
    }
}
