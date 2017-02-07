using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class GameOverScript : CommunScript
{

    private Button[] buttons;
    private Text[] texts;
    private Image[] images;

    public GameObject gamePanel, player, achivementImage;
    
    public Text winText, loseText;
    public Text scoreText, trophyText, goldText;
    public Button restartButton, nextButton;

    public int currentLevel = 1;
    private int maxLevel = 5;

    void Awake()
    {
        // Get the buttons
        buttons = GetComponentsInChildren<Button>();
        texts = GetComponentsInChildren<Text>();
        images = GetComponentsInChildren<Image>(false);

        // Disable them
        HideButtons();
    }

    public void HideButtons()
    {
        Time.timeScale = 1;
        resetPanel(false);
    }

    public void ShowButtons(bool win)
    {
        // game pause
        Time.timeScale = 0;
        if (player == null || winText == null)
            return;
        LoadInterstitialAd();
        resetPanel(true);
        
        // main text
        loseText.gameObject.SetActive(!win);
        winText.gameObject.SetActive(win);
        
        // update UI
        int score = GameHelper.Instance.getScore();
        scoreText.text = score.ToString();
        trophyText.text = GameHelper.Instance.playerPref.bestScore.ToString();
 
        int bonusGold = (score / 100);
        goldText.text = " +" + bonusGold;
        if (player.GetComponent<PlayerScript>().nbHitTaken == 0)
        {
            achivementImage.SetActive(true);
            bonusGold += 10;
        }

        if (win)
        {
            if (currentLevel + 1 > GameHelper.Instance.playerPref.currentMaxLevel)
            {
                GameHelper.Instance.playerPref.currentMaxLevel = currentLevel + 1;
            }
            nextButton.gameObject.SetActive(currentLevel < maxLevel);
        }
        restartButton.gameObject.SetActive(!win);

        // update playerpref
        if (score > GameHelper.Instance.playerPref.bestScore)
        {
            GameHelper.Instance.playerPref.bestScore = score;
        }
        GameHelper.Instance.playerPref.gold += bonusGold;
        GameHelper.Instance.save();
    }

    public void ExitToMenu()
    {
        resetAd();
        resetPlayer();
        // Reload the level
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void NextGame()
    {
        resetAd();
        LoadingScript.loadLevel = currentLevel + 1;
        // Reload the level
        SceneManager.LoadScene("Loading", LoadSceneMode.Single);
    }

    public void RestartGame()
    {
        resetAd();
        resetPlayer();
        // Reload the level
        SceneManager.LoadScene("Stage" + currentLevel, LoadSceneMode.Single);
    }

    private void resetPanel(bool active)
    {
        foreach (var b in buttons)
        {
            b.gameObject.SetActive(active);
        }
        foreach (var t in texts)
        {
            t.gameObject.SetActive(active);
        }
        foreach (var i in images)
        {
            if (i.gameObject.GetInstanceID() != GetInstanceID())
            {
                i.enabled = active;
            }
        }
        gamePanel.SetActive(!active);
        GetComponent<Image>().enabled = active;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ExitToMenu();
        }
    }

    public void resetPlayer()
    {
        PlayerScript.lastShieldLevel = 0;
        PlayerScript.lastLife = 0;
        PlayerScript.lastWeaponBonus = null;
    }

}
