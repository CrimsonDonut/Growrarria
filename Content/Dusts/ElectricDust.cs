using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Growrarria.Content.Dusts
{
    public class ElectricDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true; // Forces it to float and swirl like actual energy fields
            dust.frame = new Rectangle(0, 0, 10, 10); // Matches the canvas dimensions of your sprite
            
            // Defines the base color of your electric texture
            dust.color = new Color(255, 230, 50); 
        }

        public override bool Update(Dust dust)
        {
            // Physics: Make the dust drift forward based on its velocity and slowly brake over time
            dust.position += dust.velocity;
            dust.velocity *= 0.96f; 

            // Micro-rotation: Spin the electric spark slightly as it drifts
            dust.rotation += dust.velocity.X * 0.05f;

            // Fading out: Shrink the dust slowly. When it gets too tiny, delete it.
            dust.scale *= 0.94f;
            if (dust.scale < 0.3f)
            {
                dust.active = false;
            }

            // Light Emission: Make your yellow particle illuminate the blocks it passes by
            Lighting.AddLight(dust.position, 0.6f, 0.5f, 0.1f);

            return false; // Return false so vanilla physics routines don't override our custom behavior
        }

        // --- FIX: UPDATED TO NULLABLE COLOR RETURN TYPE ---
        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            // The last value '0' is the alpha transparency channel. 
            // Setting it to 0 creates an additive, intense glow that ignores world shadows completely.
            return new Color(255, 230, 50, 0); 
        }
    }
}