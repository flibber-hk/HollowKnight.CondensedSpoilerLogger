using ItemChanger.Internal;
using MenuChanger;
using MenuChanger.MenuElements;
using Modding;
using RandomizerMod.Logging;
using RandomizerMod.Menu;
using RandomizerMod.RC;
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
                
                ReflectionHelper.CallMethod(typeof(LogManager), "WriteLogs", rc.args);
                writtenLogs = true;
                menuButton.Button.transform.Find("Image").GetComponent<Image>().sprite = CslSpriteManager.GetSprite("Map");
            };

            button = menuButton;
            return CondensedSpoilerLogger.GS.DisplayWriteLogsButton;
        }

        private static void OpenLogFolder() => RandomizerMenu.OpenFile(null, string.Empty, DirectoryOptions.RecentLogFolder);
    }
}
