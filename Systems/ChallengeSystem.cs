
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TileTouchingChallenge.Config;

namespace TileTouchingChallenge.Systems {
    internal class ChallengeSystem : ModPlayer {
        static string DisplayName(int id) {
            ModTile? tile = TileLoader.GetTile(id);
            return tile != null ? tile.Name : TileID.Search.GetName(id);
        }
        static string TileName(int id, int frameY = -1) {
            ModTile? tile = TileLoader.GetTile(id);
            var variant = frameY >= 0 ? $":{frameY}" : "";
            if (tile is not null) {
                return $"{tile.Mod.Name}:{tile.Name}{variant}";
            } else {
                return "Terraria:" + TileID.Search.GetName(id) + variant;
            }
        }
        static int TileType(string id) {
            string[] parts = id.Split(':');
            string mod = parts[0];
            string name = parts[1];
            if (mod == "Terraria") {
                return TileID.Search.GetId(name);
            } else {
                return TileID.Search.GetId(mod + '/' + name);
            }
        }
        void KillIfFailed() {
            List<Point> touchedTiles = [];
            Collision.GetEntityEdgeTiles(touchedTiles, Player);
            foreach (Point touched in touchedTiles) {
                var tile = Framing.GetTileSafely(touched);
                if (!tile.HasTile || !tile.HasUnactuatedTile) {
                    continue;
                }
                var config = ModContent.GetInstance<ChallengeConfig>();
                if (config.Whitelist.Contains(TileName(tile.TileType))) {
                    continue;
                }
                Player.KillMe(
                    PlayerDeathReason.ByCustomReason(
                        Language.GetTextValue(
                            config.ShowInternalName
                                ? "Mods.TileTouchingChallenge.ChallengeFailedDebug"
                                : "Mods.TileTouchingChallenge.ChallengeFailed",
                            Player.name,
                            DisplayName(tile.TileType),
                            TileName(tile.TileType)
                        )
                    ),
                    9999,
                    0
                );
            }
        }
        void SpawnPlatform() {
            var config = ModContent.GetInstance<ChallengeConfig>();
            if (!config.SpawnSafetyPlatform) {
                return;
            }
            var x = Player.position.X;
            var y = Player.position.Y + Player.height;
            var tileToUse = TileType(config.SafetyPlatformBlock ?? "Terraria:" + TileID.Search.GetName(TileID.CandyCaneBlock));
            for (int i = 0; i <= Player.width; i++) {
                try {
                    Main.tile[(int) x, (int) y].ResetToType((ushort) tileToUse);
                } catch (IndexOutOfRangeException) { return; }
            }
            if (config.DisableSafetyPlatformAfterSpawn) {
                config.SpawnSafetyPlatform = false;
            }
        }
        public override void PostUpdateMiscEffects() {
            SpawnPlatform();
            KillIfFailed();
        }
    }
}
