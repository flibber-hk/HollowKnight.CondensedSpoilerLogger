using System;
using MonoMod.ModInterop;

namespace CondensedSpoilerLogger.Interop
{
    internal static class DebugMod
    {
        [ModImportName("DebugMod")]
        private static class DebugImport
        {
            public static Action<Action, string, string, bool> AddActionToKeyBindList = null;
            public static Action<string> LogToConsole = null;
        }
        static DebugMod()
        {
            typeof(DebugImport).ModInterop();
        }

        /// <summary>
        /// Add an action to the keybinds list.
        /// </summary>
        public static void AddActionToKeyBindList(Action method, string name)
            => DebugImport.AddActionToKeyBindList?.Invoke(method, name, "CSL", true);

        public static void LogToConsole(string msg)
            => DebugImport.LogToConsole?.Invoke(msg);
    }
}