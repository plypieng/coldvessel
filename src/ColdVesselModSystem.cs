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
                ColdVesselConfig config = api.LoadModConfig<ColdVesselConfig>("coldvessel.json") ?? new ColdVesselConfig();
                NormalizeConfig(config);
                api.StoreModConfig(config, "coldvessel.json");
                return config;
            }
            catch (Exception ex)
            {
                api.Logger.Warning("[coldvessel] Failed to load config, using defaults: {0}", ex.Message);
                ColdVesselConfig config = new ColdVesselConfig();
                NormalizeConfig(config);
                api.StoreModConfig(config, "coldvessel.json");
                return config;
            }
        }

        private void NormalizeConfig(ColdVesselConfig config)
        {
            if (config.Coolants == null || config.Coolants.Count == 0)
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

            foreach (ColdVesselCoolant coolant in new ColdVesselConfig().Coolants)
            {
                if (seenCodes.Contains(coolant.Code)) continue;
                seenCodes.Add(coolant.Code);
                uniqueCoolants.Add(coolant);
            }

            config.Coolants = uniqueCoolants.Count == 0 ? new ColdVesselConfig().Coolants : uniqueCoolants;
        }
    }
}
