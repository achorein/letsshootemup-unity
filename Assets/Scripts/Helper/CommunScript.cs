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

    public PlayerPref playerPref;

    public CommunScript()
    {
        ships = new List<Ship>();
        ships.Add(new Ship("playerShip2_orange", 0));
        ships.Add(new Ship("playerShip3_red", 100));
        ships.Add(new Ship("playerShip1_blue", 1000));
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
