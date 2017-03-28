using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GooglePlayGames;
using UnityEngine.Purchasing;
using System;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class MenuScript : CommunScript {
    public Sprite muteSound, normalSound;
    public Button soundButton, rightButton, leftButton, buyButton, startButton, marketButton;
    public Image playerShip;
    public Text scoreText, goldText, marketGoldText;

    public Transform hfPrefab, hfContentView, lbContentView;
    public GameObject mainPanel, messageInfo, levelInfo;
    public Toggle vibrationToggle, infinityModeToggle, normalModeToggle;
    public Text levelText;
    public Toggle[] levelToggle;

    private static bool firstStart = true;
    private int menuPos = 0;
    private int levelPos = 1;

    public TouchGesture.GestureSettings GestureSetting;
    private TouchGesture touch;

    public void Start() {
        //touch = new TouchGesture(this.GestureSetting);
        //StartCoroutine(touch.CheckHorizontalSwipes(
        //    onLeftSwipe: () => { left(); },
        //    onRightSwipe: () => { right(); }
        //    ));
        if (firstStart) {
#if UNITY_ANDROID
            PlayGamesPlatform.Activate();
            Social.localUser.Authenticate((bool authSuccess) => { });
#endif
            firstStart = false;
        }
    }

    public void Awake() {
        Time.timeScale = 1;
        // load player data
        load(); 

        // switch normal/infinity mode from user preference
        infinityModeToggle.isOn = !playerPref.gameModeNormal;
        normalModeToggle.isOn = playerPref.gameModeNormal;
        // load available mode
        infinityModeToggle.interactable = playerPref.currentMaxLevel >= MAX_LEVEL;
        // set current level toggle position
        for (int i = 0; i < levelToggle.Length; i++) {
            levelToggle[i].interactable = i <= playerPref.currentMaxLevel;
            levelToggle[i].isOn = false;
        }
        ToggleLevel(playerPref.currentMaxLevel);
        if (playerPref.currentMaxLevel < MAX_LEVEL) {
            levelToggle[playerPref.currentMaxLevel].isOn = true;
        } else {
            levelToggle[MAX_LEVEL - 1].isOn = true;
        }
        ToggleGameMode(normalModeToggle.isOn);

        // load audio preference
        AudioListener.volume = playerPref.volume;
        if (playerPref.volume == 0) {
            soundButton.GetComponent<Image>().sprite = muteSound;
        }

        // update gold info
        goldText.text = " " + playerPref.gold;

        // update player ship
        menuPos = playerPref.currentShip;
        loadMenuPos();
    }

    /// <summary>
    /// Click right button on main screen
    /// </summary>
    public void right() {
        if (mainPanel.activeSelf) {
            menuPos++;
            startButton.GetComponent<Animator>().enabled = false;
            loadMenuPos();
        }
    }

    /// <summary>
    /// Click left button on main screen
    /// </summary>
    public void left() {
        if (mainPanel.activeSelf) {
            menuPos--;
            startButton.GetComponent<Animator>().enabled = false;
            loadMenuPos();
        }
    }

    /// <summary>
    /// Refresh screen to match ship index
    /// </summary>
    private void loadMenuPos() {
        playerShip.sprite = Resources.Load<Sprite>(ships[menuPos].sprite);
        Image[] speedImgs = playerShip.GetComponentsInChildren<Image>();

        for (int i = 1; i <= 3; i++) {
            if (i <= ships[menuPos].speed) {
                speedImgs[i].color = new Color32(255, 255, 255, 255); // enabled
            } else {
                speedImgs[i].color = new Color32(100, 100, 100, 255); // disabled
            }
        }
        for (int i = 4; i <= 6; i++) {
            if (i-3 <= ships[menuPos].hp) {
                speedImgs[i].color = new Color32(255, 255, 255, 255); // enabled
            } else {
                speedImgs[i].color = new Color32(100, 100, 100, 255); // disabled
            }
        }
        for (int i = 7; i <= 9; i++) {
            if (i-6 <= ships[menuPos].damage) {
                speedImgs[i].color = new Color32(255, 255, 255, 255); // enabled
            } else {
                speedImgs[i].color = new Color32(100, 100, 100, 255); // disabled
            }
        }

        if (menuPos != 0 && !playerPref.ships.Contains(menuPos)) {
            // don't have this ship
            buyButton.gameObject.SetActive(true);
            buyButton.GetComponentInChildren<Text>().text = ships[menuPos].price.ToString();
            buyButton.interactable = playerPref.gold >= ships[menuPos].price;
        } else {
            buyButton.gameObject.SetActive(false);
        }
        startButton.gameObject.SetActive(!buyButton.gameObject.activeSelf);
        marketButton.gameObject.SetActive(buyButton.gameObject.activeSelf);

        rightButton.gameObject.SetActive(menuPos < ships.Count - 1);
        leftButton.gameObject.SetActive(menuPos != 0);
    }

    /// <summary>
    /// 
    /// </summary>
    public void buyShip() {
        if (playerPref.gold >= ships[menuPos].price) {
            playerPref.ships.Add(menuPos);
            playerPref.gold -= ships[menuPos].price;
            goldText.text = playerPref.gold.ToString();
            save();
            buyButton.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
        }
    }

    public void ToggleGameMode(bool normal) {
        playerPref.gameModeNormal = normal;
        save();
        levelInfo.SetActive(normal);
    }

    public void ToggleLevel(int level) {
        levelText.text = " Act I - Level " + level;
        levelPos = level;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="panel"></param>
    public void StartGame(GameObject panel) {
        playerPref.currentShip = menuPos;
        save();
        if (!playerPref.gameModeNormal) {
            StartLevel(0); // infinity
        } else {
            StartLevel(levelPos);
        }
        /*else if (playerPref.currentMaxLevel > 1) {
            panel.SetActive(true);
            Button[] levelButtons = panel.GetComponentsInChildren<Button>();
            for (int i = 0; i < levelButtons.Length - 1; i++) {
                levelButtons[i].interactable = (i <= playerPref.currentMaxLevel);
            }
            //ShowAd();
        } else {
            StartLevel(1);
        }*/
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    public void StartLevel(int level) {
        Analytics.CustomEvent("startLevel", new Dictionary<string, object> {
            { "currentShip", playerPref.currentShip },
            { "currentMaxLevel", playerPref.currentMaxLevel },
        });
        GameHelper.Instance.reset();
        LoadingScript.loadLevel = level;
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ToggleAudio() {
        if (AudioListener.volume == 0) {
            AudioListener.volume = 1;
            soundButton.GetComponent<Image>().sprite = normalSound;
        } else {
            AudioListener.volume = 0;
            soundButton.GetComponent<Image>().sprite = muteSound;
        }
        playerPref.volume = AudioListener.volume;
        save();
        Analytics.CustomEvent("toggleAudio", new Dictionary<string, object> {
            { "audio", playerPref.volume }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    public void loadLeaderBoardPanel(Button button) {
        Analytics.CustomEvent("loadLeaderBoardPanel", new Dictionary<string, object> { });
#if UNITY_ANDROID
        // authenticate user
        Social.localUser.Authenticate((bool success) => {
            if (success) {
                // display google games leader board
                PlayGamesPlatform.Instance.ShowLeaderboardUI(LEADERBOARD_ID);
            } else {
                showMessage("Error when loading Google Games...");
            }
        });
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void loadHelpPanel() {
        Analytics.CustomEvent("loadHelpPanel", new Dictionary<string, object> {});
    }

    public void loadSetupPanel(GameObject setupPanel) {
        Analytics.CustomEvent("loadSetupPanel", new Dictionary<string, object> { });
        vibrationToggle.isOn = playerPref.vibrationOn;
        if (playerPref.premiumMode == 1) {
            Image[] imgs = setupPanel.GetComponentsInChildren<Image>();
            imgs[imgs.Length - 1].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void loadMarketPanel() {
        Analytics.CustomEvent("loadMarketPanel", new Dictionary<string, object> {
            { "ship", menuPos }
        });
        marketGoldText.text = playerPref.gold.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    public void marketIAPSuccess(Product product) {
        if (product.definition.id == "premium.account") {
            playerPref.premiumMode = 1;
        } else {
            int gold = int.Parse(product.definition.id.Replace("gold.", ""));
            playerPref.gold += gold;
            goldText.text = " " + playerPref.gold;
            marketGoldText.text = " " + playerPref.gold;
        }
        save();
        loadMenuPos();
    }

    /// <summary>
    /// 
    /// </summary>
    public void marketAddGold(GameObject item) {
        var texts = item.GetComponentsInChildren<Text>();
        int gold = int.Parse(texts[0].text.Substring(1));
        float price = float.Parse(texts[1].text.TrimEnd('€'));

        playerPref.gold += gold;
        save();

        goldText.text = " " + playerPref.gold;
        marketGoldText.text = " " + playerPref.gold;
        loadMenuPos();
    }

    /// <summary>
    /// 
    /// </summary>
    public void loadHFPanel() {
        Analytics.CustomEvent("loadHFPanel", new Dictionary<string, object> { });
        foreach (Transform child in hfContentView.transform) {
            Destroy(child.gameObject);
        }
        hfContentView.DetachChildren();
        // Level wins
        foreach (HF hf in hfs[HF.TYPE_HF.Level]) {
            var newHf = Instantiate(hfPrefab) as Transform;
            var hfTexts = newHf.GetComponentsInChildren<Text>();
            hfTexts[0].text = hf.description;
            hfTexts[1].text = " ";
            hfTexts[2].text = "+" + hf.gold;
            if ((!hf.special && playerPref.currentMaxLevel < hf.nb)
                || (hf.special && playerPref.currentLevelCombo < hf.nb)) { // need to be disabled ?
                newHf.GetComponent<Image>().color = new Color32(100, 100, 100, 255);
            }
            newHf.SetParent(hfContentView.transform, false);
        }
        // Weapon upgrade
        foreach (HF hf in hfs[HF.TYPE_HF.Weapon]) {
            var newHf = Instantiate(hfPrefab) as Transform;
            var hfTexts = newHf.GetComponentsInChildren<Text>();
            hfTexts[0].text = hf.description;
            hfTexts[1].text = " ";
            hfTexts[2].text = "+" + hf.gold;
            if (playerPref.currentWeaponUpgrade < hf.nb) { // need to be disabled ?
                newHf.GetComponent<Image>().color = new Color32(100, 100, 100, 255);
            }
            newHf.SetParent(hfContentView.transform, false);
        }
        // Untouchable
        foreach (HF hf in hfs[HF.TYPE_HF.Other]) {
            var newHf = Instantiate(hfPrefab) as Transform;
            var hfTexts = newHf.GetComponentsInChildren<Text>();
            hfTexts[0].text = hf.description;
            hfTexts[1].text = " ";
            hfTexts[2].text = "+" + hf.gold;
            if (playerPref.currentUntouchable < hf.nb) { // need to be disabled ?
                newHf.GetComponent<Image>().color = new Color32(100, 100, 100, 255);
            }
            newHf.SetParent(hfContentView.transform, false);
        }
        // kills
        foreach (HF hf in hfs[HF.TYPE_HF.Kill]) {
            var newHf = Instantiate(hfPrefab) as Transform;
            var hfTexts = newHf.GetComponentsInChildren<Text>();
            hfTexts[0].text = hf.description;
            hfTexts[1].text = " " + ((playerPref.kills > hf.nb) ? hf.nb : playerPref.kills) + "/" + hf.nb;
            hfTexts[2].text = "+" + hf.gold;
            if (playerPref.kills < hf.nb) { // need to be disabled ?
                newHf.GetComponent<Image>().color = new Color32(100, 100, 100, 255);
            }
            newHf.SetParent(hfContentView.transform, false);
        }
        // bonus
        foreach (HF hf in hfs[HF.TYPE_HF.Bonus]) {
            var newHf = Instantiate(hfPrefab) as Transform;
            var hfTexts = newHf.GetComponentsInChildren<Text>();
            hfTexts[0].text = hf.description;
            hfTexts[1].text = " " + ((playerPref.bonus > hf.nb) ? hf.nb : playerPref.bonus) + "/" + hf.nb;
            hfTexts[2].text = "+" + hf.gold;
            if (playerPref.bonus < hf.nb) { // need to be disabled ?
                newHf.GetComponent<Image>().color = new Color32(100, 100, 100, 255);
            }
            newHf.SetParent(hfContentView.transform, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void loadGoogleGamesAchievementUI() {
        Analytics.CustomEvent("loadGoogleGamesAchievementUI", new Dictionary<string, object> { });
        Social.localUser.Authenticate((bool authSuccess) => {
            if (authSuccess) {
                Social.ShowAchievementsUI();
            } else {
                showMessage("Error when loading Google Games...");
            }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetGame() {
        playerPref = new PlayerPref();
        save();
        soundButton.GetComponent<Image>().sprite = normalSound;
        Awake();
    }

    /// <summary>
    /// 
    /// </summary>
    public void unlockLevels() {
        playerPref.currentMaxLevel = MAX_LEVEL;
        playerPref.gold = 10000;
        save();
        Awake();
    }

    public void toggleVibration(Boolean isOn) {
        playerPref.vibrationOn = isOn;
        save();
        Analytics.CustomEvent("toggleVibration", new Dictionary<string, object> {
            { "vibration", playerPref.vibrationOn }
        });
    }

    public void showMessage(string text) {
        messageInfo.GetComponentInChildren<Text>().text = text;
        messageInfo.SetActive(true);
        messageInfo.GetComponent<Image>().CrossFadeAlpha(1, 0.5f, false);
        messageInfo.GetComponentInChildren<Text>().CrossFadeAlpha(1, 0.5f, false);
        // dismiss in 2 secondes
        Invoke("dismissMessage", 2);
    }

    internal void dismissMessage() {
        messageInfo.GetComponent<Image>().CrossFadeAlpha(0, 1f, false);
        messageInfo.GetComponentInChildren<Text>().CrossFadeAlpha(0, 1f, false);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            Application.Quit();
        }
    }

}
