﻿
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
            var config = ModContent.GetInstance<ChallengeConfig>();
            foreach (Point touched in touchedTiles) {
                var tile = Framing.GetTileSafely(touched);
                if (
                    !tile.HasTile 
                    || !tile.HasUnactuatedTile 
                    || !(
                        Main.tileSolid[tile.TileType] 
                        || (Main.tileSolidTop[tile.TileType] && !config.AllowSemiSolids)
                    )
                ) {
                    continue;
                }
                if (config.Whitelist.Contains(TileName(tile.TileType))) {
                    continue;
                }
                Player.KillMe(
                    PlayerDeathReason.ByCustomReason(
                        Language.GetOrRegister(
                            config.ShowInternalName
                                ? "Mods.TileTouchingChallenge.ChallengeFailedDebug"
                                : "Mods.TileTouchingChallenge.ChallengeFailed"
                        ).Format(
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

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
            spawnedPlatformThisLife = false;
        }
        bool spawnedPlatformThisLife = false;
        void SpawnPlatform() {
            var config = ModContent.GetInstance<ChallengeConfig>();
            if (!config.SpawnSafetyPlatform || spawnedPlatformThisLife) {
                return;
            }
            var x = (int)Player.position.X/16;
            var y = (int)Player.position.Y/16 + Player.height/14;
            var tileToUse = TileType(config.SafetyPlatformBlock ?? "Terraria:" + TileID.Search.GetName(TileID.CandyCaneBlock));
            for (int i = 0; i <= Player.width/10; i++) {
                Main.tile[(int) x + i, (int) y].ResetToType((ushort) tileToUse);
            }
            spawnedPlatformThisLife = true;
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
