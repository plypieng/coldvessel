using System.Collections.Generic;

namespace ColdVessel
{
    public class ColdVesselConfig
    {
        public float CooledPerishRate { get; set; } = 0.55f;
        public int TickIntervalMs { get; set; } = 10000;
        public bool ConsumeOnlyWhenPerishablePresent { get; set; } = true;
        public bool DebugLogging { get; set; } = false;
        public List<ColdVesselCoolant> Coolants { get; set; } = new List<ColdVesselCoolant>
        {
            new ColdVesselCoolant { Code = "foodshelves:cutice", CoolingHours = 12 },
            new ColdVesselCoolant { Code = "game:snowblock", CoolingHours = 6 },
            new ColdVesselCoolant { Code = "game:lakeice", CoolingHours = 48 },
            new ColdVesselCoolant { Code = "game:glacierice", CoolingHours = 48 },
            new ColdVesselCoolant { Code = "game:ice-glacier", CoolingHours = 48 },
            new ColdVesselCoolant { Code = "aldiclasses:rawice", CoolingHours = 48 },
            new ColdVesselCoolant { Code = "game:packedglacierice", CoolingHours = 96 },
            new ColdVesselCoolant { Code = "game:ice-packedglacier", CoolingHours = 96 }
        };
    }

    public class ColdVesselCoolant
    {
        public string Code { get; set; } = "";
        public double CoolingHours { get; set; }
    }
}
