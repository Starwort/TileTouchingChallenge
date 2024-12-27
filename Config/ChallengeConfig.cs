using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace TileTouchingChallenge.Config {
    internal class ChallengeConfig : ModConfig {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultListValue("Terraria:")]
        public HashSet<string> Whitelist = [
            "Terraria:" + TileID.Search.GetName(TileID.CandyCaneBlock),
            "Terraria:" + TileID.Search.GetName(TileID.GreenCandyCaneBlock),
        ];

        [DefaultValue(true)]
        public bool ShowInternalName = true;

        [DefaultValue("Terraria:CandyCaneBlock")]
        public string SafetyPlatformBlock = "Terraria:" + TileID.Search.GetName(TileID.CandyCaneBlock);
        [DefaultValue(true)]
        public bool SpawnSafetyPlatform = true;
        [DefaultValue(true)]
        public bool DisableSafetyPlatformAfterSpawn = true;
        [DefaultValue(true)]
        public bool AllowSemiSolids = false;
    }
}
