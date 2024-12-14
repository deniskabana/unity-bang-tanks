using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TranslationEnglish
{
    public static class Weapons
    {
        public static Dictionary<string, string> names = new()
        {
            { "grenade", "Grenade" },
            { "flame_gun", "Flame gun" },
            { "large_cannon", "Large cannon" },
            { "small_cannon", "Small cannon" },
            { "triple_shot", "Triple shot" },
        };
    }

    public static class Ui
    {
        public static Dictionary<string, string> inGameText = new()
        {
            { "player_hit_1", "hit!" },
            { "player_hit_2", "ouch" },
            { "player_hit_3", "boom" },
            { "player_hit_4", "bang" },
            { "player_hit_5", "oof" },
            { "player_hit_6", "bam" },
            { "player_hit_dead_1", "dead" },
            { "player_hit_dead_2", "killed" },
            { "player_hit_dead_3", "destroyed" },
            { "player_hit_dead_4", "annihilated" },
            { "player_hit_dead_5", "obliterated" },
            { "player_hit_dead_6", "exterminated" },
        };

        public static Dictionary<string, string> hud = new()
        {
            { "armor", "Armor" },
            { "fuel", "Fuel" },
            { "player", "Player" },
            { "score", "Score" },
            { "phase_movement", "Movement" },
            { "phase_shooting", "Shooting" },
            { "phase_outcome_pls_wait", "Please wait..." },
            { "phase_game_over", "Game over" },
        };

        public static Dictionary<string, string> infoText = new()
        {
            { "game_over", "The game is over!" },
            { "current_player_died", "Oh, dear! You have been killed." },
            { "player_s_died", "%s has fallen in duty!" },
        };

        public static Dictionary<string, string> controls = new()
        {
            { "full_screen", "Full-screen" },
            { "pause", "Pause" },
            { "restart", "Restart" },
            { "resume", "Resume" },
        };
    }
}
