using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class CommunScript : MonoBehaviour {

    private const string PLAYER_KEY = "PLAYER";

    public class Ship
    {
        public string sprite;
        public int price;

        public Ship(string sprite, int price)
        {
            this.sprite = sprite;
            this.price = price;
        }
    }

    public List<Ship> ships;
    public Dictionary<HF.TYPE_HF, List<HF>> hfs;

    public PlayerPref playerPref;

    public CommunScript()
    {
        ships = new List<Ship>();
        ships.Add(new Ship("playerShip1", 0));
        ships.Add(new Ship("playerShip2", 100));
        ships.Add(new Ship("playerShip3", 500));
        ships.Add(new Ship("playerShip4", 1500));
        ships.Add(new Ship("playerShip5", 5000));
        hfs = new Dictionary<HF.TYPE_HF, List<HF>>();
        List<HF> killHfs = new List<HF>();
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "5 kills", 5, 1));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "20 kills", 20, 2));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "100 kills", 100, 5));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "500 kills", 500, 50));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "1000 kills", 1000, 100));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "2500 kills", 2500, 250));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "5000 kills", 5000, 500));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "10000 kills", 10000, 1000));
        hfs.Add(HF.TYPE_HF.Kill, killHfs);
        List<HF> bonusHfs = new List<HF>();
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "5 bonus", 5, 2));
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "20 bonus", 20, 10));
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "100 bonus", 100, 50));
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "500 bonus", 500, 250));
        hfs.Add(HF.TYPE_HF.Bonus, bonusHfs);
    }

    public void load()
    {
        string data = PlayerPrefs.GetString(PLAYER_KEY);
        if (data == null || data.Length == 0)
        {
            playerPref = new PlayerPref();
        }
        else
        {
            BinaryFormatter bf = new BinaryFormatter();
            byte[] b = Convert.FromBase64String(data);
            MemoryStream ms = new MemoryStream(b);

            try
            {
                playerPref = (PlayerPref)bf.Deserialize(ms);
            }
            finally
            {
                ms.Close();
            }
        }
    }

    public void save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream memStr = new MemoryStream();

        try
        {
            bf.Serialize(memStr, playerPref);
            memStr.Position = 0;

            PlayerPrefs.SetString(PLAYER_KEY, Convert.ToBase64String(memStr.ToArray()));
        }
        finally
        {
            memStr.Close();
        }
    }

    public string getCurrentShipSprite()
    {
        return ships[playerPref.currentShip].sprite;
    }
}
