using System;
using System.Collections.Generic;

[Serializable()]
public class PlayerPref {
    // game
    public List<int> ships = new List<int>();
    public int currentShip = 0;

    // Fun
    public int gold = 0;
    public int bestScore = 0;
    // HF
    public int kills = 0 ;
    
    // settings
    public float volume = 1;
}
