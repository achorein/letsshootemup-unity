using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GooglePlayGames;

public class MenuScript : CommunScript {
    public Sprite muteSound, normalSound;
    public Button soundButton, rightButton, leftButton, buyButton, startButton, marketButton;
    public Image playerShip;
    public Text scoreText, goldText, marketGoldText;

    public Transform hfPrefab, hfContentView, lbContentView;
    public GameObject mainPanel;

    private int menuPos = 0;

    public TouchGesture.GestureSettings GestureSetting;
    private TouchGesture touch;

    public void Start() {
        //touch = new TouchGesture(this.GestureSetting);
        //StartCoroutine(touch.CheckHorizontalSwipes(
        //    onLeftSwipe: () => { left(); },
        //    onRightSwipe: () => { right(); }
        //    ));
    }

    public void Awake() {
        Time.timeScale = 1;
        load();

        AudioListener.volume = playerPref.volume;
        if (playerPref.volume == 0) {
            soundButton.GetComponent<Image>().sprite = muteSound;
        }

        goldText.text = " " + playerPref.gold;

        menuPos = playerPref.currentShip;
        loadMenuPos();
        PlayGamesPlatform.Activate();
        Social.localUser.Authenticate((bool authSuccess) => { });
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="panel"></param>
    public void StartGame(GameObject panel) {
        playerPref.currentShip = menuPos;
        save();

        if (playerPref.currentMaxLevel > 1) {
            panel.SetActive(true);
            Button[] levelButtons = panel.GetComponentsInChildren<Button>();
            for (int i = 0; i < levelButtons.Length - 1; i++) {
                levelButtons[i].interactable = (i < playerPref.currentMaxLevel);
            }
            //ShowAd();
        } else {
            StartLevel(1);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    public void StartLevel(int level) {
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
    }

    /// <summary>
    /// 
    /// </summary>
    public void loadLeaderBoardPanel() {
        scoreText.text = playerPref.bestScore.ToString();
        // authenticate user
        Social.localUser.Authenticate((bool success) => {
            if (success) {
                // display google games leader board
                PlayGamesPlatform.Instance.ShowLeaderboardUI(LEADERBOARD_ID);
            }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    public void loadHelpPanel() {
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void loadMarketPanel() {
        marketGoldText.text = playerPref.gold.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    public void marketAddGold(GameObject item) {
        var texts = item.GetComponentsInChildren<Text>();

        int gold = int.Parse(texts[0].text.Substring(1));
        float price = float.Parse(texts[1].text.TrimEnd('€'));
        print("gold:" + gold);
        print("price:" + price);
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
        Social.localUser.Authenticate((bool authSuccess) => {
            Social.ShowAchievementsUI();
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
        playerPref.currentMaxLevel = 6;
        save();
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            Application.Quit();
        }
    }

}
