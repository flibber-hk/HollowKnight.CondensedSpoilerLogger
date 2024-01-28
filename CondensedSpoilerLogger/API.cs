using System;
using System.Collections.Generic;
using MonoMod.ModInterop;
using RandomizerMod.Logging;
using Category = CondensedSpoilerLogger.Loggers.CondensedSpoilerLog.Category;

namespace CondensedSpoilerLogger
{
    public static class API
    {
        private static Modding.ILogger _logger = new Modding.SimpleLogger("CondensedSpoilerLogger.API");

        [ModExportName(nameof(CondensedSpoilerLogger))]
        internal static class Export
        {
            public static void AddCategory(string categoryName, Func<LogArguments, bool> test, List<string> entries)
                => API.AddCategory(categoryName, test, entries);

            /// <summary>
            /// Add a category to the condensed spoiler log - delegates to AddCategory, but only uses
            /// System types so does not create a run-time Randomizer dependency.
            /// </summary>
            public static void AddCategorySafe(string categoryName, Func<bool> test, List<string> entries)
                => API.AddCategory(categoryName, _ => test(), entries);
        }

        private static List<Category> AdditionalCategories = new();

        /// <summary>
        /// Add a category to the Condensed Spoiler Log.
        /// </summary>
        /// <param name="categoryName">The title to give the category.</param>
        /// <param name="test">Return false to skip adding this category to the log.</param>
        /// <param name="entries">A list of items to log in the category.</param>
        public static void AddCategory(string categoryName, Func<LogArguments, bool> test, List<string> entries)
        {
            AdditionalCategories.Add(new(categoryName, test, entries));
            _logger.LogDebug($"Received category {categoryName} with up to {entries.Count} distinct entries.");
        }

        internal static IEnumerable<Category> GetAdditionalCategories()
        {
            return AdditionalCategories;
        }
    }
}
