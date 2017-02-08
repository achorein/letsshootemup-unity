using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class GameHelper : CommunScript {
    /// <summary>
    /// Singleton
    /// </summary>
    public static GameHelper Instance;

    public Text scoreText;
    public GameObject achievementPanel;

    private int score = 0;

    void Awake() {
        // Register the singleton
        if (Instance != null) {
            Debug.LogError("Multiple instances of GameHelper!");
        }
        Instance = this;
        load();
        Instance.UpdateScore(0);
        resetAd();
    }

    /// <summary>
    /// Update GUI
    /// </summary>
    /// <param name="points"></param>
    public void UpdateScore(int points) {
        if (scoreText != null) {
            score += points;
            scoreText.text = " " + score.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enemyPoints"></param>
    internal void enemeyKill(int enemyPoints) {
        // ignore asteroid
        if (enemyPoints > 1) {
            playerPref.kills++;
            foreach (HF hf in hfs[HF.TYPE_HF.Kill]) {
                if (hf.nb == playerPref.kills) {
                    showHF(hf);
                }
            }
            save();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bonusName"></param>
    internal void collectBonus(string bonusName) {
        playerPref.bonus++;
        foreach (HF hf in hfs[HF.TYPE_HF.Bonus]) {
            if (hf.nb == playerPref.bonus) {
                showHF(hf);
            }
        }
        save();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hf"></param>
    private void showHF(HF hf) {
        playerPref.gold += hf.gold;
        save();
        // show achievement in Google Games
        if (Social.localUser.authenticated) {
            Social.ReportProgress(hf.id, 100.0f, (bool success) => { });
        }

        achievementPanel.GetComponentsInChildren<Text>()[0].text = hf.description;
        achievementPanel.GetComponentsInChildren<Text>()[1].text = "+" + hf.gold;
        // show panel 
        achievementPanel.SetActive(true);
        foreach (Image img in achievementPanel.GetComponentsInChildren<Image>()) {
            img.CrossFadeAlpha(1, 0.5f, false);
        }
        foreach (Text text in achievementPanel.GetComponentsInChildren<Text>()) {
            text.CrossFadeAlpha(1, 0.5f, false);
        }
        // dismiss in 2 secondes
        Invoke("dismissAchivement", 2);
    }

    /// <summary>
    /// 
    /// </summary>
    public void dismissAchivement() {
        foreach (Image img in achievementPanel.GetComponentsInChildren<Image>()) {
            img.CrossFadeAlpha(0, 1f, false);
        }
        foreach (Text text in achievementPanel.GetComponentsInChildren<Text>()) {
            text.CrossFadeAlpha(0, 1f, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void cameraHitAnimation() {
        //Camera.main.guiTexture.
    }

    /// <summary>
    /// score getter
    /// </summary>
    /// <returns></returns>
    public int getScore() {
        return score;
    }

}
