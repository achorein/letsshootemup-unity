using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectsHelper : MonoBehaviour {

    /// <summary>
    /// Singleton
    /// </summary>
    public static SoundEffectsHelper Instance;

    public AudioClip explosionSound;
    public AudioClip playerShotSound;
    public AudioClip enemyShotSound;

    public AudioClip shieldUpSound;
    public AudioClip shieldDownSound;
    public AudioClip pickupSound;

    public AudioClip bossComing;
    public AudioClip bossTheme;

    public AudioClip loseSound;
    public AudioClip victorySound;
    public AudioClip gameOverSound;
    public AudioClip powerUpSound;

    public AudioSource mainMusic;

    void Awake()
    {
        // Register the singleton
        if (Instance != null)
        {
            Debug.LogError("Multiple instances of SoundEffectsHelper!");
        }
        Instance = this;
    }

    public void MakeExplosionSound()
    {
        MakeSound(explosionSound);
    }

    public void MakePlayerShotSound()
    {
        MakeSound(playerShotSound);
    }

    public void MakeEnemyShotSound()
    {
        MakeSound(enemyShotSound);
    }

    public void MakeShieldSound(bool up)
    {
        if (up)
        {
            MakeSound(shieldUpSound);
        }
        else
        {
            MakeSound(shieldDownSound);
        }
    }

    public void MakePickupSound()
    {
        MakeSound(pickupSound);
    }

    public void MakeLoseLifeSound()
    {
        MakeSound(loseSound);
    }

    public void MakePowerUpSound() {
        MakeSound(powerUpSound);
    }

    // MAIN SOUND

    public void MakeBossComingSound()
    {
        stopAllAudio();
        changeMainAudio(bossTheme, 0.1f);
        MakeSound(bossComing);
    }

    public void MakeVictorySound()
    {
        stopAllAudio();
        changeMainAudio(victorySound, 1);
    }

    public void MakeGameOverSound()
    {
        stopAllAudio();
        changeMainAudio(gameOverSound, 1);
    }

    private void stopAllAudio()
    {
        foreach(AudioSource audio in FindObjectsOfType<AudioSource>())
        {
            audio.Stop();
        }
    }

    private void changeMainAudio(AudioClip newMusic, float volume)
    {
        if (mainMusic)
        {
            mainMusic.volume = volume;
            mainMusic.clip = newMusic;
            mainMusic.Play();
        }
    }

    /// <summary>
    /// Play a given sound
    /// </summary>
    /// <param name="originalClip"></param>
    private void MakeSound(AudioClip originalClip)
    {
        MakeSound(originalClip, 1f);
    }

    /// <summary>
    /// Play a given sound
    /// </summary>
    /// <param name="originalClip"></param>
    private void MakeSound(AudioClip originalClip, float volume)
    {
        // As it is not 3D audio clip, position doesn't matter.
        AudioSource.PlayClipAtPoint(originalClip, transform.position, volume);
    }

}
