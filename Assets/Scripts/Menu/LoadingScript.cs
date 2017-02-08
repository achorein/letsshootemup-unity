using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScript : CommunScript {

    public static int loadLevel = 1;

    public Text progressText;

    private AsyncOperation async = null; // When assigned, load is in progress.
    private bool loadScene = false;

    void Start()
    {
        LoadBannerAd(true);
        bannerView.OnAdLoaded += LoadingOnAdLoaded;
        bannerView.OnAdFailedToLoad += LoadingOnAdFailedToLoad;
        Invoke("startLoadingScene", 3);
    }

	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
        startLoadingScene();
#endif
        // If the new scene has started loading...
        if (loadScene)
        {
            int percentageLoaded = Mathf.CeilToInt(async.progress * 100);
            progressText.text = percentageLoaded.ToString() + " %";
            if (percentageLoaded >= 90)
            {
                resetAd();
                async.allowSceneActivation = true;
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (async != null)
                
            if (bannerView != null)
                bannerView.Destroy();
            // Reload the level
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
    }

    // The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
    IEnumerator LoadNewScene()
    {
        if (async == null)
        {
            // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
            async = SceneManager.LoadSceneAsync("Stage" + loadLevel);
            async.allowSceneActivation = false;
        }

        // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
        while (!async.isDone)
        {
            yield return null;
        }

    }

    void startLoadingScene()
    {
        if (!loadScene)
        {
            // ...set the loadScene boolean to true to prevent loading a new scene more than once...
            loadScene = true;
            // ...and start a coroutine that will load the desired scene.
            StartCoroutine(LoadNewScene());
        }
    }

    void LoadingOnAdLoaded(object sender, EventArgs args)
    {
        bannerView.Show();
        startLoadingScene();
    }

    void LoadingOnAdFailedToLoad(object sender, EventArgs args)
    {
        resetAd();
        startLoadingScene();
    }

}
