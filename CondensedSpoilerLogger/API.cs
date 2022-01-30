using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.ModInterop;
using RandomizerMod.Logging;

namespace CondensedSpoilerLogger
{
    public static class API
    {
        [ModExportName(nameof(CondensedSpoilerLogger))]
        internal static class Export
        {
            public static void AddCategory(string categoryName, Func<LogArguments, bool> test, List<string> entries)
                => API.AddCategory(categoryName, test, entries);
        }

        private static List<(string, Func<LogArguments, bool>, List<string>)> AdditionalCategories = new();

        /// <summary>
        /// Add a category to the Condensed Spoiler Log.
        /// </summary>
        /// <param name="categoryName">The title to give the category.</param>
        /// <param name="test">Return false to skip adding this category to the log.</param>
        /// <param name="entries">A list of items to log in the category.</param>
        public static void AddCategory(string categoryName, Func<LogArguments, bool> test, List<string> entries)
        {
            AdditionalCategories.Add((categoryName, test, entries));
        }

        internal static IEnumerable<(string, Func<LogArguments, bool>, List<string>)> GetAdditionalCategories()
        {
            return AdditionalCategories;
        }
    }
}
