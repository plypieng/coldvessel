# Ice Cold Vessel

Ice Cold Vessel adds ice-powered cooling to fired storage vessels in Vintage Story.

It is intentionally narrow: it cools fired/fancy storage vessels, compatible Chonky Vessels, Upgradeable Storage vessels, and Seafarer storage amphorae, while leaving chests, crates, CM-Icebox, FoodShelves cabinets/freezers, barrels, and other storage blocks untouched. Cooling is applied on top of normal vessel, cellar, room, and food-category preservation calculations.

## How it works

When a supported vessel contains perishable food and a configured coolant, it consumes one coolant item/block and starts a cooling timer. While active, the vessel applies an extra perish-speed multiplier to its inventory.

Filled liquid containers count as perishable when the liquid inside them can spoil, so cooled vessels can also preserve bottles, buckets, jugs, and similar containers holding perishable liquids.

Coolants are consumed one at a time only when cooling has run out and perishable food is present. Extra coolant can be stored in the vessel and will wait for later refills.

## Default coolant values

- `foodshelves:cutice` gives 12 in-game hours of cooling.
- `game:snowblock` gives 6 in-game hours of cooling.
- `game:lakeice` and `lakeice` give 48 in-game hours of cooling.
- `game:glacierice`, `glacierice`, and `game:ice-glacier` give 48 in-game hours of cooling.
- `aldiclasses:rawice` and `rawice` give 48 in-game hours of cooling.
- `game:packedglacierice`, `packedglacierice`, and `game:ice-packedglacier` give 96 in-game hours of cooling.

## Compatibility

Required:

- Vintage Story 1.22.x

Optional:

- FoodShelves, for `foodshelves:cutice`
- Chonky Vessels, including the regular, bundled, and girthy storage vessels
- Upgradeable Storage 1.1.9, for regular and labeled upgradeable storage vessels
- Seafarer, for storage amphorae
- Aldi Classes, for `aldiclasses:rawice`

Container mod authors can add optional support without referencing the Ice Cold Vessel source code. See [COMPATIBILITY.md](COMPATIBILITY.md) for the block entity behavior patch and testing checklist.
Compatible container blocks can opt in automatically with the `coldVesselCompatible` block attribute.

## Configuration

Configuration is written to `ModConfig/coldvessel.json` after the first launch.
After that, the mod only reads the file and does not rewrite user changes during startup. If the JSON is invalid, the mod uses defaults for that session, reports the problem in the log, and leaves the file untouched so it can be repaired.

Important options:

- `CooledPerishRate`: extra perish-speed multiplier while cold. Default: `0.55`.
- `ConsumeOnlyWhenPerishablePresent`: avoids wasting coolant in empty vessels. Default: `true`.
- `Coolants`: list of item/block codes and cooling hours.

Coolant codes support exact item/block codes and prefix wildcards. For example, `game:glacierice` matches `game:glacierice` and variant codes that start with `game:glacierice-`, while `game:glacierice*` matches any code that starts with that text. Unqualified aliases such as `rawice` are included to catch stacks that resolve without a domain.

## Install

Place the release zip in your Vintage Story `Mods` folder and restart the game or server.

For multiplayer, install the same mod version on the server and clients, then restart the server and reconnect.

## Server troubleshooting

- Cooling requires perishable food as well as ice by default. Ice alone leaves the vessel inactive.
- Supported vessels should show an inactive cooling tooltip even before food or ice is added. If the tooltip is missing, check that the client has the mod and that the server's vessel patches loaded.
- A successful C# compilation message does not confirm that the mod's JSON assets loaded. Look for `[coldvessel] Cooling behavior attached to ... block variants` in `Logs/server-main.log`; a count of zero means the container patches were not applied.
- Versions through 0.1.23 were packaged with backslashes inside the ZIP, which can prevent asset discovery on Linux. Upgrade to 0.1.24 or later on the server and clients. As a temporary workaround, repack the archive using forward-slash entry names with `modinfo.json`, `src/`, and `assets/` directly at its root.

## Building a release

From the repository root, run:

```powershell
./scripts/Build-Release.ps1
```

The script reads the version from `modinfo.json`, creates a ZIP with forward-slash entry names, and verifies its complete file list, JSON, and contents against the source tree. It refuses to overwrite an existing archive. Do not use Windows PowerShell `Compress-Archive` to package this mod: the old archives created by that command contained Windows path separators.

To verify a previously built archive:

```powershell
./scripts/Test-Release.ps1 -PackagePath ./coldvessel_0.1.24.zip
```
