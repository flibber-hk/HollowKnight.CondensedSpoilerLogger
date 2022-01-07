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

namespace CondensedSpoilerLogger
{
    public class CondensedSpoilerLog : RandoLogger
    {
        public override void Log(LogArguments args)
        {
            List<ItemPlacement> raw = args.ctx.itemPlacements;

            Dictionary<string, List<(string location, string item, string costText)>> placements = new();
            foreach (ItemPlacement placement in raw)
            {
                string item = placement.item.Name;
                if (placement.item.item is SplitCloakItem sc)
                {
                    item = sc.LeftBiased ? ItemNames.Left_Mothwing_Cloak : ItemNames.Right_Mothwing_Cloak;
                }
                if (item.StartsWith(RandomizerMod.RC.PlaceholderItem.Prefix))
                {
                    item = item.Substring(RandomizerMod.RC.PlaceholderItem.Prefix.Length);
                }

                string loc = placement.location.Name;

                string costText = string.Empty;
                if (placement.location.costs != null)
                {
                    costText = string.Join(", ", placement.location.costs.Select(cost => GetCostText(cost)));
                }

                if (!placements.TryGetValue(item, out List<(string, string, string)> locations))
                {
                    placements[item] = locations = new();
                }

                locations.Add((loc, item, costText));
            }

            void Merge(string item1, params string[] items)
            {
                if (!placements.ContainsKey(item1)) placements[item1] = new();
                foreach (string item2 in items)
                {
                    if (placements.TryGetValue(item2, out List<(string, string, string)> others))
                    {
                        placements[item1].AddRange(others);
                    }
                }
            }
            Merge(ItemNames.Mothwing_Cloak, ItemNames.Shade_Cloak, ItemNames.Left_Mothwing_Cloak, "Left_Shade_Cloak",
                ItemNames.Right_Mothwing_Cloak, "Right_Shade_Cloak");
            Merge(ItemNames.Mantis_Claw, ItemNames.Left_Mantis_Claw, ItemNames.Right_Mantis_Claw);
            Merge(ItemNames.Crystal_Heart, ItemNames.Left_Crystal_Heart, ItemNames.Right_Crystal_Heart);
            Merge(ItemNames.Vengeful_Spirit, ItemNames.Shade_Soul);
            Merge(ItemNames.Desolate_Dive, ItemNames.Descending_Dark);
            Merge(ItemNames.Howling_Wraiths, ItemNames.Abyss_Shriek);
            Merge(ItemNames.Dream_Nail, ItemNames.Dream_Gate, ItemNames.Awoken_Dream_Nail);
            Merge(ItemNames.Kingsoul, ItemNames.Queen_Fragment, ItemNames.King_Fragment, ItemNames.Void_Heart);
            Merge(ItemNames.Fragile_Strength, ItemNames.Unbreakable_Strength);

            StringBuilder sb = new();
            void Add(string item)
            {
                if (!placements.TryGetValue(item, out List<(string, string, string)> locations))
                {
                    return;
                }

                foreach ((string loc, string itm, string costText) in locations)
                {
                    if (!string.IsNullOrEmpty(costText))
                    {
                        sb.AppendLine($"{itm} <---at---> {loc} ({costText})");
                    }
                    else
                    {
                        sb.AppendLine($"{itm} <---at---> {loc}");
                    }
                }
            }

            sb.AppendLine("----------Major Progression:----------");
            Add(ItemNames.Mothwing_Cloak);
            Add(ItemNames.Mantis_Claw);
            Add(ItemNames.Monarch_Wings);
            Add(ItemNames.Crystal_Heart);
            Add(ItemNames.Ismas_Tear);
            Add(ItemNames.Dream_Nail);
            Add(ItemNames.Swim);
            Add(ItemNames.Leftslash);
            Add(ItemNames.Rightslash);
            Add(ItemNames.Upslash);
            Add(ItemNames.Downslash);
            sb.AppendLine();

            sb.AppendLine("----------Spells:----------");
            Add(ItemNames.Vengeful_Spirit);
            Add(ItemNames.Desolate_Dive);
            Add(ItemNames.Howling_Wraiths);
            Add(ItemNames.Focus);
            sb.AppendLine();

            sb.AppendLine("----------Nail Arts:----------");
            Add(ItemNames.Cyclone_Slash);
            Add(ItemNames.Great_Slash);
            Add(ItemNames.Dash_Slash);
            sb.AppendLine();

            sb.AppendLine("----------Dreamers:----------");
            Add(ItemNames.Lurien);
            Add(ItemNames.Monomon);
            Add(ItemNames.Herrah);
            Add(ItemNames.Dreamer);
            sb.AppendLine();

            sb.AppendLine("----------White Fragments:----------");
            Add(ItemNames.Kingsoul);
            sb.AppendLine();

            sb.AppendLine("----------Stag Stations:----------");
            Add(ItemNames.Dirtmouth_Stag);
            Add(ItemNames.Crossroads_Stag);
            Add(ItemNames.Greenpath_Stag);
            Add(ItemNames.Queens_Station_Stag);
            Add(ItemNames.Queens_Gardens_Stag);
            Add(ItemNames.City_Storerooms_Stag);
            Add(ItemNames.Kings_Station_Stag);
            Add(ItemNames.Resting_Grounds_Stag);
            Add(ItemNames.Distant_Village_Stag);
            Add(ItemNames.Hidden_Station_Stag);
            Add(ItemNames.Stag_Nest_Stag);
            sb.AppendLine();

            sb.AppendLine("----------Keys: ----------");
            Add(ItemNames.Simple_Key);
            Add(ItemNames.Shopkeepers_Key);
            Add(ItemNames.Elegant_Key);
            Add(ItemNames.Love_Key);
            Add(ItemNames.Tram_Pass);
            Add(ItemNames.Elevator_Pass);
            Add(ItemNames.Lumafly_Lantern);
            Add(ItemNames.Kings_Brand);
            Add(ItemNames.City_Crest);
            sb.AppendLine();

            sb.AppendLine("----------Important Charms:----------");
            Add(ItemNames.Grimmchild1);
            Add(ItemNames.Grimmchild2);
            Add(ItemNames.Dashmaster);
            Add(ItemNames.Shaman_Stone);
            Add(ItemNames.Spell_Twister);
            Add(ItemNames.Fragile_Strength);
            Add(ItemNames.Quick_Slash);
            Add(ItemNames.Grubberflys_Elegy);
            Add(ItemNames.Glowing_Womb);
            Add(ItemNames.Spore_Shroom);
            Add(ItemNames.Weaversong);
            Add(ItemNames.Mark_of_Pride);
            sb.AppendLine();

            LogManager.Write(sb.ToString(), "CondensedSpoilerLog.json");
        }

        public static string GetCostText(LogicCost c)
        {
            if (c is LogicGeoCost lgc)
            {
                return $"{lgc.GeoAmount} GEO";
            }
            else if (c is SimpleCost sc)
            {
                return $"{sc.threshold} {sc.term.Name}";
            }

            return c.ToString();
        }
    }
}
