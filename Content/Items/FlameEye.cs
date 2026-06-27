using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Growrarria.Content.Players;

namespace Growrarria.Content.Items
{
    // Eye accessory that grants the flamethrower ability when Q is held.
    // The active logic lives in FlameEyePlayer.UpdateAccessory below.

    public class FlameEye : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
        }

        // This is the correct hook for accessories: called every frame the item is
        // equipped. We flip the flag on our ModPlayer so PostUpdate can check it.
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FlameEyePlayer>().hasFlameEye = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FlameEyeAbility", "Hold [Q] to breathe fire at your cursor. Adds +3 reach"));
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HellstoneBar, 8);
            recipe.AddIngredient(ItemID.Lens, 2);
            recipe.AddIngredient(ItemID.SoulofLight, 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}