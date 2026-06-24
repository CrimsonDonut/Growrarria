using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Growrarria.Content.Projectiles
{
    public class ZeusLightningBoltProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = 113; 
            Projectile.timeLeft = 600;

            Projectile.extraUpdates = 1; 
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; 
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6; 
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            // 1. GLOWING GHOST TRAIL
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 trailCenterPos = Projectile.oldPos[i] + new Vector2(Projectile.width / 2f, Projectile.height / 2f);
                Vector2 drawPos = trailCenterPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                float trailProgress = (float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                
                // --- FIX: Force trail to use full bright White instead of environmental lightColor ---
                Color ghostGlowColor = Color.White * trailProgress * 0.45f; 
                ghostGlowColor.A = 0; // Clears alpha channel to create an additive glowing bloom look

                Main.EntitySpriteDraw(texture, drawPos, null, ghostGlowColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            // 2. GLOWING MAIN BODY
            Vector2 mainDrawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color mainGlowColor = Color.White; // --- FIX: Ignore darkness completely ---
            
            Main.EntitySpriteDraw(
                texture, 
                mainDrawPos, 
                null, 
                mainGlowColor, 
                Projectile.rotation, 
                drawOrigin, 
                Projectile.scale, 
                SpriteEffects.None, 
                0
            );

            return false; // Return false so the game engine doesn't draw a second, un-glowing javelin underneath ours
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 1.0f);

            if (Main.rand.NextBool(2))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 100, default, 1.0f);
                Main.dust[d].noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill(); 
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.Kill(); 
        }

        public override void OnKill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            for (int i = 0; i < 30; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(6f, 6f);
                int dustType = Main.rand.NextBool() ? DustID.Electric : DustID.Smoke;
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, speed.X, speed.Y, 100, default, 1.5f);
                Main.dust[d].noGravity = true;
            }

            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.LunarFlare, Projectile.damage, Projectile.knockBack * 2f, Projectile.owner);
            }
        }
    }
}