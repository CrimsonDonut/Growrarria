using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Growrarria.Content.Projectiles;

namespace Growrarria.Content.Players
{
    public class FlameEyePlayer : ModPlayer
    {
        public bool hasFlameEye = false;
        private int flameCooldown = 0;
        private const int FlameInterval = 2; 

        public override void ResetEffects()
        {
            hasFlameEye = false;
        }

        // FIX: Re-engineered to make the player's eyes literally ignite with a persistent flame
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (hasFlameEye) 
            {
                // Core baseline head tracking tracking[cite: 3]
                Vector2 headCenter = Player.MountedCenter + new Vector2(0f, Player.gfxOffY) + new Vector2(0f, -12f);
                float offsetX = 1.5f * Player.direction; 

                if (Player.bodyFrame.Y > 0)
                {
                    offsetX -= 1.0f * Player.direction;
                }

                // Precision eye coordinate location[cite: 3]
                Vector2 eyePosition = headCenter + new Vector2(offsetX, -4f);

                // 1. DENSE OUTER EYE FLAME (Spawns heavily for a continuous burning effect)
                if (Main.rand.NextBool(1, 2)) 
                {
                    // Dictates an upward lift velocity vector so it behaves like real fire
                    Vector2 flameVelocity = new Vector2(-0.15f * Player.direction, -1.4f) + Player.velocity * 0.15f;

                    int d = Dust.NewDust(
                        eyePosition - new Vector2(2, 2), // Tighten boundary directly over the socket
                        4, 4, 
                        DustID.Torch, 
                        flameVelocity.X, 
                        flameVelocity.Y, 
                        100, default, Main.rand.NextFloat(0.8f, 1.2f)
                    );
                    Main.dust[d].noGravity = true; 
                }

                // 2. INNER WHITE-HOT PUPIL GLOW (Adds extreme heat depth to the eye core)
                if (Main.rand.NextBool(1, 3))
                {
                    Vector2 coreVelocity = new Vector2(-0.05f * Player.direction, -0.6f);

                    int d = Dust.NewDust(
                        eyePosition, 0, 0, 
                        DustID.SolarFlare, 
                        coreVelocity.X, 
                        coreVelocity.Y, 
                        50, default, Main.rand.NextFloat(0.5f, 0.75f)
                    );
                    Main.dust[d].noGravity = true;
                }

                // 3. AMPLIFIED LIGHT EMISSION
                // Cranked up the luminosity values so your character casts a strong fire glow onto nearby tiles
                Lighting.AddLight(eyePosition, 1.3f, 0.55f, 0.1f);
            }
        }

// Inside Growrarria.Content.Players.FlameEyePlayer

        public override void PostUpdate()
        {
            if (!hasFlameEye || Player.whoAmI != Main.myPlayer)
            {
                if (!hasFlameEye) flameCooldown = 0;
                return;
            }

            if (flameCooldown > 0)
            {
                flameCooldown--;
                return;
            }

            if (!Keyboard.GetState().IsKeyDown(Keys.Q))
                return;

            // --- NEW: SOUND ENGINE TRIGGER ---
            // We play the sound specifically when the cooldown hits zero (the exact moment of firing).
            // Using SoundID.Item34 (a common flamethrower/fire sound) with a slight pitch variation
            // makes it feel more dynamic.
            if (Main.rand.NextBool(3)) // Play sound every 3rd shot for a rhythmic "hiss" rather than spamming
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item34 with { 
                    Volume = 0.5f, 
                    Pitch = Main.rand.NextFloat(-0.2f, 0.2f) 
                }, Player.Center);
            }
            // ---------------------------------

            flameCooldown = FlameInterval;

            Vector2 toMouse = Main.MouseWorld - Player.Center;
            if (toMouse == Vector2.Zero)
                toMouse = -Vector2.UnitY;
            toMouse.Normalize();

            float spreadAngle = Main.rand.NextFloat(-MathHelper.ToRadians(4.5f), MathHelper.ToRadians(4.5f));
            Vector2 spreadVelocity = toMouse.RotatedBy(spreadAngle) * Main.rand.NextFloat(14.0f, 19.0f);

            Vector2 spawnPos = Player.Center + toMouse * 16f;
            
            Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                spawnPos,
                spreadVelocity,
                ModContent.ProjectileType<FlameBreath>(),
                14, 
                1.2f, 
                Player.whoAmI
            );

            Player.ChangeDir(Main.MouseWorld.X > Player.Center.X ? 1 : -1);
        }
    }
}