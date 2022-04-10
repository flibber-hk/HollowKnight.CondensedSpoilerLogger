# HollowKnight.CondensedSpoilerLogger

Hollow Knight Randomizer add-on to create a condensed spoiler log. Adds the following logs:

- CondensedSpoilerLog: Summarises the locations of the most important items.
- NotchCostSpoiler: Lists the randomized notch costs. Only generated with notch costs randomized.
- AreaSortedItemSpoilerLog: Lists the items placed at randomized locations, with the locations grouped by area.
- OrderedItemProgressionSpoilerLog: Lists the randomized item placements in an order that they could be collected to complete the seed.
Progression Sphere 0 consists of the items reachable from the start. Each subsequent sphere consists of all items reachable with the previous sphere's items.
Consequently, it should be possible to complete the seed by collecting items in the order they appear in this log.

Also adds an API for connections to add to the CondensedSpoilerLog.
