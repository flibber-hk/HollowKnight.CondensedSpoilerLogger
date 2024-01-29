using RandomizerMod.Logging;
using RandomizerMod.RC;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CondensedSpoilerLogger.Loggers
{
    public class MultiworldCondensedSpoilerLog : CondensedSpoilerLog
    {
        /// <summary>
        /// Return a sorted list of MW players found in the rando context.
        /// </summary>
        private List<string> GetMultiworldPlayers(LogArguments args)
        {
            HashSet<string> MWPlayers = new();

            Regex mwMatcher = new(@"^([^ ]+)'s [^ ]+_\(\d+\)$");

            foreach (ItemPlacement pmt in args.ctx.itemPlacements)
            {
                Match match = mwMatcher.Match(pmt.Item.Name);
                if (match.Success)
                {
                    MWPlayers.Add(match.Groups[1].Value);
                }
            }

            if (MWPlayers.Count > 0)
            {
                Logger.LogDebug($"Found {MWPlayers.Count} remote players: {string.Join(", ", MWPlayers)}");
            }
            else
            {
                Logger.LogDebug("No MW detected");
            }

            return MWPlayers.OrderBy(x => x).ToList();
        }

        protected override IEnumerable<(string text, string filename)> CreateLogTexts(LogArguments args)
        {
            List<string> mwPlayers = GetMultiworldPlayers(args);
            if (mwPlayers.Count == 0)
            {
                yield break;
            }

            SpoilerReader mainSr = new(args);
            List<SpoilerReader> remoteSrs = mwPlayers.Select(p => new SpoilerReader(args.ctx, p)).ToList();


            bool WriteRemote(StringBuilder sb, string itemName, bool? forceMulti)
            {
                bool ret = false;
                foreach (SpoilerReader sr in remoteSrs)
                {
                    ret |= sr.AddItemToStringBuilder(sb, itemName, forceMulti);
                }
                return ret;
            }

            StringBuilder remoteSb = new();
            remoteSb.AppendLine($"Remote condensed spoiler log for seed: {args.gs.Seed}");
            remoteSb.AppendLine();
            remoteSb.Append(CreateCondensedSpoilerBody(WriteRemote, args));
            yield return (remoteSb.ToString(), "RemoteCondensedSpoilerLog.txt");


            bool WriteAll(StringBuilder sb, string itemName, bool? forceMulti)
            {
                return mainSr.AddItemToStringBuilder(sb, itemName, forceMulti) | WriteRemote(sb, itemName, forceMulti);
            }

            StringBuilder fullSb = new();
            fullSb.AppendLine($"Multiworld condensed spoiler log for seed: {args.gs.Seed}");
            fullSb.AppendLine();
            fullSb.Append(CreateCondensedSpoilerBody(WriteAll, args));
            yield return (fullSb.ToString(), "MultiworldCondensedSpoilerLog.txt");
        }
    }
}