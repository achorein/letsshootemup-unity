using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using GoogleMobileAds.Api;

public class CommunScript : MonoBehaviour {

    private const string PLAYER_KEY = "PLAYER";
    public static string FIREBASE_URL = "https://lets-shootem-up-23835757.firebaseio.com";

    //ads 
    protected InterstitialAd interstitial;
    protected BannerView bannerView;

    public class Ship {
        public string sprite;
        public int price;

        public Ship(string sprite, int price) {
            this.sprite = sprite;
            this.price = price;
        }
    }

    public List<Ship> ships;
    public Dictionary<HF.TYPE_HF, List<HF>> hfs;

    public PlayerPref playerPref;

    public CommunScript() {
        ships = new List<Ship>();
        ships.Add(new Ship("playerShip1", 0));
        ships.Add(new Ship("playerShip2", 100));
        ships.Add(new Ship("playerShip3", 500));
        ships.Add(new Ship("playerShip4", 1500));
        ships.Add(new Ship("playerShip5", 5000));
        hfs = new Dictionary<HF.TYPE_HF, List<HF>>();
        List<HF> killHfs = new List<HF>();
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "5 kills", 5, 1));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "20 kills", 20, 2));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "100 kills", 100, 5));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "500 kills", 500, 50));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "1000 kills", 1000, 100));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "2500 kills", 2500, 250));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "5000 kills", 5000, 500));
        killHfs.Add(new HF(HF.TYPE_HF.Kill, "10000 kills", 10000, 1000));
        hfs.Add(HF.TYPE_HF.Kill, killHfs);
        List<HF> bonusHfs = new List<HF>();
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "5 bonus", 5, 2));
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "20 bonus", 20, 10));
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "100 bonus", 100, 50));
        bonusHfs.Add(new HF(HF.TYPE_HF.Bonus, "500 bonus", 500, 250));
        hfs.Add(HF.TYPE_HF.Bonus, bonusHfs);
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
    public void saveScore(int score) {
        if (score > playerPref.bestScore) {
            playerPref.bestScore = score;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>sprite name</returns>
    public string getCurrentShipSprite() {
        return ships[playerPref.currentShip].sprite;
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