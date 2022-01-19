using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerCore;
using RandomizerCore.Logic;
using ItemChanger;
using Modding;

namespace CondensedSpoilerLogger
{
    public class SpoilerReader
    {
        private readonly Dictionary<string, List<(string location, string costText)>> placements;
        private readonly Dictionary<string, string> itemUINames = new();

        private int _indent;
        /// <summary>
        /// Set to add the specified number of spaces to the start of each line.
        /// </summary>
        public int Indent
        {
            get { return _indent; }
            set { _indent = value; IndentString = new string(' ', value); }
        }
        private string IndentString = string.Empty;
        

        public SpoilerReader(LogArguments args)
        {
            List<ItemPlacement> raw = args.ctx.itemPlacements;

            placements = new();
            foreach (ItemPlacement placement in raw)
            {
                RandoItem item = placement.item;
                if (item is PlaceholderItem { innerItem: RandoItem itm })
                {
                    item = itm;
                }

                string itemName = item.Name;
                if (item.item is SplitCloakItem sc)
                {
                    itemName = sc.LeftBiased ? ItemNames.Left_Mothwing_Cloak : ItemNames.Right_Mothwing_Cloak;
                }

                string locationName = placement.location.Name;

                string costText = string.Empty;
                if (placement.location.costs != null)
                {
                    costText = string.Join(", ", placement.location.costs.Select(cost => GetCostText(cost)));
                }

                if (!placements.TryGetValue(itemName, out List<(string, string)> locations))
                {
                    placements[itemName] = locations = new();
                }

                locations.Add((locationName, costText));
            }

            ApplyMerges();
            AddNotchCosts(args);
        }

        private void AddNotchCosts(LogArguments args)
        {
            if (!args.gs.MiscSettings.RandomizeNotchCosts) return;

            foreach ((string charmName, int charmNum) in CharmIdList.CharmIdMap)
            {
                itemUINames.Add(charmName, $"{charmName} [{args.ctx.notchCosts[charmNum - 1]}]");
            }    
        }

        private void ApplyMerges()
        {
            Merge(ItemNames.Mothwing_Cloak, ItemNames.Shade_Cloak);
            Merge(ItemNames.Left_Mothwing_Cloak, "Left_Shade_Cloak");
            Merge(ItemNames.Right_Mothwing_Cloak, "Right_Shade_Cloak");

            Merge(ItemNames.Vengeful_Spirit, ItemNames.Shade_Soul);
            Merge(ItemNames.Desolate_Dive, ItemNames.Descending_Dark);
            Merge(ItemNames.Howling_Wraiths, ItemNames.Abyss_Shriek);
            Merge(ItemNames.Dream_Nail, ItemNames.Dream_Gate, ItemNames.Awoken_Dream_Nail);
            Merge(ItemNames.Kingsoul, ItemNames.Queen_Fragment, ItemNames.King_Fragment, ItemNames.Void_Heart);
            Merge(ItemNames.Fragile_Strength, ItemNames.Unbreakable_Strength);
            Merge(ItemNames.Fragile_Heart, ItemNames.Unbreakable_Heart);
            Merge(ItemNames.Fragile_Greed, ItemNames.Unbreakable_Greed);
            Merge("Grimmchild", ItemNames.Grimmchild1, ItemNames.Grimmchild2);
        }

        /// <summary>
        /// Moves all of the placements for items in items are treated as part of the list for item1.
        /// Used when all of the items are considered to be identical.
        /// </summary>
        /// <param name="item1">The display name given to items from the group.</param>
        /// <param name="items">The items to be merged in.</param>
        private void Merge(string item1, params string[] items)
        {
            if (!placements.ContainsKey(item1)) placements[item1] = new();
            foreach (string item2 in items)
            {
                if (placements.TryGetValue(item2, out List<(string, string)> others))
                {
                    placements[item1].AddRange(others);
                }
                placements.Remove(item2);
            }
        }

        /// <summary>
        /// Returns true if any of the items are found in the list of placements.
        /// </summary>
        public bool HasRandomizedAny(params string[] items)
        {
            return items.Any(item => placements.TryGetValue(item, out var value) && value.Count > 0);
        }

        /// <summary>
        /// Adds an entry for the given item to the StringBuilder, provided there is at least one copy of the item.
        /// </summary>
        /// <param name="sb">The StringBuilder in use.</param>
        /// <param name="item">The name of the item.</param>
        /// <param name="forceMulti">If true or false, specify whether to treat the location as a multi location or a single location in the log.</param>
        public void AddItemToStringBuilder(StringBuilder sb, string item, bool? forceMulti = null)
        {
            if (!placements.TryGetValue(item, out List<(string, string)> locations) || locations.Count == 0)
            {
                return;
            }

            string itemUIName = itemUINames.TryGetValue(item, out string val) ? val : item;
            
            bool multi = forceMulti ?? (locations.Count > 1);
            if (!multi)
            {
                foreach ((string loc, string costText) in locations)
                {
                    sb.AppendLine($"{IndentString}{itemUIName} <---at---> {GetDisplayString(loc, costText)}");
                }
            }
            else
            {
                sb.AppendLine($"{IndentString}{itemUIName}:");
                foreach ((string loc, string costText) in locations)
                {
                    sb.AppendLine($"{IndentString}- {GetDisplayString(loc, costText)}");
                }
            }
        }

        private static string GetDisplayString(string loc, string costText)
        {
            if (!string.IsNullOrEmpty(costText))
            {
                return $"{loc} ({costText})";
            }
            else
            {
                return loc;
            }
        }

        public static string GetCostText(LogicCost c)
        {
            if (c is LogicGeoCost lgc)
            {
                return $"{lgc.GeoAmount} Geo";
            }
            else if (c is SimpleCost sc)
            {
                return $"{sc.threshold} {sc.term.Name}";
            }

            return c.ToString();
        }
    }
}
