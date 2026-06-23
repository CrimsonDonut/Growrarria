using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Growrarria.Content.Projectiles
{
	// This is the "left click" beam. It does not move like a normal projectile.
	// Each instance only lives for ~2 ticks (set in AI), and the item re-fires a fresh
	// one every Item.useTime ticks for as long as you hold the mouse button - this is
	// the same trick vanilla's Heat Ray uses to fake a continuous beam.
	//
	// Every tick it does a manual raycast from the player to the cursor, stopping at the
	// first solid tile it finds. That tile is what gets mined (via Player.PickTile) and
	// that distance is what the beam is visually stretched to.
	public class BlackThunderboltBeam : ModProjectile
	{
		// How far the beam can reach if nothing solid is in the way. ~44 tiles.
		private const float MaxRange = 700f;

		// How strong the beam mines. 200+ is enough to break almost anything in vanilla,
		// including most Hardmode ores. Lower this if you want it to respect normal
		// pickaxe-power progression instead.
		private const int PickPower = 210;

		// The texture is a vertical spritesheet: 3 frames of equal height stacked on
		// top of each other. FrameDelay is how many ticks each frame is shown for.
		private const int FrameCount = 3;
		private const int FrameDelay = 6;

		// How far out from the player's center the beam originates, along the aim
		// direction - this is what pushes the start point out to the blade instead of
		// the middle of the character. Tune this to taste; bigger = further out.
		private const float MuzzleDistance = 44f;

		// Current length of the beam this tick, used by both drawing and hit detection.
		private float beamLength = 32f;

		public override string Texture => "Growrarria/Content/Projectiles/BlackThunderboltBeam";

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = FrameCount;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1; // fully custom AI, see below
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.tileCollide = false; // we handle tile interaction manually
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (!player.active || player.dead)
			{
				Projectile.Kill();
				return;
			}

			// Keep refreshing the lifespan while the item is actively channeled by its owner.
			if (player.channel && Projectile.owner == Main.myPlayer)
			{
				Projectile.timeLeft = 2;
			}

			// Track the cursor every tick (only the owning client needs to compute this).
			if (Projectile.owner == Main.myPlayer)
			{
				Vector2 toMouse = Main.MouseWorld - player.Center;
				if (toMouse == Vector2.Zero)
				{
					toMouse = -Vector2.UnitY;
				}

				toMouse.Normalize();

				if (Projectile.velocity != toMouse)
				{
					Projectile.velocity = toMouse;
					Projectile.netUpdate = true;
				}
			}

			// Use the player's center to figure out which way to aim (this is the
			// rotation pivot), then push the actual beam origin out along that
			// direction so it starts at the blade instead of the character's middle.
			Vector2 direction = Projectile.velocity;
			if (direction == Vector2.Zero)
			{
				direction = -Vector2.UnitY;
			}

			Vector2 origin = player.Center + direction * MuzzleDistance;

			// Only search out to where the cursor actually is (capped by MaxRange), not
			// always the full MaxRange. This is what makes the beam stop near your cursor
			// in open air instead of always blasting out to max distance.
			float cursorDistance = Vector2.Distance(origin, Main.MouseWorld);
			float castLength = MathHelper.Clamp(cursorDistance, 16f, MaxRange);

			Vector2 endPoint = FindBeamEnd(origin, direction, castLength, out bool hitTile, out Point tileHit);
			beamLength = Vector2.Distance(origin, endPoint);

			Projectile.Center = origin + direction * (beamLength / 2f);
			Projectile.rotation = direction.ToRotation();

			player.ChangeDir(direction.X < 0 ? -1 : 1);
			player.itemRotation = direction.ToRotation();
			if (player.direction < 0)
			{
				player.itemRotation += MathHelper.Pi;
			}

			// Only mine if the tile the beam stopped at is the same tile your mouse is
			// actually over - not just whatever solid block happens to be first in the
			// beam's path. If something else is in the way, the beam still visually
			// stops there, it just won't break it.
			Point mouseTile = new Point((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));
			if (hitTile && player.controlUseItem && tileHit == mouseTile)
			{
				player.PickTile(tileHit.X, tileHit.Y, PickPower);
			}

			// Cycle through the 3 frames based on the global tick counter rather than a
			// per-instance timer - this instance only lives 2 ticks before a fresh one
			// replaces it, so anything counted on "this" object would just reset constantly.
			Projectile.frame = (int)(Main.GameUpdateCount / FrameDelay) % FrameCount;

			Lighting.AddLight(Projectile.Center, 0.55f, 0.15f, 0.65f);
			if (Main.rand.NextBool(2))
			{
				Dust.NewDust(endPoint, 2, 2, DustID.RedTorch, 0f, 0f, 0, default, 1.3f);
			}
		}

		// Steps along the line from start to start + direction * maxLength, 4 pixels at
		// a time, until it finds a solid tile. Returns the point where the beam should stop.
		private Vector2 FindBeamEnd(Vector2 start, Vector2 direction, float maxLength, out bool hitTile, out Point tileHit)
		{
			const float step = 4f;
			hitTile = false;
			tileHit = Point.Zero;
			Vector2 point = start + direction * maxLength;

			for (float traveled = 0f; traveled < maxLength; traveled += step)
			{
				Vector2 sample = start + direction * traveled;
				int tileX = (int)(sample.X / 16f);
				int tileY = (int)(sample.Y / 16f);
				Tile tile = Framing.GetTileSafely(tileX, tileY);

				if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
				{
					hitTile = true;
					tileHit = new Point(tileX, tileY);
					point = sample;
					break;
				}
			}

			return point;
		}

		// Custom hit detection: since this "projectile" is really just a thin line from the
		// player to endPoint, we test that line against each NPC's hitbox instead of using
		// the projectile's tiny default hitbox. This is the same trick ExampleLaser.cs uses.
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Player player = Main.player[Projectile.owner];
			Vector2 direction = Projectile.velocity;
			Vector2 start = player.Center + direction * MuzzleDistance;
			Vector2 end = start + direction * beamLength;
			float point = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 18f, ref point);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			// The texture is 3 frames stacked vertically, each the same height.
			// Projectile.frame (set in AI) picks which one to slice out and draw.
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

			Vector2 direction = Projectile.velocity;
			Vector2 origin = player.Center + direction * MuzzleDistance;
			Vector2 drawOrigin = new Vector2(0f, frameHeight / 2f);
			float scaleX = beamLength / texture.Width;

			Main.EntitySpriteDraw(
				texture,
				origin - Main.screenPosition,
				sourceRect,
				Color.White,
				direction.ToRotation(),
				drawOrigin,
				new Vector2(scaleX, 1f),
				SpriteEffects.None,
				0
			);

			return false;
		}
	}
}