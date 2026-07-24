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
