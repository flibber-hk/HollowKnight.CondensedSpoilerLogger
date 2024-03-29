﻿using System;
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

namespace CondensedSpoilerLogger.Loggers
{
    internal static class AreaSpoilerLogExtensions
    {
        public const string Other = "Other";

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

        public static void AddEntry<TKey1, TKey2, TValue>(this Dictionary<TKey1, Dictionary<TKey2, HashSet<TValue>>> data, TKey1 key1, TKey2 key2, TValue entry)
        {
            if (!data.TryGetValue(key1, out Dictionary<TKey2, HashSet<TValue>> mapAreaData))
            {
                mapAreaData = new();
                data[key1] = mapAreaData;
            }
            if (!mapAreaData.TryGetValue(key2, out HashSet<TValue> titledAreaData))
            {
                titledAreaData = new();
                mapAreaData[key2] = titledAreaData;
            }

            titledAreaData.Add(entry);
        }
    }


    internal class AreaSpoilerLog : CslLogger
    {
        /// <summary>
        /// Sorts strings by putting Other to the end, and optionally the supplied header to the start.
        /// </summary>
        private class AreaNameOrderingStringComparer : IComparer<string>
        {
            private string header;
            public AreaNameOrderingStringComparer(string header)
            {
                this.header = header;
            }
            public AreaNameOrderingStringComparer() : this(string.Empty) { }

            int IComparer<string>.Compare(string x, string y)
            {
                if (x == y) return 0;

                if (x == AreaSpoilerLogExtensions.Other) return 1;
                if (y == AreaSpoilerLogExtensions.Other) return -1;
                if (x == header) return -1;
                if (y == header) return 1;

                return StringComparer.InvariantCulture.Compare(x, y);
            }
        }

        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            SpoilerReader sr = new(args.ctx);
            StringBuilder sb = new();

            Dictionary<string, Dictionary<string, HashSet<string>>> LocationGrouping = new();
            HashSet<string> multiLocations = new();
            foreach (ItemPlacement pmt in args.ctx.itemPlacements)
            {
                string location = pmt.Location.Name;
                string titledArea = pmt.Location.LocationDef?.TitledArea ?? AreaSpoilerLogExtensions.Other;
                string mapArea = pmt.Location.LocationDef?.MapArea ?? AreaSpoilerLogExtensions.Other;

                LocationGrouping.AddEntry(mapArea, titledArea, location);

                if (pmt.Location.LocationDef?.FlexibleCount ?? false)
                {
                    multiLocations.Add(location);
                }
            }

            sb.AppendLine($"Area spoiler log for seed: {args.gs.Seed}");
            sb.AppendLine();
            foreach ((string mapArea, var mapAreaGroup) in LocationGrouping.OrderBy(kvp => kvp.Key, new AreaNameOrderingStringComparer(LocationNames.Start)))
            {
                foreach ((string titledArea, HashSet<string> titledAreaLocations)
                    in mapAreaGroup.OrderBy(kvp => kvp.Key, new AreaNameOrderingStringComparer(mapArea)))
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

            yield return (sb.ToString(), "AreaSortedItemSpoilerLog.txt");
        }
    }
}