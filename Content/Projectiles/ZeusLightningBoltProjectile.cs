using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Growrarria.Content.Projectiles
{
    public class ZeusLightningBoltProjectile : ModProjectile
    {
        // Tracks trailing projectile spawn cycles
        private int spearSpawnTimer = 0;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = 113; // Uses Daybreak embedded logic
            Projectile.timeLeft = 600;

            Projectile.extraUpdates = 1; 
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; 
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6; 
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            // Clean ghosting trail fix (centered position calculation)
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 trailCenterPos = Projectile.oldPos[i] + new Vector2(Projectile.width / 2f, Projectile.height / 2f);
                Vector2 drawPos = trailCenterPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                float trailProgress = (float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                Color ghostColor = Projectile.GetAlpha(lightColor) * trailProgress * 0.45f; 

                Main.EntitySpriteDraw(
                    texture, 
                    drawPos, 
                    null, 
                    ghostColor, 
                    Projectile.rotation, 
                    drawOrigin, 
                    Projectile.scale, 
                    SpriteEffects.None, 
                    0
                );
            }

            return true; 
        }

        public override void AI()
        {
            // Standard electrical dust particle behavior
            if (Main.rand.NextBool(2))
            {
                int d = Dust.NewDust(
                    Projectile.position, 
                    Projectile.width, 
                    Projectile.height, 
                    DustID.Electric, 
                    Projectile.velocity.X * 0.1f, 
                    Projectile.velocity.Y * 0.1f, 
                    100, 
                    default, 
                    1.0f
                );
                Main.dust[d].noGravity = true;
            }

            // --- NEW: SPAWN EXISTING STORMSPEAR TRAIL ---
            // ai[0] == 0f means the projectile is flying freely through the air and hasn't stuck to a target yet.
            if (Projectile.ai[0] == 0f && Main.myPlayer == Projectile.owner)
            {
                spearSpawnTimer++;
                if (spearSpawnTimer >= 8) // Spawns a trailing spear every 8 frames
                {
                    spearSpawnTimer = 0;

                    // Calculate a position slightly behind the current lightning bolt
                    Vector2 spawnBehind = Projectile.Center - Vector2.Normalize(Projectile.velocity) * 24f;

                    // Match the velocity or fire slightly slower so they look like a continuous drop trail
                    Vector2 trailingVelocity = Projectile.velocity * 0.6f;

                    // Change ProjectileID.StormSpearShot to whatever your exact modded type/vanilla ID is.
                    // (Assuming vanilla's Storm Spear projectile projectile ID, which is ProjectileID.StormSpearShot)
                    int trailProj = Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        spawnBehind,
                        trailingVelocity,
                        ProjectileID.MartianTurretBolt, 
                        Projectile.damage / 2, // Deal half damage for trailing projectiles
                        Projectile.knockBack * 0.5f,
                        Projectile.owner
                    );

                    // Prevent trailing bolts from cluttering up target immunity frames
                    Main.projectile[trailProj].noDropItem = true;
                }
            }
        }

        // --- NEW: LANDING EXPLOSION (TACTILE/TILE IMPACT) ---
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Forces the Daybreak AI to terminate immediately so it executes our Explode function
            Projectile.Kill();
            return true;
        }

        // --- NEW: LANDING EXPLOSION (NPC TARGET IMPACT) ---
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // If you want it to blow up instantly like a missile instead of sticking inside them like a Daybreak,
            // uncomment the line below:
            // Projectile.Kill(); 
        }

        public override void OnKill(int timeLeft)
        {
            // Play an explosive lightning boom sound on landing
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // Visual shockwave explosion particles
            for (int i = 0; i < 30; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(6f, 6f);
                
                // Mix fire/smoke dust with electrical discharge particles
                int dustType = Main.rand.NextBool() ? DustID.Electric : DustID.Smoke;
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, speed.X, speed.Y, 100, default, 1.5f);
                Main.dust[d].noGravity = true;
            }

            // Optional: Spawn a brief explosion hit-box that damages nearby targets
            if (Projectile.owner == Main.myPlayer)
            {
                // Spawns a standard blast explosion projectile zone
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ProjectileID.LunarFlare, // Swap to an explosive type or custom explosion if needed
                    Projectile.damage,
                    Projectile.knockBack * 2f,
                    Projectile.owner
                );
            }
        }
    }
}