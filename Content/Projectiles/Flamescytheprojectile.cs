using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Growrarria.Content.Projectiles
{
    // Right-click projectile for the FlameScythe.
    // Behaves exactly like the vanilla Death Sickle (spinning arc that curves back
    // toward the player), but with tileCollide = false so it phases through walls.
    public class FlameScytheProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // Trails look great on a spinning sickle - store 10 old positions,
            // mode 2 = record position+rotation each tick (needed for rotating sprites).
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 56;

            // ProjAIStyleID.Sickle is the Death Sickle / Fetid Baghnakhs arc AI:
            // the projectile launches outward, spins, then curves back toward the owner.
            Projectile.aiStyle = ProjAIStyleID.Sickle;
            AIType = ProjectileID.DeathSickle;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            // Infinite penetration - slices through every enemy it touches, just
            // like the vanilla Death Sickle.
            Projectile.penetrate = -1;

            Projectile.timeLeft = 300;

            // Wall-phasing: set to false so tiles are completely ignored.
            Projectile.tileCollide = false;

            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            // Makes the sickle spin visually as it travels.
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Orange fire glow instead of the purple from SkyThunderbolt.
            Lighting.AddLight(Projectile.Center, 1.0f, 0.4f, 0.05f);

            // Fire/ember dust trail.
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f,
                    100, default, 1.4f);
                Main.dust[dust].noGravity = true;
            }

            // Occasional larger ember.
            if (Main.rand.NextBool(6))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.OrangeTorch, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f,
                    0, default, 2.0f);
                Main.dust[dust].noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            for (int i = 0; i < 16; i++)
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                    0, default, 2.0f);
                Main.dust[dust].noGravity = true;
            }
        }

        // Afterimage trail: draw previous positions behind the projectile with
        // decreasing opacity for a motion-blur effect.
        public override bool PreDraw(ref Color lightColor)
        {
            // Let the vanilla aiStyle draw the main sprite; we just add the trail here.
            // Returning true lets the default draw call happen afterwards.
            var texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float alpha = (1f - (float)i / Projectile.oldPos.Length) * 0.5f;
                Color trailColor = lightColor * alpha;

                Vector2 drawPos = Projectile.oldPos[i] + new Vector2(Projectile.width / 2f, Projectile.height / 2f) - Main.screenPosition;
                float rotation = Projectile.oldRot[i];

                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    null,
                    trailColor,
                    rotation,
                    texture.Size() / 2f,
                    Projectile.scale,
                    Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
                    0
                );
            }

            return true;
        }
    }
}