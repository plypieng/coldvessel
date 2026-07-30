using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace ColdVessel
{
    public class ColdVesselModSystem : ModSystem
    {
        public static ColdVesselConfig Config { get; private set; } = new ColdVesselConfig();

        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockEntityBehaviorClass("ColdVessel", typeof(ColdVesselBlockEntityBehavior));
            Config = LoadConfig(api);
            api.Logger.Notification("[coldvessel] Registered ColdVessel block entity behavior");
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            int addedCount = 0;

            foreach (Block block in api.World.Blocks)
            {
                if (block == null || block.Attributes?["coldVesselCompatible"].AsBool(false) != true) continue;

                if (string.IsNullOrEmpty(block.EntityClass))
                {
                    api.Logger.Warning("[coldvessel] Block {0} is marked coldVesselCompatible but has no block entity; skipping", block.Code);
                    continue;
                }

                BlockEntityBehaviorType[] behaviors = block.BlockEntityBehaviors;
                if (HasColdVesselBehavior(behaviors)) continue;

                int existingCount = behaviors?.Length ?? 0;
                BlockEntityBehaviorType[] updatedBehaviors = new BlockEntityBehaviorType[existingCount + 1];
                if (existingCount > 0)
                {
                    Array.Copy(behaviors, updatedBehaviors, existingCount);
                }

                updatedBehaviors[existingCount] = new BlockEntityBehaviorType { Name = "ColdVessel" };
                block.BlockEntityBehaviors = updatedBehaviors;
                addedCount++;
            }

            if (addedCount > 0)
            {
                api.Logger.Notification("[coldvessel] Automatically enabled cooling on {0} tagged vessel block types", addedCount);
            }
        }

        private bool HasColdVesselBehavior(BlockEntityBehaviorType[] behaviors)
        {
            if (behaviors == null) return false;

            foreach (BlockEntityBehaviorType behavior in behaviors)
            {
                if (behavior != null && string.Equals(behavior.Name, "ColdVessel", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private ColdVesselConfig LoadConfig(ICoreAPI api)
        {
            try
            {
                ColdVesselConfig config = api.LoadModConfig<ColdVesselConfig>("coldvessel.json");
                if (config == null)
                {
                    config = new ColdVesselConfig();
                    api.StoreModConfig(config, "coldvessel.json");
                    api.Logger.Notification("[coldvessel] Created default config: ModConfig/coldvessel.json");
                    return config;
                }

                NormalizeConfig(config);
                return config;
            }
            catch (Exception ex)
            {
                api.Logger.Error("[coldvessel] Failed to load ModConfig/coldvessel.json. Using defaults for this session without overwriting the file: {0}", ex);
                ColdVesselConfig config = new ColdVesselConfig();
                NormalizeConfig(config);
                return config;
            }
        }

        private void NormalizeConfig(ColdVesselConfig config)
        {
            if (config.Coolants == null)
            {
                config.Coolants = new ColdVesselConfig().Coolants;
                return;
            }

            List<ColdVesselCoolant> uniqueCoolants = new List<ColdVesselCoolant>();
            HashSet<string> seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ColdVesselCoolant coolant in config.Coolants)
            {
                if (coolant == null || string.IsNullOrEmpty(coolant.Code)) continue;
                if (!seenCodes.Add(coolant.Code)) continue;

                uniqueCoolants.Add(coolant);
            }

            config.Coolants = uniqueCoolants;
        }
    }
}
