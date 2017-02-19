using UnityEngine;
using UnityEngine.UI;

public class PauseScript : CommunScript {

    private Button[] buttons;
    private Text[] texts;
    private Image[] images;

    void Awake() {
        // Get the buttons
        buttons = GetComponentsInChildren<Button>();
        texts = GetComponentsInChildren<Text>();
        images = GetComponentsInChildren<Image>(false);
        resetPanel(false);
        load();
    }

    public void PauseGame() {
        resetPanel(true);
        if (showAdTimeout() >= 0) {
            LoadBannerAd(false);
        }
        Time.timeScale = 0;
    }

    public void ResumeGame() {
        resetAd();
        resetPanel(false);
        Time.timeScale = 1;
    }

    internal void resetPanel(bool active) {
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
    }
}
