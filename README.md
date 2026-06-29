# Cold Vessel

Cold Vessel adds ice-powered cooling to fired storage vessels in Vintage Story.

It is intentionally narrow: it cools fired/fancy storage vessels, compatible Chonky Vessels, and Seafarer storage amphorae, while leaving chests, crates, CM-Icebox, FoodShelves cabinets/freezers, barrels, and other storage blocks untouched. Cooling is applied on top of normal vessel, cellar, room, and food-category preservation calculations.

## How it works

When a supported vessel contains perishable food and a configured coolant, it consumes one coolant item/block and starts a cooling timer. While active, the vessel applies an extra perish-speed multiplier to its inventory.

Coolants are consumed one at a time only when cooling has run out and perishable food is present. Extra coolant can be stored in the vessel and will wait for later refills.

## Default coolant values

- `foodshelves:cutice` gives 12 in-game hours of cooling.
- `game:snowblock` gives 6 in-game hours of cooling.
- `game:lakeice` gives 48 in-game hours of cooling.
- `game:glacierice` gives 48 in-game hours of cooling.
- `aldiclasses:rawice` gives 48 in-game hours of cooling.
- `game:packedglacierice` gives 96 in-game hours of cooling.

## Compatibility

Required:

- Vintage Story 1.22.x

Optional:

- FoodShelves, for `foodshelves:cutice`
- Chonky Vessels, for supported Chonky storage vessels
- Seafarer, for storage amphorae
- Aldi Classes, for `aldiclasses:rawice`

## Configuration

Configuration is written to `ModConfig/coldvessel.json` after the first launch.

Important options:

- `CooledPerishRate`: extra perish-speed multiplier while cold. Default: `0.55`.
- `ConsumeOnlyWhenPerishablePresent`: avoids wasting coolant in empty vessels. Default: `true`.
- `Coolants`: list of item/block codes and cooling hours.

Coolant codes support exact item/block codes and prefix wildcards. For example, `game:glacierice` matches `game:glacierice` and variant codes that start with `game:glacierice-`, while `game:glacierice*` matches any code that starts with that text.

## Install

Place the release zip in your Vintage Story `Mods` folder and restart the game or server.

For multiplayer, install the same mod version on the server and on clients if your server requires matching client mod lists.
