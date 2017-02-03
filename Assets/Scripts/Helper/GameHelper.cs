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

    internal void enemeyKill(int enemyPoints)
    {
        // ignore asteroid
        if (enemyPoints > 1)
        {
            playerPref.kills++;
            foreach (HF hf in hfs[HF.TYPE_HF.Kill])
            {
                if (hf.nb == playerPref.kills)
                {
                    showHF(hf);
                }
            }
            save();
        }
    }

    internal void collectBonus(string bonusName)
    {
        playerPref.bonus++;
        foreach (HF hf in hfs[HF.TYPE_HF.Bonus])
        {
            if (hf.nb == playerPref.bonus)
            {
                showHF(hf);
            }
        }
        save();
    }

    private void showHF(HF hf)
    {
        achievementPanel.GetComponentsInChildren<Text>()[0].text = hf.description;
        achievementPanel.GetComponentsInChildren<Text>()[1].text = "+" + hf.gold;
        UpdateGold(hf.gold);
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
