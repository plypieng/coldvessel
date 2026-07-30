# Adding Ice Cold Vessel Compatibility

Ice Cold Vessel can automatically enable cooling on compatible container block types that opt in with the `coldVesselCompatible` block attribute. Directly appending its `ColdVessel` block entity behavior remains supported for older integrations.

## Container requirements

The target block entity must:

- expose an `IInventory` by implementing `IBlockEntityContainer` or through an `Inventory` property;
- use an inventory with a writable `TransitionableSpeedMulByType` property; and
- store items that use the normal `EnumTransitionType.Perish` transition.

The behavior preserves the inventory's existing perish multiplier and applies the configured cold multiplier on top of it. Container-specific preservation bonuses can therefore continue to work while the vessel is cold.

## Recommended opt-in attribute

Add the following attribute to a compatible container block type:

```json
"attributes": {
  "coldVesselCompatible": true
}
```

If the block already has an `attributes` object, add only the `coldVesselCompatible` property to it. Ice Cold Vessel scans finalized block types, appends its behavior once, and ignores blocks that already have the behavior.

An optional compatibility patch can add the marker without requiring Ice Cold Vessel to know the container's block code:

```json
[
  {
    "dependsOn": [ { "modid": "coldvessel" } ],
    "file": "yourmod:blocktypes/path/to/container.json",
    "op": "add",
    "path": "/attributes/coldVesselCompatible",
    "value": true
  }
]
```

The marker is deliberately opt-in. Generic container properties are also used by chests, crates, cabinets, and dedicated ice storage, so they are not treated as vessel tags automatically.

## Direct behavior patch

Place a patch like this at `assets/yourmod/patches/coldvessel-compat.json`. Replace the `file` value with the asset path of the container block type.

```json
[
  {
    "dependsOn": [ { "modid": "coldvessel" } ],
    "file": "yourmod:blocktypes/path/to/container.json",
    "op": "addmerge",
    "path": "/entityBehaviors",
    "value": []
  },
  {
    "dependsOn": [ { "modid": "coldvessel" } ],
    "file": "yourmod:blocktypes/path/to/container.json",
    "op": "addmerge",
    "path": "/entityBehaviors/-",
    "value": { "name": "ColdVessel" }
  },
  {
    "dependsOn": [ { "modid": "coldvessel" } ],
    "file": "yourmod:blocktypes/path/to/container.json",
    "op": "addmerge",
    "path": "/attributes/handbook",
    "value": {
      "extraSections": [
        {
          "title": "coldvessel:coldvessel-handbook-title",
          "text": "coldvessel:coldvessel-handbook-text"
        }
      ]
    }
  }
]
```

The handbook patch is optional. The first two entries are the ones required for cooling.

## Testing checklist

- Confirm the inactive Ice Cold Vessel tooltip appears on the target container.
- Add perishable food and a supported coolant, then confirm one coolant is consumed.
- Confirm the tooltip becomes active and food's remaining shelf life increases.
- Let cooling expire and confirm the container's original preservation rate returns.
- Test save/reload while cooling is active.
- Test on both server and client when the container mod is required on both sides.

If a container does not expose its inventory in one of the supported forms, open an issue with the block code, block entity class, and container mod version so a small adapter can be considered.
