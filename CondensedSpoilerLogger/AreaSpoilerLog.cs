using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RandomizerMod.IC;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerCore;
using RandomizerCore.Logic;
using ItemChanger;
using Modding;

namespace CondensedSpoilerLogger
{
    internal static class AreaSpoilerLogExtensions
    {
        internal static IEnumerable<T> MoveMatchesToEnd<T>(this IEnumerable<T> iter, Func<T, bool> selector)
        {
            using IEnumerator<T> enumerator = iter.GetEnumerator();

            List<T> matches = new();
            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                if (!selector(current))
                {
                    yield return current;
                }
                else
                {
                    matches.Add(current);
                }
            }

            foreach (T match in matches)
            {
                yield return match;
            }
        }
    }


    internal class AreaSpoilerLog : RandoLogger
    {
        internal const string Other = "Other";

        private class AreaNameOrderingStringComparer : IComparer<string>
        {
            int IComparer<string>.Compare(string x, string y)
            {
                if (x == y) return 0;

                if (x == Other) return 1;
                if (y == Other) return -1;

                return StringComparer.InvariantCulture.Compare(x, y);
            }
        }

        private static readonly IComparer<string> AreaNameOrdering = new AreaNameOrderingStringComparer();

        public override void Log(LogArguments args)
        {
            SpoilerReader sr = new(args);
            StringBuilder sb = new();

            Dictionary<string, Dictionary<string, HashSet<string>>> LocationGrouping = new();
            HashSet<string> multiLocations = new();
            foreach (ItemPlacement pmt in args.ctx.itemPlacements)
            {
                string location = pmt.Location.Name;
                string titledArea = pmt.Location.LocationDef.TitledArea ?? Other;
                string mapArea = pmt.Location.LocationDef.MapArea ?? Other;

                if (!LocationGrouping.TryGetValue(mapArea, out var inMapArea))
                {
                    LocationGrouping[mapArea] = inMapArea = new();
                }
                if (!inMapArea.TryGetValue(titledArea, out var inTitledArea))
                {
                    inMapArea[titledArea] = inTitledArea = new();
                }
                inTitledArea.Add(location);

                if (pmt.Location.LocationDef.FlexibleCount)
                {
                    multiLocations.Add(location);
                }
            }

            sb.AppendLine($"Area spoiler log for seed: {args.gs.Seed}");
            sb.AppendLine();
            foreach (var mapAreaGroup in LocationGrouping.MoveMatchesToEnd(kvp => kvp.Key == Other).Select(kvp => kvp.Value))
            {
                foreach ((string titledArea, HashSet<string> titledAreaLocations) 
                    in mapAreaGroup.MoveMatchesToEnd(kvp => kvp.Key == Other))
                {
                    bool anyNonMultiLocations = titledAreaLocations.Any(x => !multiLocations.Contains(x));

                    Action OnTitledArea = null;
                    if (anyNonMultiLocations) sb.AppendLine($"{titledArea}:");
                    sr.Indent += 2;
                    foreach (string location in titledAreaLocations)
                    {
                        if (!multiLocations.Contains(location))
                        {
                            sr.AddLocationToStringBuilder(sb, location, false);
                        }
                        else
                        {
                            OnTitledArea += () =>
                            {
                                sr.AddLocationToStringBuilder(sb, location, true);
                                sb.AppendLine();
                            };
                        }
                    }
                    sr.Indent -= 2;
                    if (anyNonMultiLocations) sb.AppendLine();
                    OnTitledArea?.Invoke();
                }
                sb.AppendLine();
            }

            LogManager.Write(sb.ToString(), "AreaSortedItemSpoilerLog.txt");
        }
    }
}