using System;
using System.Collections.Generic;
using System.Linq;

namespace CondensedSpoilerLogger.Util
{
    internal static class CollectionUtil
    {
        public static IEnumerator<T> Chain<T>(this IEnumerable<IEnumerator<T>> enumerators)
        {
            foreach (IEnumerator<T> enumerator in enumerators)
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }

                enumerator.Dispose();
            }

        }
    }
}
