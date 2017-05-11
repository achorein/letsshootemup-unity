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

    public GameObject gamePanel, player;

    public Text winText, loseText, newhighscoreText;
    public Text scoreText, trophyText, goldText, goldBankText;
    public Button restartButton, nextButton;
    public Transform hfPrefab, hfContentView;

    public int currentLevel = 1;
    internal bool ready = false; // mutex...
    internal bool win = false;

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
    public void EndGame(bool hasWin) {
        if (player == null || winText == null || !ready) {
            return;
        }
        if (win) {
            SoundEffectsHelper.Instance.MakeVictorySound();
        } else {
            SoundEffectsHelper.Instance.MakeGameOverSound();
        }
        win = hasWin;
        //Time.timeScale = 0.5f;
        //Invoke("ShowButtons", 1f);
        ShowButtons();
    }

    void ShowButtons() {
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
            HF hf = GameHelper.Instance.levelCompletedWithSuccess(currentLevel, player.GetComponent<PlayerScript>().nbHitTaken);
            if (hf != null) addHf(hf);
            
            // show achievment panel
            if (player.GetComponent<PlayerScript>().nbHitTaken == 0) {
                addHf(hfs[HF.TYPE_HF.Other][0]); // untouchable
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
        if (GameHelper.Instance.saveScore(score, bonusGold, currentLevel)) { 
            newhighscoreText.gameObject.SetActive(true);
        }
        // Update Bank Gold UI
        load();
        goldBankText.text = " " + playerPref.gold;
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
		resetAd();
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

    private void addHf(HF hf) {
        var newHf = Instantiate(hfPrefab) as Transform;
        var hfTexts = newHf.GetComponentsInChildren<Text>();
        hfTexts[0].text = hf.description;
        hfTexts[1].text = "+" + hf.gold;
        newHf.SetParent(hfContentView.transform, false);
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
