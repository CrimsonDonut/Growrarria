using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Growrarria.Content.Projectiles;

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
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing; // This triggers the arm movement
            Item.noMelee = true;
            Item.noUseGraphic = true;             // Hides the item sprite during the throw
            Item.autoReuse = true;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item60; 
            Item.shoot = ModContent.ProjectileType<ZeusLightningBoltProjectile>();
            Item.shootSpeed = 30f;                // Increase this for a faster projectile
        }
    }
}