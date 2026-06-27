using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace Growrarria.Content.Players
{
    public class CosmicCapePlayer : ModPlayer
    {
        public bool hasCosmicCape = false;

        // Custom Dash State Variables
        private int dashCooldown = 0;
        private int dashActiveTimer = 0;
        private int dashDirection = 0;

        // Double-Tap System Monitors
        private int leftTapTimer = 0;
        private int rightTapTimer = 0;
        private bool oldLeftInput = false;
        private bool oldRightInput = false;

        // Rhythmic timer to track and execute footstep scuff sounds
        private int sprintSoundTimer = 0;

        public override void ResetEffects()
        {
            hasCosmicCape = false;
        }

public override void PreUpdateMovement()
{
    if (!hasCosmicCape)
    {
        dashCooldown = 0;
        dashActiveTimer = 0;
        sprintSoundTimer = 0;
        return;
    }

    if (dashCooldown > 0) dashCooldown--;
    if (leftTapTimer > 0) leftTapTimer--;
    if (rightTapTimer > 0) rightTapTimer--;
    if (sprintSoundTimer > 0) sprintSoundTimer--;

    // 1. PROCESS ACTIVE DASH PHYSICS & ENEMY REAPING
    if (dashActiveTimer > 0)
    {
        dashActiveTimer--;

        Player.velocity.X = dashDirection * 21.0f;
        Player.velocity.Y *= 0.8f;

        Player.immune = true;
        Player.immuneTime = 20;

        Rectangle dashHitbox = Player.getRect();
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && !npc.friendly && npc.damage > 0 && !npc.dontTakeDamage)
            {
                if (dashHitbox.Intersects(npc.getRect()) && npc.immune[Player.whoAmI] == 0)
                {
                    int dashDamage = 85;

                    var hitInfo = npc.CalculateHitInfo(dashDamage, dashDirection, false, 5f, DamageClass.Generic);
                    npc.StrikeNPC(hitInfo);

                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendStrikeNPC(npc, hitInfo);
                    }

                    npc.immune[Player.whoAmI] = 12;

                    // Vortex explosion on enemy hit
                    for (int k = 0; k < 8; k++)
                    {
                        int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.UltraBrightTorch,
                            Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 0, default, 1.8f);
                        Main.dust[d].noGravity = true;
                    }
                }
            }
        }

        if (Main.netMode != NetmodeID.Server)
        {
            // Vortex trails during dash movement
            for (int i = 0; i < 4; i++)
            {
                int d1 = Dust.NewDust(Player.position, Player.width, Player.height, DustID.UltraBrightTorch, -Player.velocity.X * 0.25f, Main.rand.NextFloat(-1.5f, 1.5f), 0, default, 1.6f);
                Main.dust[d1].noGravity = true;
                Lighting.AddLight(Player.position, 0.5f, 0.8f, 2.0f); // Brightened
            }
        }

        if (dashActiveTimer == 0)
        {
            dashCooldown = 40;
            Player.velocity.X *= 0.35f;
        }
        return;
    }

    // 2. DETECT DOUBLE TAP KEY INPUT TRIGGERS
    if (dashCooldown == 0 && Player.dashDelay == 0)
    {
        if (Player.controlLeft && !oldLeftInput)
        {
            if (leftTapTimer > 0) TriggerCustomDash(-1);
            else leftTapTimer = 15;
        }
        if (Player.controlRight && !oldRightInput)
        {
            if (rightTapTimer > 0) TriggerCustomDash(1);
            else rightTapTimer = 15;
        }
    }

    oldLeftInput = Player.controlLeft;
    oldRightInput = Player.controlRight;

    // 3. RUNTIME FOOT LIGHTNING SPRINT TRAILS & SOUNDS
    if (Math.Abs(Player.velocity.Y) <= 0.45f && Math.Abs(Player.velocity.X) >= 4.5f && !Player.mount.Active)
    {
        if (Main.rand.NextBool(2))
        {
            Vector2 footCoordinates = new Vector2(Player.position.X, Player.position.Y + Player.height - 2f);

            int dEnergy = Dust.NewDust(footCoordinates, Player.width, 2, DustID.UltraBrightTorch, -Player.velocity.X * 0.35f, Main.rand.NextFloat(-1.2f, -0.2f), 0, default, 1.5f);
            Main.dust[dEnergy].noGravity = true;

            int dGlow = Dust.NewDust(footCoordinates, Player.width, 2, DustID.UltraBrightTorch, -Player.velocity.X * 0.15f, Main.rand.NextFloat(-0.5f, 0f), 0, default, 1.2f);
            Main.dust[dGlow].noGravity = true;

            Lighting.AddLight(footCoordinates, 0.3f, 0.7f, 1.5f); // Brightened
        }

        if (sprintSoundTimer == 0)
        {
            SoundEngine.PlaySound(SoundID.Item24 with { Volume = 0.35f, Pitch = Main.rand.NextFloat(-0.05f, 0.05f) }, Player.position);
            sprintSoundTimer = 13;
        }
    }
    else
    {
        sprintSoundTimer = 0;
    }

    // Custom Blazing Flight Trails
    if (Player.wingTime < 180 && Player.velocity.Y != 0f && !Player.mount.Active)
    {
        if (Main.rand.NextBool(2))
        {
            int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.UltraBrightTorch, -Player.velocity.X * 0.25f, -Player.velocity.Y * 0.25f, 0, default, 1.4f);
            Main.dust[d].noGravity = true;
            Lighting.AddLight(Player.position, 0.2f, 0.6f, 1.2f); // Brightened
        }
    }

    // Ambient Room Glow & Standing Sparks
    if (hasCosmicCape)
    {
        Lighting.AddLight(Player.Center, 0.3f, 0.7f, 1.5f); // Brightened

        if (Player.velocity.X == 0f && Math.Abs(Player.velocity.Y) <= 0.45f && !Player.mount.Active)
        {
            if (Main.rand.NextBool(10))
            {
                Vector2 sparkSpawnPosition = Main.rand.NextBool(2)
                    ? new Vector2(Player.position.X + Main.rand.Next(-2, Player.width + 2), Player.position.Y + Player.height - 2f)
                    : Player.Center + Main.rand.NextVector2Circular(16f, 22f);

                for (int i = 0; i < 4; i++)
                {
                    int dIdle = Dust.NewDust(sparkSpawnPosition, 2, 2, DustID.UltraBrightTorch, Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.0f, -0.2f), 0, default, 1.1f);
                    Main.dust[dIdle].noGravity = true;
                }
            }
        }
    }
}
        public override void PostUpdate()
        {
            if (hasCosmicCape)
            {
                Lighting.AddLight(Player.Center, 0.6f, 1.2f, 2.0f);
            }
        }

        private void TriggerCustomDash(int dir)
        {
            dashDirection = dir;
            dashActiveTimer = 14; 
            leftTapTimer = 0;
            rightTapTimer = 0;

            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.5f, Pitch = 0.2f }, Player.position);
        }
    }
}