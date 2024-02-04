using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private static readonly ILogger _logger = new SimpleLogger("CondensedSpoilerLogger.SpoilerReader");

        private readonly Dictionary<string, List<(string location, string costText)>> placementsByItem = new();
        private readonly Dictionary<string, List<(string item, string costText)>> placementsByLocation = new();
        private readonly Dictionary<string, string> itemUINames = new();

        public IEnumerable<string> EnumerateItems() => placementsByItem.Keys;
        public IEnumerable<string> EnumerateLocations() => placementsByLocation.Keys;
        public IEnumerable<string> LocationsForItem(string item)
        {
            if (placementsByItem.TryGetValue(item, out List<(string location, string costText)> locationInfo))
            {
                foreach ((string location, string _) in locationInfo)
                {
                    yield return location;
                }
            }
            
            yield break;
        }
        public IEnumerable<string> ItemsForLocation(string location)
        {
            if (placementsByLocation.TryGetValue(location, out List<(string item, string costText)> itemInfo))
            {
                foreach ((string item, string _) in itemInfo)
                {
                    yield return item;
                }
            }

            yield break;
        }


        private string GetItemUIName(string item) => itemUINames.TryGetValue(item, out string uiName) ? uiName : Translation.Translate(item);
        private string GetLocationUIName(string loc) => Translation.Translate(loc);

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


        public SpoilerReader(LogArguments args) : this(args.ctx) { }

        public SpoilerReader(RandoModContext ctx)
        {
            foreach (ItemPlacement placement in ctx.itemPlacements)
            {
                ProcessPlacement(placement);
            }

            ApplyMerges();
            AddNotchCosts(ctx);
        }

        private void ProcessPlacement(ItemPlacement pmt)
        {
            RandoItem item = pmt.Item;
            if (item is PlaceholderItem { innerItem: RandoItem itm })
            {
                item = itm;
            }

            string itemName = item.Name;
#pragma warning disable CS0618 // Type or member is obsolete
            if (item.item is SplitCloakItem sc)
            {
                itemName = sc.LeftBiased ? ItemNames.Left_Mothwing_Cloak : ItemNames.Right_Mothwing_Cloak;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            string locationName = pmt.Location.Name;
            string costText = GetCostText(pmt);

            ProcessPlacement(itemName, locationName, costText);
        }

        /// <summary>
        /// Record a placement indicated by the given item being at the given location and cost.
        /// </summary>
        private void ProcessPlacement(string itemName, string locationName, string costText = "")
        {
            if (!placementsByItem.TryGetValue(itemName, out List<(string, string)> locations))
            {
                placementsByItem[itemName] = locations = new();
            }
            locations.Add((locationName, costText));

            if (!placementsByLocation.TryGetValue(locationName, out List<(string, string)> items))
            {
                placementsByLocation[locationName] = items = new();
            }
            items.Add((itemName, costText));
        }

        /// <summary>
        /// Create a SpoilerReader encompassing only the items which belong to a (specified) remote player.
        /// 
        /// Because of how Multiworld works, these items will only be those which appear in the current player's world.
        /// </summary>
        /// <param name="ctx">The RandoModContext</param>
        /// <param name="remotePlayerName">The name of the specified remote player.</param>
        public SpoilerReader(RandoModContext ctx, string remotePlayerName)
        {
            Regex suffix = new(@"_\(\d+\)$");

            string prefix = remotePlayerName + "'s ";

            int count = 0;
            foreach (ItemPlacement placement in ctx.itemPlacements)
            {
                string itemName = placement.Item.Name;
                string locationName = placement.Location.Name;

                if (!itemName.StartsWith(prefix))
                {
                    continue;
                }

                itemName = itemName.Substring(prefix.Length);
                if (!suffix.IsMatch(itemName))
                {
                    _logger.LogDebug($"Failed to remove suffix from placement: {placement.Item.Name}, {placement.Location.Name}");
                    continue;
                }
                itemName = suffix.Replace(itemName, string.Empty);

                ProcessPlacement(itemName, locationName, GetCostText(placement));
                count += 1;
            }

            _logger.LogDebug($"Processed {count} placements for player {remotePlayerName}");

            ApplyMerges();

            foreach (string item in placementsByItem.Keys)
            {
                itemUINames.Add(item, prefix + item);
            }
        }

        private void AddNotchCosts(RandoModContext ctx)
        {
            if (!ctx.GenerationSettings.MiscSettings.RandomizeNotchCosts) return;

            foreach ((string charmName, int charmNum) in CharmIdList.CharmIdMap)
            {
                itemUINames.Add(charmName, $"{Translation.Translate(charmName)} [{ctx.notchCosts[charmNum - 1]}]");
            }    
        }

        /// <summary>
        /// Record certain items as being the same
        /// </summary>
        private void ApplyMerges()
        {
            MergeItems(ItemNames.Mothwing_Cloak, ItemNames.Shade_Cloak);
            MergeItems(ItemNames.Left_Mothwing_Cloak, "Left_Shade_Cloak", "Left_Biased_Shade_Cloak");
            MergeItems(ItemNames.Right_Mothwing_Cloak, "Right_Shade_Cloak", "Right_Biased_Shade_Cloak");

            MergeItems(ItemNames.Vengeful_Spirit, ItemNames.Shade_Soul);
            MergeItems(ItemNames.Desolate_Dive, ItemNames.Descending_Dark);
            MergeItems(ItemNames.Howling_Wraiths, ItemNames.Abyss_Shriek);
            MergeItems(ItemNames.Dream_Nail, ItemNames.Dream_Gate, ItemNames.Awoken_Dream_Nail);
            MergeItems(ItemNames.Kingsoul, ItemNames.Queen_Fragment, ItemNames.King_Fragment, ItemNames.Void_Heart);
            MergeItems(ItemNames.Fragile_Strength, ItemNames.Unbreakable_Strength);
            MergeItems(ItemNames.Fragile_Heart, ItemNames.Unbreakable_Heart);
            MergeItems(ItemNames.Fragile_Greed, ItemNames.Unbreakable_Greed);
            MergeItems("Grimmchild", ItemNames.Grimmchild1, ItemNames.Grimmchild2);
        }

        /// <summary>
        /// Moves all of the placements for items in items are treated as part of the list for item1.
        /// Used when all of the items are considered to be identical.
        /// </summary>
        /// <param name="item1">The display name given to items from the group.</param>
        /// <param name="items">The items to be merged in.</param>
        private void MergeItems(string item1, params string[] items)
        {
            if (!placementsByItem.ContainsKey(item1)) placementsByItem[item1] = new();
            foreach (string item2 in items)
            {
                if (placementsByItem.TryGetValue(item2, out List<(string, string)> others))
                {
                    placementsByItem[item1].AddRange(others);
                }
                placementsByItem.Remove(item2);
            }
        }

        /// <summary>
        /// Returns true if any of the items are found in the list of placements.
        /// </summary>
        public bool HasRandomizedAny(params string[] items)
        {
            return items.Any(item => placementsByItem.TryGetValue(item, out var value) && value.Count > 0);
        }

        /// <summary>
        /// Adds an entry for the given item to the StringBuilder, provided there is at least one copy of the item.
        /// </summary>
        /// <param name="sb">The StringBuilder in use.</param>
        /// <param name="item">The name of the item.</param>
        /// <param name="forceMulti">If true or false, specify whether to treat the item as a multi item or a single item in the log.</param>
        /// <returns>True if anything was added, false otherwise.</returns>
        public bool AddItemToStringBuilder(StringBuilder sb, string item, bool? forceMulti = null)
        {
            if (!placementsByItem.TryGetValue(item, out List<(string, string)> locations) || locations.Count == 0)
            {
                return false;
            }

            string itemUIName = GetItemUIName(item);
            
            bool multi = forceMulti ?? (locations.Count > 1);
            if (!multi)
            {
                foreach ((string loc, string costText) in locations)
                {
                    string locUI = GetLocationUIName(loc);
                    sb.AppendLine($"{IndentString}{itemUIName} <---at---> {GetDisplayString(locUI, costText)}");
                }
            }
            else
            {
                sb.AppendLine($"{IndentString}{itemUIName}:");
                foreach ((string loc, string costText) in locations)
                {
                    string locUI = GetLocationUIName(loc);
                    sb.AppendLine($"{IndentString}- {GetDisplayString(locUI, costText)}");
                }
            }

            return true;
        }

        /// <summary>
        /// Adds an entry for the given location to the StringBuilder, provided there is at least item there.
        /// </summary>
        /// <param name="sb">The StringBuilder in use.</param>
        /// <param name="location">The name of the location.</param>
        /// <param name="forceMulti">If true or false, specify whether to treat the location as a multi location or a single location in the log.</param>
        /// <returns>True if anything was added, false otherwise.</returns>
        public bool AddLocationToStringBuilder(StringBuilder sb, string location, bool? forceMulti = null)
        {
            if (!placementsByLocation.TryGetValue(location, out List<(string, string)> items) || items.Count == 0)
            {
                return false;
            }

            bool multi = forceMulti ?? (items.Count > 1);
            if (!multi)
            {
                foreach ((string item, string costText) in items)
                {
                    AddPlacementToStringBuilder(sb, location, item, costText);
                }
            }
            else
            {
                string locUI = GetLocationUIName(location);

                sb.AppendLine($"{IndentString}{locUI}:");
                foreach ((string item, string costText) in items)
                {
                    string itemUIName = GetItemUIName(item);
                    sb.AppendLine($"{IndentString}- {GetDisplayString(itemUIName, costText)}");
                }
            }

            return true;
        }

        /// <summary>
        /// Add the given placement to a string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="pmt">The placement.</param>
        /// <returns>Always returns true.</returns>
        public bool AddPlacementToStringBuilder(StringBuilder sb, ItemPlacement pmt)
            => AddPlacementToStringBuilder(sb, pmt.Location.Name, pmt.Item.Name, GetCostText(pmt));

        /// <summary>
        /// Add the given placement to a string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="location">The location.</param>
        /// <param name="item">The name of the item.</param>
        /// <param name="costText">The cost text - usually generated using <see cref="GetCostText(ItemPlacement)"/> or <see cref="GetCostText(LogicCost)"/> if not null.</param>
        /// <returns>Always returns true.</returns>
        public bool AddPlacementToStringBuilder(StringBuilder sb, string location, string item, string costText = "")
        {
            string itemUIName = GetItemUIName(item);
            string locUI = GetLocationUIName(location);
            sb.AppendLine($"{IndentString}{itemUIName} <---at---> {GetDisplayString(locUI, costText)}");

            return true;
        }

        public static string GetDisplayString(string loc, string costText)
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

        /// <summary>
        /// Get the formatted cost text string for an item placement.
        /// </summary>
        public static string GetCostText(ItemPlacement placement)
        {
            string costText = string.Empty;
            if (placement.Location.costs != null)
            {
                costText = string.Join(", ", placement.Location.costs.Select(cost => GetCostText(cost)));
            }

            return costText;
        }

        /// <summary>
        /// Get the formatted cost text for a logic cost.
        /// </summary>
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

            string text = c.ToString();

            // Match `LogicEnemyKillCost {count enemy}` => `count enemy`
            Match match = Regex.Match(text, @"LogicEnemyKillCost \{(\d+) (\w+)\}");
            if (match.Success)
            {
                string count = match.Groups[1].Value;
                string enemy = match.Groups[2].Value;
                return $"{count} {enemy}";
            }

            return text;
        }
    }
}
