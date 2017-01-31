using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class GameHelper : MonoBehaviour {

    private const string GOLD_KEY = "GOLD";
    private const string SCORE_KEY = "SCORE";

    /// <summary>
    /// Singleton
    /// </summary>
    public static GameHelper Instance;

    public Text scoreText;
    public GameObject achievementPanel;

    private int score = 0;
    private int enemyKills = 0;

    void Awake()
    {
        // Register the singleton
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of GameHelper!");
        }
        Instance = this;
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
        int currentGold = PlayerPrefs.GetInt(GOLD_KEY, 0);
        PlayerPrefs.SetInt(GOLD_KEY, currentGold + bonusGold);
    }

    public int getScore()
    {
        return score;
    }


    public void SaveScore()
    {
        if (score > LoadBestScore())
        {
            PlayerPrefs.SetInt(SCORE_KEY, score);
        }
    }

    internal void enemeyKill()
    {
        enemyKills++;
        if (enemyKills == 5)
        {
            achievementPanel.SetActive(true);
            UpdateGold(5);
            Invoke("dismissAchivement", 2);
        }
    }

    public void dismissAchivement()
    {
        achievementPanel.SetActive(false);
    }

    public int LoadBestScore()
    {
        return PlayerPrefs.GetInt(SCORE_KEY, 0);
    }
}
