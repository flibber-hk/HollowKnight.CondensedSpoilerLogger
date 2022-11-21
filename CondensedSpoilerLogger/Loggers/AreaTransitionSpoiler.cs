using Modding;
using RandomizerMod.Logging;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CondensedSpoilerLogger.Loggers
{
    public class AreaTransitionSpoiler : CslLogger
    {
        public override void Log(LogArguments args)
        {
            if ((args.ctx.transitionPlacements?.Count ?? 0) == 0) return;

            // groupedPlacements[mapArea][titledArea] is the placements in that map/titled area
            Dictionary<string, Dictionary<string, HashSet<string>>> sourceGroupedPlacements = new();
            Dictionary<string, Dictionary<string, HashSet<string>>> targetGroupedPlacements = new();

            foreach (TransitionPlacement pmt in args.ctx.transitionPlacements)
            {
                TransitionDef source = pmt.Source.TransitionDef;
                TransitionDef target = pmt.Target.TransitionDef;

                string sourceMsg;
                string targetMsg;
                if (args.gs.TransitionSettings.Coupled && source.Sides == TransitionSides.Both)
                {
                    sourceMsg = $"{source.Name} <---> {target.Name}";
                    targetMsg = $"{target.Name} <---> {source.Name}";
                }
                else
                {
                    sourceMsg = $"{source.Name} ----> {target.Name}";
                    targetMsg = $"{target.Name} <---- {source.Name}";
                }

                sourceGroupedPlacements.AddEntry(
                    source.MapArea ?? AreaSpoilerLogExtensions.Other,
                    source.TitledArea ?? AreaSpoilerLogExtensions.Other,
                    sourceMsg);

                targetGroupedPlacements.AddEntry(
                    target.MapArea ?? AreaSpoilerLogExtensions.Other,
                    target.TitledArea ?? AreaSpoilerLogExtensions.Other,
                    targetMsg);
            }

            MakeLog(args, sourceGroupedPlacements, "Area Transition spoiler log", "SourceTransitionSpoiler.txt");
            MakeLog(args, targetGroupedPlacements, "Area Transition spoiler log", "TargetTransitionSpoiler.txt");
        }

        private void MakeLog(LogArguments args,
            Dictionary<string, Dictionary<string, HashSet<string>>> groupedPlacements,
            string logType, string fileName)
        {
            StringBuilder sb = new();

            sb.AppendLine($"{logType} for seed: {args.gs.Seed}");
            sb.AppendLine();

            foreach (var mapAreaGroup in groupedPlacements.MoveMatchesToEnd(kvp => kvp.Key == AreaSpoilerLogExtensions.Other).Select(kvp => kvp.Value))
            {
                foreach ((string titledArea, HashSet<string> titledAreaPlacements)
                    in mapAreaGroup.MoveMatchesToEnd(kvp => kvp.Key == AreaSpoilerLogExtensions.Other))
                {
                    sb.AppendLine($"{titledArea}:");

                    foreach (string msg in titledAreaPlacements.OrderBy(x => x, SceneOrder.Instance))
                    {
                        sb.AppendLine("- " + msg);
                    }
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            WriteLog(sb.ToString(), fileName);
        }

        private class SceneOrder : IComparer<string>
        {
            private Dictionary<string, int> sceneToIndex;

            private static SceneOrder _instance;
            public static SceneOrder Instance => _instance ??= new();

            private SceneOrder()
            {
                sceneToIndex = new();

                int index = 0;
                foreach (string scene in Data.Rooms.Keys)
                {
                    sceneToIndex[scene] = index;
                    index++;
                }
            }

            public int Compare(string x, string y)
            {
                if (x == y)
                {
                    return Comparer<string>.Default.Compare(x, y);
                }

                string xScene = x.Split('[')[0];
                string yScene = y.Split('[')[0];

                if (xScene == yScene)
                {
                    return Comparer<string>.Default.Compare(x, y);
                }

                bool containsX = sceneToIndex.TryGetValue(xScene, out int xIndex);
                bool containsY = sceneToIndex.TryGetValue(yScene, out int yIndex);

                if (!containsX && !containsY)
                {
                    return Comparer<string>.Default.Compare(x, y);
                }

                if (!containsX)
                {
                    return 1;
                }

                if (!containsY)
                {
                    return -1;
                }

                return xIndex - yIndex;
            }
        }
    }
}
