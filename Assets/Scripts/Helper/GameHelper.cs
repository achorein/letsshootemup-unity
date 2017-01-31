using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class GameHelper : CommunScript
{
    /// <summary>
    /// Singleton
    /// </summary>
    public static GameHelper Instance;

    public Text scoreText;
    public GameObject achievementPanel;

    private int score = 0;

    void Awake()
    {
        // Register the singleton
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of GameHelper!");
        }
        Instance = this;
        load();
        Instance.UpdateScore(0);
    }

    public void UpdateScore(int points)
    {
        if (scoreText != null)
        {
            score += points;
            scoreText.text = " " + score.ToString();
        }
    }

    public void UpdateGold(int bonusGold)
    {
        playerPref.gold += bonusGold;
        save();
    }

    public int getScore()
    {
        return score;
    }


    public void SaveScore()
    {
        if (score > playerPref.bestScore)
        {
            playerPref.bestScore = score;
            save();
        }
    }

    internal void enemeyKill(int points)
    {
        // ignore asteroid
        if (points > 1)
        {
            playerPref.kills++;

            string hfText = "";
            int hfBonus = 0;
            if (playerPref.kills == 5)
            {
                hfBonus = 1;
                hfText = "5 kills !";
            }
            else if (playerPref.kills == 20)
            {
                hfBonus = 5;
                hfText = "20 kills !";
            }
            else if (playerPref.kills == 100)
            {
                hfBonus = 20;
                hfText = "100 kills !";
            }

            if (hfBonus > 0)
            {
                achievementPanel.GetComponentsInChildren<Text>()[0].text = hfText;
                achievementPanel.GetComponentsInChildren<Text>()[1].text = "+" + hfBonus;
                UpdateGold(hfBonus);
                // show panel 
                achievementPanel.SetActive(true);
                foreach (Image img in achievementPanel.GetComponentsInChildren<Image>())
                {
                    img.CrossFadeAlpha(1, 0.5f, false);
                }
                foreach (Text text in achievementPanel.GetComponentsInChildren<Text>())
                {
                    text.CrossFadeAlpha(1, 0.5f, false);
                }
                // dismiss in 2 secondes
                Invoke("dismissAchivement", 2);
            }
            save();
        }
    }

    public void dismissAchivement()
    {
        foreach(Image img in achievementPanel.GetComponentsInChildren<Image>()) {
            img.CrossFadeAlpha(0, 1f, false);
        }
        foreach (Text text in achievementPanel.GetComponentsInChildren<Text>())
        {
            text.CrossFadeAlpha(0, 1f, false);
        }
    }

 
}
