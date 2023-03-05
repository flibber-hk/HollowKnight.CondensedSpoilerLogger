using CondensedSpoilerLogger.Loggers;
using CondensedSpoilerLogger.Util;
using ItemChanger;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CondensedSpoilerLogger.SpecificProgression
{
    /// <summary>
    /// Tool to calculate the progression to get to places
    /// </summary>
    public class Calculator
    {
        private readonly RandoModContext _ctx;
        /// <summary>
        /// List of placements in the order they can be collected; any progression chain will be a subsequence of this
        /// </summary>
        private readonly ItemPlacement[] _orderedPlacements;

        public Calculator(RandoModContext ctx, out ItemPlacement[] orderedPlacements) : this(ctx, ItemProgressionSpoiler.GetOrderedPlacements(ctx))
        {
            orderedPlacements = _orderedPlacements;
        }

        public Calculator(RandoModContext ctx, ItemPlacement[] orderedPlacements)
        {
            _ctx = ctx;
            _orderedPlacements = orderedPlacements;
        }

        private const string DummyItemName = "csl_empty_item";
        private static RandoModItem MakeDummyItem()
            => new() { item = new EmptyItem(DummyItemName) };


        public ItemPlacement[] GetProgressionForTerm(string term)
        {
            RCUtil.SetupPM(_ctx, out LogicManager lm, out ProgressionManager pm, out MainUpdater mu);

            Term t = lm.GetTerm(term);
            if (t is null)
            {
                return null;
            }

            return GetProgressionForPredicate(() => pm.Has(t), pm);
        }

        public ItemPlacement[] GetProgressionForLogicDef(string logicDef)
        {
            RCUtil.SetupPM(_ctx, out LogicManager lm, out ProgressionManager pm, out MainUpdater mu);

            LogicDef def = lm.GetLogicDef(logicDef);
            if (def == null)
            {
                return null;
            }

            return GetProgressionForPredicate(() => def.CanGet(pm), pm);
        }

        public ItemPlacement[] GetProgressionForPredicate(Func<bool> predicate, ProgressionManager pm)
        {
            pm.Reset();
            pm.mu.StartUpdating();

            int i = 0; // The number of placements we've taken
            while (!predicate())
            {
                if (i >= _orderedPlacements.Length)
                {
                    return null;
                }
                pm.mu.AddEntry(new PrePlacedItemUpdateEntry(_orderedPlacements[i]));
                i++;
            }

            List<ItemPlacement> segment = _orderedPlacements.Take(i).ToList();
            pm.Reset();

            // Remove items where possible
            for (int j = i-1; j >= 0; j--)
            {
                ItemPlacement current = segment[j];
                segment[j] = new(MakeDummyItem(), current.Location);

                if (!Validate(segment))
                {
                    segment[j] = current;
                }
            }

            bool Validate(IEnumerable<ItemPlacement> pmts)
            {
                pm.Reset();
                pm.mu.AddEntries(pmts.Select(p => new PrePlacedItemUpdateEntry(p)));
                pm.mu.StartUpdating();
                return predicate();
            }

            return segment.Where(x => x.Item.Name != DummyItemName).ToArray();
        }
    }
}
