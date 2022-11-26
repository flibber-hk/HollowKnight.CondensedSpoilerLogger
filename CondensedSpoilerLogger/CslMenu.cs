using ItemChanger.Internal;
using MenuChanger;
using MenuChanger.MenuElements;
using Modding;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using System;
using UnityEngine.UI;
using static RandomizerMod.Localization;

namespace CondensedSpoilerLogger
{
    public static class CslMenu
    {
        private static SpriteManager CslSpriteManager = new(typeof(CslMenu).Assembly, "CondensedSpoilerLogger.Resources.");

        internal static void Hook()
        {
            if (!CondensedSpoilerLogger.GS.DisplayWriteLogsButton) return;
            RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(_ => { }, CreateButton);
        }

        private static bool CreateButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            BigButton menuButton = new(landingPage, CslSpriteManager.GetSprite("Quill"), Localize("Write Spoiler Logs Immediately"));
            bool writtenLogs = false;
            menuButton.OnClick += () =>
            {
                if (writtenLogs)
                {
                    CondensedSpoilerLogger.instance.LogWarn("Logs already written");
                    return;
                }
                
                ReflectionHelper.CallMethod(typeof(LogManager), "WriteLogs", rc.args);
                writtenLogs = true;
                menuButton.Button.transform.Find("Text").GetComponent<Text>().color = Colors.FALSE_COLOR;
            };

            button = menuButton;
            return true;
        }
    }
}
