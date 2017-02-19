using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class GameOverScript : CommunScript {

    private Button[] buttons;
    private Text[] texts;
    private Image[] images;

    public GameObject gamePanel, player, achivementImage;

    public Text winText, loseText;
    public Text scoreText, trophyText, goldText;
    public Button restartButton, nextButton;

    public int currentLevel = 1;
    internal bool ready = false; // mutex...

    void Awake() {
        // Get the buttons
        buttons = GetComponentsInChildren<Button>();
        texts = GetComponentsInChildren<Text>();
        images = GetComponentsInChildren<Image>(false);

        // Disable them
        HideButtons();
        load();
        ready = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void HideButtons() {
        Time.timeScale = 1;
        resetPanel(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="win"></param>
    public void ShowButtons(bool win) {
        // game pause
        Time.timeScale = 0;
        if (player == null || winText == null || !ready) {
            return;
        }
        ready = false;
        resetPanel(true);
        // main text
        loseText.gameObject.SetActive(!win);
        winText.gameObject.SetActive(win);
        // update UI
        int score = GameHelper.Instance.getScore(); //global
        trophyText.text = GameHelper.Instance.playerPref.bestScore.ToString();

        // Compute bonus / score
        int bonusGold = GameHelper.Instance.computeBonusGoldLevelCompleted();
        goldText.text = " +" + bonusGold;

        if (win) {
            // handle achievments and playerPref
            GameHelper.Instance.levelCompletedWithSuccess(currentLevel, player.GetComponent<PlayerScript>().nbHitTaken);
            
            // show achievment panel
            if (player.GetComponent<PlayerScript>().nbHitTaken == 0) {
                achivementImage.SetActive(true);
                bonusGold += 10;
                score += 100;
            }
            // show "next level" button
            nextButton.gameObject.SetActive(currentLevel <= MAX_LEVEL);
            if (currentLevel == MAX_LEVEL) {
                nextButton.gameObject.GetComponentInChildren<Text>().text = "Run infinity !";
            }
        }
        // show "restart" button when lose
        restartButton.gameObject.SetActive(!win);

        // Update Score UI
        scoreText.text = score.ToString();

        Analytics.CustomEvent("gameOver", new Dictionary<string, object> {
            { "win", win },
            { "currentLevel", currentLevel },
            { "points", score }
        });

        // update playerpref and leaderboard
        GameHelper.Instance.saveScore(score, bonusGold, currentLevel);
        if (!win) {
            // reset score and player if game over
            GameHelper.Instance.reset();
        }
        // Loading Ad
        float waitTime = showAdTimeout();
        if (waitTime >= 0) {
            StartCoroutine(waitForAd(waitTime));
        }
    }

    private IEnumerator waitForAd(float delay) {
        yield return StartCoroutine(WaitForRealTime(delay)); // timescale=0, can't use WaitForTime here
        LoadInterstitialAd();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ExitToMenu() {
        GameHelper.Instance.reset();
        // Reload the level
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    /// <summary>
    /// 
    /// </summary>
    public void NextGame() {
        resetAd();
        LoadingScript.loadLevel = currentLevel + 1;
        if (LoadingScript.loadLevel > MAX_LEVEL) {
            LoadingScript.loadLevel = 0; // infinity
        }
        // Reload the level
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);
    }

    /// <summary>
    /// 
    /// </summary>
    public void RestartGame() {
        resetAd();
        PlayerScript.reset();
        // Reload the level
        SceneManager.LoadScene("Stage" + currentLevel, LoadSceneMode.Single);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    private void resetPanel(bool active) {
        foreach (var b in buttons) {
            b.gameObject.SetActive(active);
        }
        foreach (var t in texts) {
            t.gameObject.SetActive(active);
        }
        foreach (var i in images) {
            if (i.gameObject.GetInstanceID() != GetInstanceID()) {
                i.enabled = active;
            }
        }
        gamePanel.SetActive(!active);
        GetComponent<Image>().enabled = active;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (Time.timeScale == 0) { // menu
                ExitToMenu();
            } else { // in game
                FindObjectOfType<PauseScript>().PauseGame();
            }
        }
    }

}
