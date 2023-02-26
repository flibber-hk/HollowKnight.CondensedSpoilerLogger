using RandomizerCore.Logic;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CondensedSpoilerLogger.Util
{
    public static class RCUtil
    {
        public static void SetupPM(RandoModContext ctx, out LogicManager lm, out ProgressionManager pm, out MainUpdater mu)
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

            mu.SetLongTermRevertPoint();
        }

        public static bool ValidateReachable(IEnumerable<ItemPlacement> placements, ProgressionManager pm, bool ordered = false)
        {
            switch (ordered)
            {
                case true:
                    return ValidateReachableOrdered(placements, pm);
                case false:
                    return ValidateReachableUnordered(placements, pm);
            }
        }

        public static bool ValidateReachableOrdered(IEnumerable<ItemPlacement> placements, ProgressionManager pm)
        {
            pm.mu.StartUpdating();
            foreach (ItemPlacement pmt in placements)
            {
                PrePlacedItemUpdateEntry entry = new(pmt);
                pm.mu.AddEntry(entry);
                if (!entry.obtained) return false;
            }
            return true;
        }

        public static bool ValidateReachableUnordered(IEnumerable<ItemPlacement> placements, ProgressionManager pm)
        {
            // First, validate that everything is reachable
            List<PrePlacedItemUpdateEntry> entries = new();
            foreach (ItemPlacement pmt in placements)
            {
                PrePlacedItemUpdateEntry entry = new(pmt);
                entries.Add(entry);
                pm.mu.AddEntry(entry);
            }

            pm.mu.StartUpdating();

            return entries.All(e => e.obtained);
        }
    }
}
