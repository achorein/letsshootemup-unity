using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : CommunScript {
    public Sprite muteSound, normalSound;
    public Button soundButton, rightButton, leftButton, buyButton, startButton;
    public Image playerShip;
    public Text scoreText, goldText;

    public Transform hfPrefab, hfContentView, lbContentView;

    private int menuPos = 0;

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
    }

    /// <summary>
    /// Click right button on main screen
    /// </summary>
    public void right() {
        menuPos++;
        startButton.GetComponent<Animator>().enabled = false;
        loadMenuPos();
    }

    /// <summary>
    /// Click left button on main screen
    /// </summary>
    public void left() {
        menuPos--;
        startButton.GetComponent<Animator>().enabled = false;
        loadMenuPos();
    }

    /// <summary>
    /// Refresh screen to match ship index
    /// </summary>
    private void loadMenuPos() {
        playerShip.sprite = Resources.Load<Sprite>(ships[menuPos].sprite);
        if (menuPos != 0 && !playerPref.ships.Contains(menuPos)) {
            buyButton.gameObject.SetActive(true);
            buyButton.GetComponentInChildren<Text>().text = ships[menuPos].price.ToString();
            buyButton.interactable = playerPref.gold >= ships[menuPos].price;
        } else {
            buyButton.gameObject.SetActive(false);
        }
        startButton.gameObject.SetActive(!buyButton.gameObject.activeSelf);

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
        print("playerPref.currentMaxLevel: " + playerPref.currentMaxLevel);
        if (playerPref.currentMaxLevel > 1) {
            panel.SetActive(true);
            Button[] levelButtons = panel.GetComponentsInChildren<Button>();
            for (int i = 0; i < levelButtons.Length - 1; i++) {
                levelButtons[i].interactable = (i < playerPref.currentMaxLevel);
                //var lockImage = levelButtons[i].GetComponentInChildren<Image>();
                //lockImage.gameObject.SetActive(playerPref.currentMaxLevel >= i + 1);
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
        LoadingScript.loadLevel = level;
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);
        //SceneManager.LoadScene("Stage" + level, LoadSceneMode.Single);
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
    }

    /// <summary>
    /// 
    /// </summary>
    public void loadHFPanel() {
        foreach (Transform child in hfContentView.transform) {
            Destroy(child.gameObject);
        }
        hfContentView.DetachChildren();

        foreach (HF hf in hfs[HF.TYPE_HF.Kill]) {
            var newHf = Instantiate(hfPrefab) as Transform;
            var hfTexts = newHf.GetComponentsInChildren<Text>();
            hfTexts[0].text = hf.description;
            hfTexts[1].text = " " + ((playerPref.kills > hf.nb) ? hf.nb : playerPref.kills) + "/" + hf.nb;
            hfTexts[2].text = "+" + hf.gold;
            if (playerPref.kills < hf.nb)
                newHf.GetComponent<Image>().color = new Color32(100, 100, 100, 255);
            newHf.SetParent(hfContentView.transform, false);
        }
        foreach (HF hf in hfs[HF.TYPE_HF.Bonus]) {
            var newHf = Instantiate(hfPrefab) as Transform;
            var hfTexts = newHf.GetComponentsInChildren<Text>();
            hfTexts[0].text = hf.description;
            hfTexts[1].text = " " + ((playerPref.bonus > hf.nb) ? hf.nb : playerPref.bonus) + "/" + hf.nb;
            hfTexts[2].text = "+" + hf.gold;
            if (playerPref.bonus < hf.nb)
                newHf.GetComponent<Image>().color = new Color32(100, 100, 100, 255);
            newHf.SetParent(hfContentView.transform, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetGame() {
        playerPref = new PlayerPref();
        save();
        Awake();
    }

    /// <summary>
    /// 
    /// </summary>
    public void goldHack() {
        playerPref.gold += 1000;
        save();
        goldText.text = " " + playerPref.gold;
    }

    /// <summary>
    /// 
    /// </summary>
    public void unlockLevels() {
        playerPref.currentMaxLevel = 5;
        save();
        //Camera.main.backgroundColor = new Color(255, 0, 0, 128);
        //Invoke("cleanCamera", 0.5f);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            Application.Quit();
        }
    }

}
