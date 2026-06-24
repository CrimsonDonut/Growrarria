using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Growrarria.Content.Projectiles
{
    public class ZeusLightningBoltSpinProjectile : ModProjectile
    {
        private ref float ChargeTimer => ref Projectile.ai[0];
        private ref float IsDetached => ref Projectile.ai[1]; 
        private int flightTimer = 0;
        private const float MaxChargeTime = 120f; 

        private SlotId loopingSoundSlot = SlotId.Invalid;

        public override void SetDefaults()
        {
            Projectile.width = 110;
            Projectile.height = 110;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ownerHitCheck = false; 

            Projectile.aiStyle = 119; 
            AIType = ProjectileID.MonkStaffT3; 
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            float chargeProgress = ChargeTimer / MaxChargeTime;
            if (IsDetached > 0f) chargeProgress = 1f;

            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 1.2f);

            // --- PHASE 1: CHARGING UP (ANCHORED) ---
            if (IsDetached == 0f)
            {
                Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true);
                player.statDefense += 10;

                if (player.itemAnimation == 1)
                {
                    player.itemAnimation = player.itemAnimationMax;
                    player.itemTime = player.itemTimeMax;
                }

                Vector2 targetDir = Main.MouseWorld - player.MountedCenter;
                player.ChangeDir(targetDir.X > 0f ? 1 : -1);

                if (Main.dedServ == false && !SoundEngine.TryGetActiveSound(loopingSoundSlot, out _))
                {
                    SoundStyle spinLoopStyle = SoundID.Item15 with { IsLooped = true, Volume = 0.8f };
                    loopingSoundSlot = SoundEngine.PlaySound(spinLoopStyle, Projectile.Center);
                }

                if (SoundEngine.TryGetActiveSound(loopingSoundSlot, out ActiveSound activeSound))
                {
                    activeSound.Position = Projectile.Center;
                }

                if (ChargeTimer < MaxChargeTime)
                {
                    ChargeTimer++;
                    if (ChargeTimer == MaxChargeTime)
                    {
                        SoundEngine.PlaySound(SoundID.Item101, Projectile.Center); 
                        for (int i = 0; i < 20; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2Circular(7f, 7f);
                            Dust.NewDust(player.position, player.width, player.height, DustID.Electric, speed.X, speed.Y, 100, default, 1.3f);
                        }
                    }
                }

                if (Projectile.owner == Main.myPlayer && !player.controlUseTile)
                {
                    if (ChargeTimer >= MaxChargeTime)
                    {
                        IsDetached = 1f; 
                        StopLoopingSound(); 
                        SoundEngine.PlaySound(SoundID.Item105, Projectile.Center); 
                    }
                    else
                    {
                        Projectile.Kill();
                        return;
                    }
                }
            }
            // --- PHASE 2: DETACHED AND FLYING ---
            else if (IsDetached == 1f)
            {
                flightTimer++;
                Vector2 targetPos = Main.MouseWorld;
                Vector2 moveDirection = targetPos - Projectile.Center;
                float distance = moveDirection.Length();

                if (flightTimer >= 90 || distance < 12f) 
                {
                    IsDetached = 2f;
                }
                else
                {
                    moveDirection.Normalize();
                    float travelSpeed = 16f;
                    Projectile.velocity = ((Projectile.velocity * 12f) + (moveDirection * travelSpeed)) / 13f;
                }
            }
            // --- PHASE 3: BOOMERANG RETURNING ---
            else if (IsDetached == 2f)
            {
                Vector2 returnDirection = player.Center - Projectile.Center;
                float distanceToPlayer = returnDirection.Length();

                if (distanceToPlayer < 24f)
                {
                    Projectile.Kill();
                    return;
                }

                returnDirection.Normalize();
                float returnAcceleration = 22f; 
                Projectile.velocity = ((Projectile.velocity * 10f) + (returnDirection * returnAcceleration)) / 11f;
            }

            if (IsDetached > 0f)
            {
                Projectile.position += Projectile.velocity;
            }

            // High-intensity electric dust engine
            int extraDustDensity = chargeProgress >= 1f ? 3 : (chargeProgress >= 0.5f ? 2 : 1);
            for (int k = 0; k < extraDustDensity; k++)
            {
                Vector2 circularOffset = Main.rand.NextVector2CircularEdge(55f, 55f);
                Vector2 ringDustPos = Projectile.Center + circularOffset;
                int d1 = Dust.NewDust(ringDustPos, 0, 0, DustID.Electric, 0f, 0f, 100, default, Main.rand.NextFloat(0.7f, 1.0f));
                Main.dust[d1].noGravity = true;
                Main.dust[d1].velocity = circularOffset.RotatedBy(MathHelper.PiOver2 * player.direction) * 0.06f;

                if (Main.rand.NextBool(3))
                {
                    Vector2 randomEdgePoint = Main.rand.NextVector2CircularEdge(20f, 20f);
                    int d2 = Dust.NewDust(Projectile.Center, 0, 0, DustID.Electric, 0f, 0f, 100, default, Main.rand.NextFloat(0.5f, 0.9f));
                    Main.dust[d2].noGravity = true;
                    Main.dust[d2].velocity = Vector2.Normalize(randomEdgePoint) * Main.rand.NextFloat(4f, 8f);
                }
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 plumePos = Projectile.Center + Main.rand.NextVector2Circular(35f, 35f);
                int d3 = Dust.NewDust(plumePos, 0, 0, DustID.Electric, 0f, 0f, 150, default, 0.6f);
                Main.dust[d3].noGravity = true;
                Main.dust[d3].velocity *= 0.2f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            StopLoopingSound();
        }

        private void StopLoopingSound()
        {
            if (SoundEngine.TryGetActiveSound(loopingSoundSlot, out ActiveSound activeSound))
            {
                activeSound.Stop();
                loopingSoundSlot = SlotId.Invalid;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Player player = Main.player[Projectile.owner];

            float spinSpeedMultiplier = 0.15f + ((ChargeTimer / MaxChargeTime) * 0.40f);
            if (IsDetached > 0f) spinSpeedMultiplier = 0.55f;

            float spinRotation = Main.GameUpdateCount * spinSpeedMultiplier * player.direction;

            Vector2 drawPos;
            if (IsDetached == 0f)
            {
                Vector2 mountedCenter = player.RotatedRelativePoint(player.MountedCenter, true);
                drawPos = mountedCenter - Main.screenPosition + new Vector2(0f, player.gfxOffY);
            }
            else
            {
                drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, player.gfxOffY);
            }

            // --- FIX: FORCE VORTEX TEXTURE TO EMIT FULL BRIGHT LIGHT ---
            Color vortexGlowColor = Color.White * 0.9f;
            vortexGlowColor.A = 0; // Turns on additive blending mode (makes electrical lines flare up intensely)

            Main.EntitySpriteDraw(
                texture, 
                drawPos, 
                null, 
                vortexGlowColor, 
                spinRotation, 
                drawOrigin, 
                Projectile.scale, 
                SpriteEffects.None, 
                0
            );

            return false; 
        }
    }
}