using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Growrarria.Content.Players;
using Terraria.DataStructures;

namespace Growrarria.Content.Items
{
    [AutoloadEquip(EquipType.Wings)]
    public class BlazingElectroWings : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Maps premium flight metrics used by vanilla Fishron Wings
            // 180 Ticks max flight time (3 seconds), 9f maximum horizontal speed, 2.5f acceleration engine
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(180, 9f, 2.5f);
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ItemRarityID.Yellow; 
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var wingPlayer = player.GetModPlayer<BlazingElectroWingsPlayer>();
            wingPlayer.hasBlazingElectroWings = true;

            // Replicate Fishron mobility inside liquids
            if (player.wet)
            {
                player.ignoreWater = true; 
            }

            // --- FIX: BALANCED TERRASPARK SPRINT MECHANIC ---
            // Removes base movement exploitation. Matches exact Terraspark thresholds.
            player.accRunSpeed = 6.75f;       
            player.maxRunSpeed = 6.75f;       
            player.runAcceleration *= 1.75f; // Gives snappy traction response when starting a run
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxAscentSpeed, ref float ascentModifier, ref float ascentSpeedUp)
        {
            ascentWhenFalling = 0.85f;
            ascentWhenRising = 0.15f;
            maxAscentSpeed = 6.25f;
            ascentModifier = 2f;
            ascentSpeedUp = 1.5f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "WingsAbility1", "Allows flight and slow fall"));
            tooltips.Add(new TooltipLine(Mod, "WingsAbility2", "Double tap LEFT or RIGHT to perform a damaging lightning phase dash"));
            tooltips.Add(new TooltipLine(Mod, "WingsAbility3", "The wearer can run super fast, kachow! And total liquid movement immunity"));
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FishronWings, 1);
            recipe.AddIngredient(ItemID.FragmentVortex, 10);

            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}