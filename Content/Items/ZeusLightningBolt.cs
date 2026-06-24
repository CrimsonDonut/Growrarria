using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Growrarria.Content.Items
{
    public class ZeusLightningBolt : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 60;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 15;        
            Item.useAnimation = 15;   
            Item.useStyle = ItemUseStyleID.Swing; 
            Item.noMelee = true;
            Item.noUseGraphic = true;             
            Item.knockBack = 5;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Yellow;
            Item.autoReuse = true;    
            Item.shoot = ModContent.ProjectileType<Projectiles.ZeusLightningBoltProjectile>();
            Item.shootSpeed = 20f;
            Item.channel = true; 

            // --- CRITICAL REMOVAL ---
            // Item.UseSound has been removed from here so it doesn't play globally.
            // Sounds are now handled dynamically below in the Shoot hook.
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            // Input lockout if the vortex is already deployed out in the world
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == ModContent.ProjectileType<Projectiles.ZeusLightningBoltSpinProjectile>())
                {
                    return false; 
                }
            }

            if (player.altFunctionUse == 2) // Right Click (Spin Configuration)
            {
                Item.useStyle = ItemUseStyleID.Swing; 
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.autoReuse = false;
            }
            else // Left Click (Normal Throwing Configuration)
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.autoReuse = true;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) // --- RIGHT CLICK FUNCTION ---
            {
                // Play a low, magical charging sound when starting the spin (Sky Dragon's Fury style)
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15, player.Center); 

                Projectile.NewProjectile(source, player.Center, velocity, ModContent.ProjectileType<Projectiles.ZeusLightningBoltSpinProjectile>(), damage, knockback, player.whoAmI);
                return false; 
            }
            else // --- LEFT CLICK FUNCTION ---
            {
                // Play a sharp, organic throwing sound when throwing the lightning javelin
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Thunder, player.Center); 

                return true; 
            }
        }
    }
}