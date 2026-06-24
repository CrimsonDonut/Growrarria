using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Growrarria.Content.Projectiles
{
    public class FlameBreath : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1; 
            Projectile.friendly = true;
            Projectile.hostile = false; 
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45; 
            
            // FIX: Turned tile collision back on so fire blocks on walls
            Projectile.tileCollide = true; 
            Projectile.ignoreWater = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0; 
        }

        public override void AI()
        {
            float age = 45f - Projectile.timeLeft;

            if (age <= 1f)
            {
                Projectile.velocity *= Main.rand.NextFloat(0.9f, 1.25f);
                Projectile.timeLeft = Main.rand.Next(42, 52);
            }

            if (age > 4f)
            {
                Projectile.velocity *= 0.975f; 
            }

            Projectile.scale = 0.35f + (age / 45f) * 1.4f;

            int currentSize = (int)(14 * Projectile.scale);
            if (currentSize > 44) currentSize = 44; 
            
            Projectile.position = Projectile.Center;
            Projectile.width = currentSize;
            Projectile.height = currentSize;
            Projectile.Center = Projectile.position;

            Lighting.AddLight(Projectile.Center, 1.2f, 0.6f, 0.15f);

            if (Main.rand.NextBool(3))
            {
                int d = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f,
                    100, default, Projectile.scale * 1.1f);
                Main.dust[d].noGravity = true;
            }
        }

        // FIX: Handles hitting solid tiles gracefully
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Instead of instantly destroying the projectile, we kill its speed.
            // This makes the fire look like it's realistically "splattering" or pooling against the wall.
            Projectile.velocity = Vector2.Zero; 
            
            // Returning false prevents the game from hard-deleting the projectile instantly,
            // allowing the current flame puff to naturally fade out in place.
            return false; 
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[ProjectileID.Flames].Value;
            
            int totalFrames = 7; 
            int frameHeight = texture.Height / totalFrames;
            float age = 45f - Projectile.timeLeft;

            int currentFrame = 0;
            if (age >= 30f) currentFrame = 5 + ((int)(age - 30f) / 4) % 2;
            else if (age >= 12f) currentFrame = 3 + ((int)(age - 12f) / 5) % 2;
            else currentFrame = ((int)age / 4) % 3;

            Rectangle sourceRectangle = new Rectangle(0, currentFrame * frameHeight, texture.Width, frameHeight);
            Vector2 drawOrigin = new Vector2(texture.Width / 2f, frameHeight / 2f);
            
            float rotation = Projectile.velocity.LengthSquared() > 0.1f 
                ? Projectile.velocity.ToRotation() + MathHelper.PiOver2 
                : Projectile.rotation + MathHelper.PiOver2;

            float opacity = 1f;
            if (age > 28f)
            {
                opacity = 1f - ((age - 28f) / 17f);
                if (opacity < 0f) opacity = 0f;
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Layer 1: Outer Shroud
            Color outerColor = new Color(230, 50, 15, 55) * opacity; 
            Main.EntitySpriteDraw(texture, drawPos, sourceRectangle, outerColor, rotation, drawOrigin, Projectile.scale * 1.25f, SpriteEffects.None, 0);

            // Layer 2: Main Body
            Color midColor = new Color(255, 140, 25, 135) * opacity;
            Main.EntitySpriteDraw(texture, drawPos, sourceRectangle, midColor, rotation, drawOrigin, Projectile.scale * 0.95f, SpriteEffects.None, 0);

            // Layer 3: Blazing Core
            Color coreColor = new Color(255, 250, 190, 215) * opacity;
            Main.EntitySpriteDraw(texture, drawPos, sourceRectangle, coreColor, rotation, drawOrigin, Projectile.scale * 0.6f, SpriteEffects.None, 0);

            return false; 
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 240);
        }
    }
}