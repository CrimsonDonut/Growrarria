using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Growrarria.Content.Projectiles
{
	// This is the "right click" attack. Unlike the beam, this one is a normal physics
	// projectile: it spawns high above the cursor and falls straight down very fast,
	// using tileCollide so it naturally stops on whatever terrain it hits (ceiling,
	// floor, platform, etc.) instead of us having to guess where the ground is.
	public class SkyThunderbolt : ModProjectile
	{
		public override string Texture => "Growrarria/Content/Projectiles/SkyThunderbolt";

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 60;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1; // strikes everything it passes through on the way down
			Projectile.timeLeft = 120;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;

			// extraUpdates makes it run several movement steps per visual frame, so at
			// high velocity it crosses the ~2000px drop in a handful of frames - reads as
			// an instant strike rather than a falling object.
			Projectile.extraUpdates = 6;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.7f);
			if (Main.rand.NextBool(2))
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, 0f, 0, default, 1.6f);
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			for (int i = 0; i < 20; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, 0f, 0, default, 2.2f);
			}
		}
	}
}
