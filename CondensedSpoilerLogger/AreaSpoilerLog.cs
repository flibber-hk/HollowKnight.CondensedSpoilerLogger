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
    internal class AreaSpoilerLog : RandoLogger
    {
        public override void Log(LogArguments args)
        {
            SpoilerReader sr = new(args);
            StringBuilder sb = new();

            Dictionary<string, Dictionary<string, HashSet<string>>> LocationGrouping = new();
            HashSet<string> multiLocations = new();
            foreach (ItemPlacement pmt in args.ctx.itemPlacements)
            {
                string location = pmt.Location.Name;
                string titledArea = pmt.Location.LocationDef.TitledArea;
                string mapArea = pmt.Location.LocationDef.MapArea;

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
            foreach (var mapAreaGroup in LocationGrouping.Values)
            {
                foreach ((string titledArea, HashSet<string> titledAreaLocations) in mapAreaGroup)
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