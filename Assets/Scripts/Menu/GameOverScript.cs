using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour {

    private Button[] buttons;
    private Text[] texts;
    private Image[] images;

    public GameObject gamePanel, player, achivementImage;
    
    public Text winText, loseText;
    public Text scoreText, trophyText, goldText;
    public Button restartButton, nextButton;

    public int currentLevel = 1;

    private int maxLevel = 2;

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
        foreach (var b in buttons)
        {
            b.gameObject.SetActive(false);
        }
        foreach (var t in texts)
        {
            t.gameObject.SetActive(false);
        }
        foreach (var i in images)
        {
            if (i.gameObject.GetInstanceID() != GetInstanceID())
            {
                //i.gameObject.SetActive(false);
                i.enabled = false;
            }
        }
        gamePanel.SetActive(true);
        GetComponent<Image>().enabled = false;
    }

    public void ShowButtons(bool win)
    {
        // game pause
        Time.timeScale = 0;
        if (player == null || winText == null)
        {
            return;
        }

        // enable panel
        gamePanel.SetActive(false);
        GetComponent<Image>().enabled = true;

        // update UI
        int score = GameHelper.Instance.getScore();
        scoreText.text = score.ToString();
        GameHelper.Instance.SaveScore();

        int bonusGold = (score / 100);
        goldText.text = " +" + bonusGold;
        if (player.GetComponent<PlayerScript>().nbHitTaken == 0)
        {
            achivementImage.SetActive(true);
            bonusGold += 10;
        }
        GameHelper.Instance.UpdateGold(bonusGold);

        if (win && currentLevel < maxLevel)
        {
            GameHelper.Instance.playerPref.currentMaxLevel = currentLevel + 1;
            GameHelper.Instance.save();
            nextButton.gameObject.SetActive(true);
        }
        restartButton.gameObject.SetActive(!win);

        trophyText.text = GameHelper.Instance.playerPref.bestScore.ToString();

        foreach (var b in buttons)
        {
            b.gameObject.SetActive(true);
        }
        foreach (var t in texts)
        {
            t.gameObject.SetActive(true);
        }
        foreach (var i in images)
        {
            i.enabled = true;
        }
        if (win) {
            loseText.gameObject.SetActive(false);
        } else {
            winText.gameObject.SetActive(false);
        }
    }

    public void ExitToMenu()
    {
        // Reload the level
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void NextGame()
    {
        // Reload the level
        SceneManager.LoadScene("Stage" + (currentLevel + 1), LoadSceneMode.Single);
    }

    public void RestartGame()
    {
        // Reload the level
        SceneManager.LoadScene("Stage" + currentLevel, LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ExitToMenu();
        }
    }

}
