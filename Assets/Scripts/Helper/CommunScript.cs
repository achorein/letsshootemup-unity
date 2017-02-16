using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using GoogleMobileAds.Api;

public class CommunScript : MonoBehaviour {

    private const string PLAYER_KEY = "PLAYER";
    public static string FIREBASE_URL = "https://lets-shootem-up-23835757.firebaseio.com";
    public static string LEADERBOARD_ID = "CgkI2Jem2tQJEAIQBg";

    //ads 
    protected InterstitialAd interstitial;
    protected BannerView bannerView;

    public List<Ship> ships;
    public Dictionary<HF.TYPE_HF, List<HF>> hfs;

    public PlayerPref playerPref;

    public CommunScript() {
        ships = new List<Ship>();
        ships.Add(new Ship("playerShip1", 0, 1, 1, 1));
        ships.Add(new Ship("playerShip2", 100, 2, 1, 1));
        ships.Add(new Ship("playerShip3", 500, 3, 2, 1));
        ships.Add(new Ship("playerShip4", 1500, 3, 2, 2));
        ships.Add(new Ship("playerShip5", 5000, 1, 3, 3));
        hfs = new Dictionary<HF.TYPE_HF, List<HF>>();
        List<HF> killHfs = new List<HF>();
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "5 kills", 5, 1, "CgkI2Jem2tQJEAIQBw"));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "20 kills", 20, 2, "CgkI2Jem2tQJEAIQCA"));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "100 kills", 100, 5, "CgkI2Jem2tQJEAIQCQ"));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "500 kills", 500, 50, "CgkI2Jem2tQJEAIQCg"));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "1000 kills", 1000, 100, "CgkI2Jem2tQJEAIQCw"));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "2500 kills", 2500, 250, "CgkI2Jem2tQJEAIQDA"));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "5000 kills", 5000, 500, "CgkI2Jem2tQJEAIQDQ"));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "10000 kills", 10000, 1000, "CgkI2Jem2tQJEAIQDg"));
        hfs.Add(HF.TYPE_HF.Kill, killHfs);
        List<HF> bonusHfs = new List<HF>();
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "5 bonus", 5, 2, "CgkI2Jem2tQJEAIQDw"));
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "20 bonus", 20, 10, "CgkI2Jem2tQJEAIQEA"));
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "100 bonus", 100, 50, "CgkI2Jem2tQJEAIQEQ"));
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "500 bonus", 500, 250, "CgkI2Jem2tQJEAIQEg"));
        hfs.Add(HF.TYPE_HF.Bonus, bonusHfs);
        List<HF> levelHfs = new List<HF>();
        levelHfs.Add(new HF(HF.TYPE_HF.Level, "Level completed", 1, 5, "CgkI2Jem2tQJEAIQEw"));
        levelHfs.Add(new HF(HF.TYPE_HF.Level, "3 levels in one shot", 3, 50, true, "CgkI2Jem2tQJEAIQFg"));
        levelHfs.Add(new HF(HF.TYPE_HF.Level, "Game finished", 5, 50, "CgkI2Jem2tQJEAIQFQ"));
        levelHfs.Add(new HF(HF.TYPE_HF.Level, "To infinity and beyond", 5, 25, "CgkI2Jem2tQJEAIQFw"));
        hfs.Add(HF.TYPE_HF.Level, levelHfs);
        List<HF> weaponHfs = new List<HF>();
        weaponHfs.Add(new HF(HF.TYPE_HF.Weapon, "Weapon upgraded", 1, 10, "CgkI2Jem2tQJEAIQGA"));
        hfs.Add(HF.TYPE_HF.Weapon, weaponHfs);
        List<HF> otherHfs = new List<HF>();
        otherHfs.Add(new HF(HF.TYPE_HF.Other, "Untouchable", 1, 10, "CgkI2Jem2tQJEAIQFA"));
        hfs.Add(HF.TYPE_HF.Other, otherHfs);
    }

    /// <summary>
    /// Load game data (Player Pref)
    /// </summary>
    public void load() {
        string data = PlayerPrefs.GetString(PLAYER_KEY);
        if (data == null || data.Length == 0) {
            playerPref = new PlayerPref();
        } else {
            BinaryFormatter bf = new BinaryFormatter();
            byte[] b = Convert.FromBase64String(data);
            MemoryStream ms = new MemoryStream(b);

            try {
                playerPref = (PlayerPref)bf.Deserialize(ms);
            } finally {
                ms.Close();
            }
        }
    }

    /// <summary>
    /// Persist data (Player Pref)
    /// </summary>
    public void save() {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream memStr = new MemoryStream();

        try {
            bf.Serialize(memStr, playerPref);
            memStr.Position = 0;

            PlayerPrefs.SetString(PLAYER_KEY, Convert.ToBase64String(memStr.ToArray()));
        } finally {
            memStr.Close();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="score"></param>
    public void saveScore(int score, int bonusGold) {
        // Save score on google game
        if (Social.localUser.authenticated) {
            Social.ReportScore(score, LEADERBOARD_ID, (bool saveSuccess) => { });
        }
        // internal storage
        if (score > playerPref.bestScore) {
            playerPref.bestScore = score;
        }
        playerPref.gold += bonusGold;
        save();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>sprite name</returns>
    public Ship getCurrentShip() {
        return ships[playerPref.currentShip];
    }

    /// <summary>
    /// Load Ads
    /// </summary>
    /// <param name="customListener"></param>
    public void LoadBannerAd(bool customListener) {
        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(getAdUnitBannerId(), AdSize.Banner, AdPosition.Bottom);

        if (!customListener) {
            bannerView.OnAdLoaded += HandleOnAdLoaded;
            bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        }

        bannerView.OnAdClosed += HandleOnAdClosed;
        bannerView.OnAdLeavingApplication += HandleOnAdClosed;

        // Load the banner with the request.
        bannerView.LoadAd(getAdsRequest());
    }

    /// <summary>
    /// Get ads request
    /// </summary>
    /// <returns></returns>
    public AdRequest getAdsRequest() {
        // Create an empty ad request.
        return new AdRequest.Builder()
           .AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
           .AddTestDevice("B089878A2C63370113F780BFAB28BD9E")  // My test device.
           .AddKeyword("game")
           .Build();
    }

    /// <summary>
    /// Get Ads banner id
    /// </summary>
    /// <returns></returns>
    protected string getAdUnitBannerId() {
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
                string adUnitId = "ca-app-pub-5490026576622469/3135352200";
#elif UNITY_IPHONE
                string adUnitId = "ca-app-pub-5490026576622469/3135352200";
#else
                string adUnitId = "unexpected_platform";
#endif
        return adUnitId;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected string getAdUnitInterstitielId() {
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
                string adUnitId = "ca-app-pub-5490026576622469/4612085401";
#elif UNITY_IPHONE
                string adUnitId = "ca-app-pub-5490026576622469/4612085401";
#else
                string adUnitId = "unexpected_platform";
#endif
        return adUnitId;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleOnAdLoaded(object sender, EventArgs args) {
        if (interstitial != null)
            interstitial.Show();
        if (bannerView != null)
            bannerView.Show();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleOnAdClosed(object sender, EventArgs args) {
        resetAd();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleOnAdFailedToLoad(object sender, EventArgs args) {
        resetAd();
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadInterstitialAd() {
        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(getAdUnitInterstitielId());
        // Load the interstitial with the request.
        interstitial.OnAdLoaded += HandleOnAdLoaded;
        interstitial.OnAdFailedToLoad += HandleOnAdClosed;
        interstitial.OnAdClosed += HandleOnAdClosed;
        interstitial.OnAdLeavingApplication += HandleOnAdClosed;
        interstitial.LoadAd(getAdsRequest());
    }

    /// <summary>
    /// Hide and destroy all ads
    /// </summary>
    protected void resetAd() {
        if (interstitial != null)
            interstitial.Destroy();
        if (bannerView != null) {
            bannerView.Hide();
            bannerView.Destroy();
        }
    }

}