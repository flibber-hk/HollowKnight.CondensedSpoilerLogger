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
    }
}
