using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Growrarria.Content.Projectiles;

namespace Growrarria.Content.Items
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class FlameScythe : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.Growrarria.hjson' file.
		public override void SetDefaults()
		{
			Item.damage = 120;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 1;
 
			// These should reflect your actual sprite's pixel size (64x56) - matters for
			// the inventory icon scaling and the item's own hitbox.
			Item.width = 64;
			Item.height = 56;
 
			// Low useTime + autoReuse + channel = it keeps re-firing the beam every few ticks
			// for as long as you hold the mouse button, just like vanilla's Heat Ray.
			Item.useTime = 4;
			Item.useAnimation = 4;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.channel = true;
 
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item43;
 
			Item.shoot = ModContent.ProjectileType<BlackThunderboltBeam>();
			Item.shootSpeed = 12f; // only sets the initial aim direction, the beam recalculates every tick
		}
 
		// AltFunctionUse is what lets the right mouse button trigger a different attack.
		// When the player right-clicks, player.altFunctionUse gets set to 2 and
		// that's checked inside Shoot() below.
		public override bool AltFunctionUse(Player player) => true;
 
		// Only applies to useStyle == Shoot (which this item uses). tModLoader's automatic
		// math for where the sprite sits in the player's hand assumes a "standard" sprite
		// size/anchor, and breaks once your art doesn't match that. There's no formula for
		// this - in-game, swap the values below in small steps (try 4-8px at a time) until
		// the scythe's handle actually sits in the character's hand instead of floating off
		// to the side:
		//   X: slides the sprite forward (+) / backward (-) along your aim direction
		//   Y: slides the sprite up (-) / down (+) relative to that aim line
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-10f, 0f);
		}
 
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				// Right click: drop a thunderbolt from the sky onto the cursor's location.
				Vector2 target = Main.MouseWorld;
				Vector2 spawnPosition = new Vector2(target.X, target.Y - 2000f);
				Vector2 strikeVelocity = new Vector2(0f, 60f); // straight down, fast
 
				Projectile.NewProjectile(source, spawnPosition, strikeVelocity, ModContent.ProjectileType<SkyThunderbolt>(), damage, knockback, player.whoAmI);
				return false;
			}
 
			// Left click: the stretching beam towards the cursor.
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}
 
		public override void AddRecipes()
		{
			// Placeholder recipe so you can craft it for testing. Change this to whatever you like.
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HellstoneBar, 12);
			recipe.AddIngredient(ItemID.SoulofNight, 6);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}