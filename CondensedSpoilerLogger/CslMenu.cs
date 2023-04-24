using ItemChanger.Internal;
using MenuChanger;
using MenuChanger.MenuElements;
using Modding;
using Newtonsoft.Json;
using RandomizerMod.Logging;
using RandomizerMod.Menu;
using RandomizerMod.RC;
using System;
using UnityEngine.UI;
using static RandomizerMod.Localization;
using DirectoryOptions = RandomizerMod.Menu.RandomizerMenu.DirectoryOptions;

namespace CondensedSpoilerLogger
{
    public static class CslMenu
    {
        private static readonly SpriteManager CslSpriteManager = new(typeof(CslMenu).Assembly, "CondensedSpoilerLogger.Resources.");

        internal static void Hook()
        {
            if (!CondensedSpoilerLogger.GS.DisplayWriteLogsButton) return;
            RandomizerMenuAPI.AddStartGameOverride(_ => { }, CreateButton);
        }

        private static bool CreateButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            BigButton menuButton = new(landingPage, CslSpriteManager.GetSprite("Quill"), Localize("Open Log Folder"));
            bool writtenLogs = false;
            menuButton.OnClick += () =>
            {
                OpenLogFolder();

                if (writtenLogs)
                {
                    return;
                }

                LogArguments args = new() { ctx = rc.ctx, gs = rc.gs, randomizer = rc.randomizer };
                ReflectionHelper.GetField<Action<LogArguments>>(typeof(RandoController), nameof(RandoController.OnCreateLogArguments))?.Invoke(args);

                ReflectionHelper.CallMethod(typeof(LogManager), "WriteLogs", args);
                LogManager.Write((tw) =>
                {
                    using JsonTextWriter jtr = new(tw);
                    RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtr, rc.ctx);
                }, "TempRawSpoiler.json");
                writtenLogs = true;
                menuButton.Button.transform.Find("Image").GetComponent<Image>().sprite = CslSpriteManager.GetSprite("Map");
            };

            button = menuButton;
            return CondensedSpoilerLogger.GS.DisplayWriteLogsButton;
        }

        private static void OpenLogFolder() => RandomizerMenu.OpenFile(null, string.Empty, DirectoryOptions.RecentLogFolder);
    }
}
