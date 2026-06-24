using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Growrarria.Content.Projectiles
{
    public class BlackThunderboltBeam : ModProjectile
    {
        private int Bounces
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetDefaults()
        {
            // Maximized dimensions for a huge footprint canvas
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            // Instantly updates across the room
            Projectile.extraUpdates = 100;
            Projectile.timeLeft = 300;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            // 1. Massive Light Engine: Casts an overwhelming volcanic glow
            Lighting.AddLight(Projectile.Center, 2.5f, 0.7f, 0.05f);

            // Calculate perpendicular vectors to puff out aura details to the sides
            Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X);
            if (perp != Vector2.Zero) perp.Normalize();

            // 2. The Core Thick Black Thunderbolt (Dense void center)
            for (int i = 0; i < 2; i++)
            {
                Vector2 spawnPos = Projectile.Center + (perp * Main.rand.NextFloat(-5f, 5f)) - new Vector2(4);
                int blackDust = Dust.NewDust(spawnPos, 8, 8, DustID.Smoke, 
                    0f, 0f, 230, Color.Black, Main.rand.NextFloat(1.6f, 2.4f));
                
                Main.dust[blackDust].noGravity = true;
                Main.dust[blackDust].velocity *= 0.01f; // Lock perfectly straight
                Main.dust[blackDust].color = new Color(5, 5, 8, 255); 
            }

            // 3. Supercharged Fiery Outer Aura (Chaotic, hyper-dense plasma cloud)
            // Splitting into three hyper-active brackets for intense texture layering
            
            // Layer A: Bright Solar Flare Liquid Fire (Extremely bright & energetic)
            if (Main.rand.NextBool(1)) // Spawns on every single sub-tick loop
            {
                Vector2 auraPos1 = Projectile.Center + (perp * Main.rand.NextFloat(-12f, 12f));
                int solarDust = Dust.NewDust(auraPos1, 1, 1, DustID.SolarFlare, 
                    0f, 0f, 50, default, Main.rand.NextFloat(1.2f, 1.8f));
                Main.dust[solarDust].noGravity = true;
                // Gives it a high-velocity violent outwards crackle
                Main.dust[solarDust].velocity = perp * Main.rand.NextFloat(-1.5f, 1.5f); 
            }

            // Layer B: Blazing Orange Plasma Standard Fire
            if (Main.rand.NextBool(2))
            {
                Vector2 auraPos2 = Projectile.Center + (perp * Main.rand.NextFloat(-16f, 16f));
                int orangeDust = Dust.NewDust(auraPos2, 1, 1, DustID.OrangeTorch, 
                    0f, 0f, 80, default, Main.rand.NextFloat(1.5f, 2.2f));
                Main.dust[orangeDust].noGravity = true;
                Main.dust[orangeDust].velocity = perp * Main.rand.NextFloat(-0.8f, 0.8f);
            }

            // Layer C: Volcanic Deep Red Fringe Heat Sparks
            if (Main.rand.NextBool(3))
            {
                Vector2 auraPos3 = Projectile.Center + (perp * Main.rand.NextFloat(-22f, 22f));
                int redDust = Dust.NewDust(auraPos3, 1, 1, DustID.Crimstone, 
                    0f, 0f, 100, default, Main.rand.NextFloat(1.0f, 1.6f));
                Main.dust[redDust].noGravity = true;
                Main.dust[redDust].velocity = perp * Main.rand.NextFloat(-2.0f, 2.0f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Bounces++;
            if (Bounces > 5)
            {
                Projectile.Kill();
                return false;
            }

            // Massive magmatic detonation at collision points
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 18; i++)
                {
                    int d1 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 
                        Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, Color.Black, 1.8f);
                    Main.dust[d1].noGravity = true;
                    Main.dust[d1].color = new Color(5, 5, 8, 255);

                    int d2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SolarFlare, 
                        Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 50, default, 2.0f);
                    Main.dust[d2].noGravity = true;

                    int d3 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.OrangeTorch, 
                        Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 50, default, 1.6f);
                    Main.dust[d3].noGravity = true;
                }
            }

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240); // Hellfire debuff
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = -0.2f }, Projectile.position);
            
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 40; i++)
                {
                    int d = Dust.NewDust(Projectile.Center, 1, 1, DustID.SolarFlare,
                        Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-7f, 7f), 0, default, 2.2f);
                    Main.dust[d].noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}