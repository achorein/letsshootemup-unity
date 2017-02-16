using System;
using UnityEngine;
using UnityEngine.UI;

public class GameHelper : CommunScript {
    /// <summary>
    /// Singleton
    /// </summary>
    public static GameHelper Instance;

    public Text scoreText;
    public GameObject achievementPanel;
    public GameObject player, bombUi;

    private static int score = 0;
    /// <summary>
    /// number of levels wins consecutively 
    /// </summary>
    private static int comboLevel = 0;

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

    internal void levelCompleted(int level, int nbTaken) {
        comboLevel++;
        if (comboLevel > playerPref.currentLevelCombo) {
            playerPref.currentLevelCombo = comboLevel;
        }
        if (level + 1 > GameHelper.Instance.playerPref.currentMaxLevel) {
            playerPref.currentMaxLevel = level + 1;
        }
        foreach (HF hf in hfs[HF.TYPE_HF.Level]) {
            if (hf.nb == level) {
                if (hf.special == false || comboLevel == hf.nb) {
                    // show achievement in Google Games
                    if (Social.localUser.authenticated) {
                        Social.ReportProgress(hf.id, 100.0f, (bool success) => { });
                    }
                }
            }
        }
        // untouchable achievement
        if (nbTaken == 0 && Social.localUser.authenticated) {
            playerPref.currentUntouchable++;
            Social.ReportProgress(hfs[HF.TYPE_HF.Other][0].id, 100.0f, (bool success) => { });
        }
        // gain +1 life 
        PlayerScript.lastLife++;
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

    /// <summary>
    /// Reset to initial state (score 0, etc)
    /// </summary>
    public void reset() {
        score = 0;
        comboLevel = 0;
        PlayerScript.reset();
        resetAd();
    }

    internal void upgradeWeapon(string lastWeaponName) {
        if (Social.localUser.authenticated) {
            Social.ReportProgress(hfs[HF.TYPE_HF.Weapon][0].id, 100.0f, (bool success) => { });
        }
        playerPref.currentWeaponUpgrade++;
        save();
    }

    public void triggerBomb() {
        SpecialEffectsHelper.Instance.Bomb(player.transform.position);
        bombUi.SetActive(false);
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            player.transform.position,
            10f,
            1 << LayerMask.NameToLayer("Enemies") // colide only with layer Enemies
        );
        foreach (Collider2D collider in hits) {
            HealthScript health = collider.gameObject.GetComponent<HealthScript>();
            if (health != null) {
                health.Damage(20);
            }
        }
        player.gameObject.GetComponent<PlayerScript>().nbBombs--;
    }

    internal void pickupBomb() {
        bombUi.SetActive(true);
    }
}
