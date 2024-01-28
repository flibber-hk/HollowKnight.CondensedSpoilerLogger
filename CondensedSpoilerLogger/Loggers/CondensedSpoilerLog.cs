using System;
using System.Collections.Generic;
using System.Text;
using RandomizerMod.Logging;
using ItemChanger;

namespace CondensedSpoilerLogger.Loggers
{
    public class CondensedSpoilerLog : CslLogger
    {
        public record Category(string Name, Func<LogArguments, bool> Test, List<string> Items);


        private static readonly List<Category> DefaultCategories = new()
        {
            new("Dreamers", (args) => true, new()
            {
                ItemNames.Lurien,
                ItemNames.Monomon,
                ItemNames.Herrah,
                ItemNames.Dreamer,
            }),

            new("White Fragments", (args) => true, new()
            {
                ItemNames.Kingsoul,
            }),

            new("Stag Stations", (args) => true, new()
            {
                ItemNames.Dirtmouth_Stag,
                ItemNames.Crossroads_Stag,
                ItemNames.Greenpath_Stag,
                ItemNames.Queens_Station_Stag,
                ItemNames.Queens_Gardens_Stag,
                ItemNames.City_Storerooms_Stag,
                ItemNames.Kings_Station_Stag,
                ItemNames.Resting_Grounds_Stag,
                ItemNames.Distant_Village_Stag,
                ItemNames.Hidden_Station_Stag,
                ItemNames.Stag_Nest_Stag,
            }),
            new("Keys", (args) => true, new()
            {
                ItemNames.Simple_Key,
                ItemNames.Shopkeepers_Key,
                ItemNames.Elegant_Key,
                ItemNames.Love_Key,
                ItemNames.Tram_Pass,
                ItemNames.Elevator_Pass,
                ItemNames.Lumafly_Lantern,
                ItemNames.Kings_Brand,
                ItemNames.City_Crest,
            }),
            new("Quest Charms", (args) => true, new()
            {
                "Grimmchild",
                ItemNames.Spore_Shroom,
                ItemNames.Defenders_Crest,
                ItemNames.Fragile_Strength,
                ItemNames.Fragile_Greed,
                ItemNames.Fragile_Heart,
            }),
            new("Lifeblood Charms", (args) => true, new()
            {
                ItemNames.Lifeblood_Heart,
                ItemNames.Lifeblood_Core,
                ItemNames.Jonis_Blessing,
            }),
            new("Useful Charms", (args) => true, new()
            {
                ItemNames.Dashmaster,
                ItemNames.Shaman_Stone,
                ItemNames.Spell_Twister,
                ItemNames.Quick_Slash,
                ItemNames.Wayward_Compass,
            }),
            new("Baldur Killers", (args) => true, new()
            {
                ItemNames.Grubberflys_Elegy,
                ItemNames.Glowing_Womb,
                ItemNames.Weaversong,
                ItemNames.Mark_of_Pride,
            }),
        };

        private static IEnumerable<Category> GetCategories()
        {
            foreach (Category cat in DefaultCategories)
            {
                yield return cat;
            }
            
            foreach (Category cat in API.GetAdditionalCategories())
            {
                yield return cat;
            }
        }

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


            foreach (Category cat in GetCategories())
            {
                if (!cat.Test(args)) continue;

                StringBuilder categorySB = new();
                bool addedAny = false;
                categorySB.AppendLine($"----------{cat.Name}:----------");
                foreach (string item in cat.Items)
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
