using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Growrarria.Content.Projectiles;

namespace Growrarria.Content.Items
{
    public class FlameScythe : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 70;
            Item.DamageType = DamageClass.Magic; 
            Item.mana = 10;

            Item.width = 64;
            Item.height = 56;

            Item.useTime      = 20;
            Item.useAnimation = 20;
            Item.useStyle     = ItemUseStyleID.Shoot;
            Item.noMelee      = true; 
            Item.autoReuse    = true;

            Item.knockBack = 4f;
            Item.value     = Item.sellPrice(gold: 8);
            Item.rare      = ItemRarityID.LightRed;
            Item.UseSound  = SoundID.Item43;

            Item.shoot      = ModContent.ProjectileType<BlackThunderboltBeam>();
            Item.shootSpeed = 18f;
        }

        // Casts a bright volcanic glow when dropped on the ground
        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1.5f, 0.5f, 0.05f);
        }

        public override bool AltFunctionUse(Player player) => true;

        public override Vector2? HoldoutOffset()
        {
            if (Main.LocalPlayer.altFunctionUse == 2)
                return null; 

            return new Vector2(-10f, 0f);
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click setup
                Item.useStyle     = ItemUseStyleID.Swing;
                Item.useTime      = 25;
                Item.useAnimation = 25;
                Item.noMelee      = false;
                Item.autoReuse    = true;
				

            }
            else
            {
                // Left-click setup
                Item.useStyle     = ItemUseStyleID.Shoot;
                Item.useTime      = 20;
                Item.useAnimation = 20;
                Item.noMelee      = true;  
            }
            return true;
        }

        // EFFECT 1: Spawns fiery dust tracing the slash arc during the Melee Right-Click Swing
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (player.altFunctionUse == 2)
            {
                // Cast light along the swinging hitbox
                Lighting.AddLight(hitbox.Center.ToVector2(), 1.5f, 0.4f, 0.1f);

                // Spawn hyper-vibrant Solar Flare fluid fire along the scythe blade arc
                if (Main.rand.NextBool(1))
                {
                    int d1 = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.SolarFlare, 
                        player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 100, default, Main.rand.NextFloat(1.2f, 1.8f));
                    Main.dust[d1].noGravity = true;
                    Main.dust[d1].velocity *= 0.5f;
                }

                // Mix in intense orange trail sparks
                if (Main.rand.NextBool(2))
                {
                    int d2 = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.OrangeTorch, 
                        0f, 0f, 50, default, Main.rand.NextFloat(1.0f, 1.5f));
                    Main.dust[d2].noGravity = true;
                }
            }
        }

        // EFFECT 2: The correct, verified hook for charging holdout frames (Left-Click Shoot)
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            // Only process if using the holdout/shoot style (Left-Click) and actively animating
            if (player.altFunctionUse != 2 && player.itemAnimation > 0)
            {
                // Cast light at the player's held weapon coordinates
                Lighting.AddLight(player.itemLocation, 1.2f, 0.35f, 0.05f);

                if (Main.rand.NextBool(3))
                {
                    // Emit sparks pushing forward from the item's location
                    int d = Dust.NewDust(player.itemLocation, 8, 8, DustID.SolarFlare, 
                        player.direction * Main.rand.NextFloat(2f, 5f), Main.rand.NextFloat(-2f, 2f), 100, default, 1.2f);
                    Main.dust[d].noGravity = true;
                }
            }
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (player.altFunctionUse == 2)
            {
                target.AddBuff(BuffID.OnFire3, 180); // Hellfire debuff upon physical slash contact
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: spinning fire scythe projectile
                Projectile.NewProjectile(
                    source,
                    player.Center,
                    velocity * 1.5f,
                    ModContent.ProjectileType<FlameScytheProjectile>(),
                    damage * 2,
                    knockback,
                    player.whoAmI
                );
                return false;
            }

            // Left-click: Instant lightning beam
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HellstoneBar, 12);
            recipe.AddIngredient(ItemID.SoulofNight, 6);
			recipe.AddIngredient(ItemID.DeathSickle, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}