using System;
using System.Collections.Generic;
using System.Text;
using RandomizerMod.Logging;
using ItemChanger;
using Modding;
using System.Linq;

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

        private static readonly Dictionary<string, List<string>> DefaultMatches = new()
        {
            [ItemNames.Mothwing_Cloak] = new()
            {
                ItemNames.Left_Mothwing_Cloak,
                ItemNames.Right_Mothwing_Cloak,
            },
            [ItemNames.Mantis_Claw] = new()
            {
                ItemNames.Left_Mantis_Claw,
                ItemNames.Right_Mantis_Claw,
            },
            [ItemNames.Crystal_Heart] = new()
            {
                ItemNames.Left_Crystal_Heart,
                ItemNames.Right_Crystal_Heart,
            },
        };

        private static Dictionary<string, List<string>> GetFullMatchDict()
        {
            Dictionary<string, List<string>> matches = new(DefaultMatches);

            foreach ((string parent, List<string> children) in API.GetAdditionalMatches())
            {
                if (!matches.TryGetValue(parent, out List<string> current))
                {
                    current = new();
                }
                matches[parent] = current.Concat(children).ToList();
            }

            return matches;
        }

        private static IEnumerable<string> GetItems(string item)
        {
            Queue<string> itemQueue = new();
            itemQueue.Enqueue(item);

            HashSet<string> seen = new();

            Dictionary<string, List<string>> matches = GetFullMatchDict();

            while (itemQueue.Count > 0)
            {
                string current = itemQueue.Dequeue();
                if (seen.Contains(current)) continue;

                seen.Add(current);
                yield return current;

                if (!matches.TryGetValue(current, out List<string> children)) continue;

                foreach (string child in children) itemQueue.Enqueue(child);
            }
        }

        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            SpoilerReader sr = new(args.ctx);
            StringBuilder sb = new();

            sb.AppendLine($"Condensed spoiler log for seed: {args.gs.Seed}");
            sb.AppendLine();
            sb.Append(CreateCondensedSpoilerBody(sr.AddItemToStringBuilder, args));

            yield return (sb.ToString(), "CondensedSpoilerLog.txt");
        }

        /// <summary>
        /// Create the body of the condensed spoiler log.
        /// </summary>
        /// <param name="writeFunc">Callable that appends a named item to the condensed spoiler.
        /// Most commonly will be SpoilerReader.AddItemToStringBuilder</param>
        /// <returns></returns>
        protected string CreateCondensedSpoilerBody(Func<StringBuilder, string, bool?, bool> writeFunc, LogArguments args)
        {
            bool AddItemToStringBuilder(StringBuilder localSb, string item, bool forceMulti = false)
            {
                bool ret = false;
                foreach (string child in GetItems(item))
                {
                    ret |= writeFunc(localSb, child, forceMulti);
                }
                return ret;
            }

            bool HasRandomizedAny(params string[] items)
            {
                StringBuilder miniSb = new();
                bool any = false;
                foreach (string item in items)
                {
                    any |= AddItemToStringBuilder(miniSb, item);
                }
                return any;
            }


            StringBuilder sb = new();

            sb.AppendLine("----------Major Progression:----------");
            AddItemToStringBuilder(sb, ItemNames.Mothwing_Cloak, forceMulti: true);
            sb.AppendLine();
            AddItemToStringBuilder(sb, ItemNames.Mantis_Claw, forceMulti: true);
            sb.AppendLine();
            AddItemToStringBuilder(sb, ItemNames.Monarch_Wings, forceMulti: true);
            sb.AppendLine();
            AddItemToStringBuilder(sb, ItemNames.Crystal_Heart, forceMulti: true);
            sb.AppendLine();
            AddItemToStringBuilder(sb, ItemNames.Ismas_Tear, forceMulti: true);
            AddItemToStringBuilder(sb, ItemNames.Dream_Nail, forceMulti: true);
            AddItemToStringBuilder(sb, ItemNames.Swim, forceMulti: true);
            sb.AppendLine();

            sb.AppendLine("----------Spells:----------");
            AddItemToStringBuilder(sb, ItemNames.Vengeful_Spirit, forceMulti: true);
            AddItemToStringBuilder(sb, ItemNames.Desolate_Dive, forceMulti: true);
            AddItemToStringBuilder(sb, ItemNames.Howling_Wraiths, forceMulti: true);
            AddItemToStringBuilder(sb, ItemNames.Focus);
            sb.AppendLine();


            bool anySlashes = HasRandomizedAny(ItemNames.Leftslash, ItemNames.Rightslash, ItemNames.Upslash, ItemNames.Downslash); ;
            if (anySlashes)
            {
                sb.AppendLine("----------Nail Slashes:----------");
            }
            else
            {
                sb.AppendLine("----------Nail Arts:----------");
            }
            AddItemToStringBuilder(sb, ItemNames.Cyclone_Slash);
            AddItemToStringBuilder(sb, ItemNames.Great_Slash);
            AddItemToStringBuilder(sb, ItemNames.Dash_Slash);
            if (anySlashes)
            {
                sb.AppendLine();
                AddItemToStringBuilder(sb, ItemNames.Leftslash);
                AddItemToStringBuilder(sb, ItemNames.Rightslash);
                AddItemToStringBuilder(sb, ItemNames.Upslash);
                AddItemToStringBuilder(sb, ItemNames.Downslash);
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
                        addedAny |= AddItemToStringBuilder(categorySB, item);
                    }
                }
                categorySB.AppendLine();

                if (addedAny)
                {
                    sb.Append(categorySB.ToString());
                }
            }

            return sb.ToString();
        }
    }
}
