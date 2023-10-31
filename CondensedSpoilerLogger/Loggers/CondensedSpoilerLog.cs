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

namespace CondensedSpoilerLogger.Loggers
{
    public class CondensedSpoilerLog : CslLogger
    {
        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            SpoilerReader sr = new(args.ctx);
            StringBuilder sb = new();

            sb.AppendLine($"Condensed spoiler log for seed: {args.gs.Seed}");
            sb.AppendLine();

            sb.AppendLine("----------Major Progression:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Mothwing_Cloak, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Left_Mothwing_Cloak, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Right_Mothwing_Cloak, forceMulti: true);
            sb.AppendLine();
            sr.AddItemToStringBuilder(sb, ItemNames.Mantis_Claw, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Left_Mantis_Claw, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Right_Mantis_Claw, forceMulti: true);
            sb.AppendLine();
            sr.AddItemToStringBuilder(sb, ItemNames.Monarch_Wings, forceMulti: true);
            sb.AppendLine();
            sr.AddItemToStringBuilder(sb, ItemNames.Crystal_Heart, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Left_Crystal_Heart, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Right_Crystal_Heart, forceMulti: true);
            sb.AppendLine();
            sr.AddItemToStringBuilder(sb, ItemNames.Ismas_Tear, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Dream_Nail, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Swim, forceMulti: true);
            sb.AppendLine();

            sb.AppendLine("----------Spells:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Vengeful_Spirit, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Desolate_Dive, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Howling_Wraiths, forceMulti: true);
            sr.AddItemToStringBuilder(sb, ItemNames.Focus);
            sb.AppendLine();

            if (sr.HasRandomizedAny(ItemNames.Leftslash, ItemNames.Rightslash, ItemNames.Upslash, ItemNames.Downslash))
            {
                sb.AppendLine("----------Nail Slashes:----------");
            }
            else
            {
                sb.AppendLine("----------Nail Arts:----------");
            }
            sr.AddItemToStringBuilder(sb, ItemNames.Cyclone_Slash);
            sr.AddItemToStringBuilder(sb, ItemNames.Great_Slash);
            sr.AddItemToStringBuilder(sb, ItemNames.Dash_Slash);
            if (sr.HasRandomizedAny(ItemNames.Leftslash, ItemNames.Rightslash, ItemNames.Upslash, ItemNames.Downslash))
            {
                sb.AppendLine();
                sr.AddItemToStringBuilder(sb, ItemNames.Leftslash);
                sr.AddItemToStringBuilder(sb, ItemNames.Rightslash);
                sr.AddItemToStringBuilder(sb, ItemNames.Upslash);
                sr.AddItemToStringBuilder(sb, ItemNames.Downslash);
            }
            sb.AppendLine();

            sb.AppendLine("----------Dreamers:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Lurien);
            sr.AddItemToStringBuilder(sb, ItemNames.Monomon);
            sr.AddItemToStringBuilder(sb, ItemNames.Herrah);
            sr.AddItemToStringBuilder(sb, ItemNames.Dreamer);
            sb.AppendLine();

            sb.AppendLine("----------White Fragments:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Kingsoul);
            sb.AppendLine();

            sb.AppendLine("----------Stag Stations:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Dirtmouth_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.Crossroads_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.Greenpath_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.Queens_Station_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.Queens_Gardens_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.City_Storerooms_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.Kings_Station_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.Resting_Grounds_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.Distant_Village_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.Hidden_Station_Stag);
            sr.AddItemToStringBuilder(sb, ItemNames.Stag_Nest_Stag);
            sb.AppendLine();

            sb.AppendLine("----------Keys:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Simple_Key);
            sr.AddItemToStringBuilder(sb, ItemNames.Shopkeepers_Key);
            sr.AddItemToStringBuilder(sb, ItemNames.Elegant_Key);
            sr.AddItemToStringBuilder(sb, ItemNames.Love_Key);
            sr.AddItemToStringBuilder(sb, ItemNames.Tram_Pass);
            sr.AddItemToStringBuilder(sb, ItemNames.Elevator_Pass);
            sr.AddItemToStringBuilder(sb, ItemNames.Lumafly_Lantern);
            sr.AddItemToStringBuilder(sb, ItemNames.Kings_Brand);
            sr.AddItemToStringBuilder(sb, ItemNames.City_Crest);
            sb.AppendLine();

            sb.AppendLine("----------Quest Charms:----------");
            sr.AddItemToStringBuilder(sb, "Grimmchild");
            sr.AddItemToStringBuilder(sb, ItemNames.Spore_Shroom);
            sr.AddItemToStringBuilder(sb, ItemNames.Defenders_Crest);
            sr.AddItemToStringBuilder(sb, ItemNames.Fragile_Strength);
            sr.AddItemToStringBuilder(sb, ItemNames.Fragile_Greed);
            sr.AddItemToStringBuilder(sb, ItemNames.Fragile_Heart);
            sb.AppendLine();

            sb.AppendLine("----------Lifeblood Charms:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Lifeblood_Heart);
            sr.AddItemToStringBuilder(sb, ItemNames.Lifeblood_Core);
            sr.AddItemToStringBuilder(sb, ItemNames.Jonis_Blessing);
            sb.AppendLine();

            sb.AppendLine("----------Useful Charms:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Dashmaster);
            sr.AddItemToStringBuilder(sb, ItemNames.Shaman_Stone);
            sr.AddItemToStringBuilder(sb, ItemNames.Spell_Twister);
            sr.AddItemToStringBuilder(sb, ItemNames.Quick_Slash);
            sr.AddItemToStringBuilder(sb, ItemNames.Wayward_Compass);
            sb.AppendLine();

            sb.AppendLine("----------Baldur Killers:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Grubberflys_Elegy);
            sr.AddItemToStringBuilder(sb, ItemNames.Glowing_Womb);
            sr.AddItemToStringBuilder(sb, ItemNames.Weaversong);
            sr.AddItemToStringBuilder(sb, ItemNames.Mark_of_Pride);
            sb.AppendLine();

            sb.AppendLine("----------Maps:----------");
            sr.AddItemToStringBuilder(sb, ItemNames.Ancient_Basin_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.City_of_Tears_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Crossroads_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Crystal_Peak_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Deepnest_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Fog_Canyon_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Fungal_Wastes_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Greenpath_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Howling_Cliffs_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Kingdoms_Edge_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Queens_Gardens_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Resting_Grounds_Map);
            sr.AddItemToStringBuilder(sb, ItemNames.Royal_Waterways_Map);

            foreach ((string name, Func<LogArguments, bool> test, List<string> items) in API.GetAdditionalCategories())
            {
                if (!test(args)) continue;

                StringBuilder categorySB = new();
                bool addedAny = false;
                categorySB.AppendLine($"----------{name}:----------");
                foreach (string item in items)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        categorySB.AppendLine();
                    }
                    else
                    {
                        addedAny |= sr.AddItemToStringBuilder(categorySB, item);
                    }
                }
                categorySB.AppendLine();

                if (addedAny)
                {
                    sb.Append(categorySB.ToString());
                }
            }

            yield return (sb.ToString(), "CondensedSpoilerLog.txt");
        }
    }
}
