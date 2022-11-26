using ItemChanger.Internal;
using MenuChanger;
using MenuChanger.MenuElements;
using Modding;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using UnityEngine.UI;
using static RandomizerMod.Localization;
using static RandomizerMod.Menu.RandomizerMenu;

namespace CondensedSpoilerLogger
{
    public static class CslMenu
    {
        private static readonly SpriteManager CslSpriteManager = new(typeof(CslMenu).Assembly, "CondensedSpoilerLogger.Resources.");

        internal static void Hook()
        {
            if (!CondensedSpoilerLogger.GS.DisplayWriteLogsButton) return;
            RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(_ => { }, CreateButton);
        }

        private static bool CreateButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            BigButton menuButton = new(landingPage, CslSpriteManager.GetSprite("Quill"), Localize("Write Spoiler Logs"));
            bool writtenLogs = false;
            menuButton.OnClick += () =>
            {
                if (writtenLogs)
                {
                    OpenFile(null, string.Empty, DirectoryOptions.RecentLogFolder);
                    return;
                }
                
                ReflectionHelper.CallMethod(typeof(LogManager), "WriteLogs", rc.args);
                writtenLogs = true;
                menuButton.Button.transform.Find("Text").GetComponent<Text>().text = Localize("Open Log Folder");
                menuButton.Button.transform.Find("Image").GetComponent<Image>().sprite = CslSpriteManager.GetSprite("Map");
            };

            button = menuButton;
            return true;
        }
    }
}
