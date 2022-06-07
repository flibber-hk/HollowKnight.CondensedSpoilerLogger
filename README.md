# HollowKnight.CondensedSpoilerLogger

Hollow Knight Randomizer add-on to create a condensed spoiler log. Adds the following logs:

- CondensedSpoilerLog: Summarises the locations of the most important items.
- NotchCostSpoiler: Lists the randomized notch costs. Only generated with notch costs randomized.
- AreaSortedItemSpoilerLog: Lists the items placed at randomized locations, with the locations grouped by area.
- OrderedItemProgressionSpoilerLog: Lists the randomized item placements in an order that they could be collected to complete the seed.
Progression Sphere 0 consists of the items reachable from the start. Each subsequent sphere consists of all items reachable with the previous sphere's items.
There is no guarantee that the order the items appear in this log reflects the order that the randomizer placed them; it is simply one possible such order.
- ReducedItemProgressionSpoilerLog: The same as the ordered spoiler, except it tries to log only log items that it thinks might unlock something later.
It may throw false positives - for example, if it thinks that a mask shard could possibly unlock later items but in fact those items are unlocked
by other progression.

Also adds an API for connections to add to the CondensedSpoilerLog.
