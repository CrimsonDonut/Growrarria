using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Growrarria.Content.Projectiles;

namespace Growrarria.Content.Players
{
    public class FlameEyePlayer : ModPlayer
    {
        public bool hasFlameEye = false;
        private int attackCooldown = 0;
        private int dustTimer = 0;

        public override void ResetEffects()
        {
            hasFlameEye = false;
        }

        public override void PostUpdateEquips()
        {
            if (hasFlameEye)
            {
                Player.tileRangeX += 3;
                Player.tileRangeY += 3;
                Player.blockRange += 3;
            }
        }

        public override void PostUpdate()
        {
            if (!hasFlameEye)
            {
                dustTimer = 0;
                return;
            }

            // Dust effects — small embers drifting upward from the eye area.
            dustTimer++;
            if (dustTimer >= 8) // spawn a dust particle every 8 ticks
            {
                dustTimer = 0;

                // Eye position: slightly in front of and above the player center
                Vector2 eyePos = Player.MountedCenter
                    + new Vector2(4f * Player.direction, -10f);

                int d = Dust.NewDust(
                    eyePos, 2, 2,
                    DustID.Torch,
                    Main.rand.NextFloat(-0.6f, 0.6f) + Player.velocity.X * 0.3f,
                    Main.rand.NextFloat(-1.5f, -0.5f), // drift upward
                    100, default, Main.rand.NextFloat(0.6f, 1.1f));
                Main.dust[d].noGravity = true;

                // Occasional larger ember
                if (Main.rand.NextBool(3))
                {
                    int d2 = Dust.NewDust(
                        eyePos, 2, 2,
                        DustID.OrangeTorch,
                        Main.rand.NextFloat(-0.4f, 0.4f),
                        Main.rand.NextFloat(-2f, -0.8f),
                        0, default, Main.rand.NextFloat(0.8f, 1.4f));
                    Main.dust[d2].noGravity = true;
                }
            }

            // Fire breath on Q
            if (attackCooldown > 0) attackCooldown--;

            if (Keyboard.GetState().IsKeyDown(Keys.Q) && attackCooldown <= 0 && !Player.dead)
            {
                if (!Main.drawingPlayerChat && !Main.editSign && !Main.editChest)
                {
                    SpawnFlameBreath();
                    attackCooldown = 5;
                }
            }
        }

        private void SpawnFlameBreath()
        {
            Vector2 spawnPos = Player.MountedCenter + new Vector2(6f * Player.direction, -2f);
            Vector2 targetDirection = Main.MouseWorld - spawnPos;
            targetDirection.Normalize();
            Vector2 spreadVelocity = (targetDirection * 8.5f).RotatedBy(Main.rand.NextFloat(-0.12f, 0.12f));

            if (Main.myPlayer == Player.whoAmI)
            {
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    spawnPos,
                    spreadVelocity,
                    ModContent.ProjectileType<FlameBreath>(),
                    25,
                    1.5f,
                    Player.whoAmI
                );
            }
        }
    }
}