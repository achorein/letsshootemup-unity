using System;
using System.Collections.Generic;

[Serializable()]
public class PlayerPref {
    // game
    internal List<int> ships = new List<int>();
    internal int currentShip = 0;
    internal int currentMaxLevel = 1;

    // Fun
    internal int gold = 0;
    internal int bestScore = 0;
    // HF
    internal int kills = 0 ;
    internal int bonus;

    // settings
    internal float volume = 1;
    internal int currentLevelCombo = 0;
    internal int currentWeaponUpgrade = 0;
    internal int currentUntouchable = 0;
    internal bool vibrationOn = true;
    internal int nbGameFinished = 0;
}
