using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Growrarria.Content.Players
{
    public class FlameEyeDrawLayer : PlayerDrawLayer
    {
        // 1. Position it after the head layer so it renders on top of hats
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            // Sync with your ModPlayer flag
            return drawInfo.drawPlayer.GetModPlayer<FlameEyePlayer>().hasFlameEye 
                && !drawInfo.drawPlayer.dead 
                && !drawInfo.drawPlayer.invis;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            FlameEyePlayer modPlayer = drawPlayer.GetModPlayer<FlameEyePlayer>();

            // Load your new standard accessory sprite
            Texture2D texture = ModContent.Request<Texture2D>("Growrarria/Content/Players/FlameEye_Face").Value;
            int totalFrames = 3;
            int frameHeight = texture.Height / totalFrames;

            // Frame calculation
            int currentEyeFrame = (int)(Main.GameUpdateCount / 6) % totalFrames;
            Rectangle sourceRect = new Rectangle(0, currentEyeFrame * frameHeight, texture.Width, frameHeight);

            // 2. Anchor to headVect for perfect animation syncing
            // headVect automatically includes the head's position during movement/bobbing
            Vector2 drawPos = new Vector2(
                (int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2)),
                (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f)
            ) + drawPlayer.headPosition + drawInfo.headVect;

            // 3. Fine-tuning offsets
            // Adjust these to lock the sprite exactly over the eyes
            float offsetX = 4f * drawPlayer.direction; 
            float offsetY = -8f; 

            drawPos.X += offsetX;
            drawPos.Y += offsetY;

            Vector2 drawOrigin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            DrawData drawData = new DrawData(
                texture,
                drawPos,
                sourceRect,
                Color.White, 
                drawPlayer.headRotation,
                drawOrigin,
                1f,
                drawInfo.playerEffect, 
                0
            );

            // Inherit the player's accessory shader
            drawData.shader = drawPlayer.cFace; 
            drawInfo.DrawDataCache.Add(drawData);
        }
    }
}