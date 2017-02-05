using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MonoBehaviour {
    
    public int rageModeHp = 20;
    public Vector2 rageSpeed = new Vector2(2, 1);
    public float rageShootingRate = 0;

    private MoveScript moveScript;
    private WeaponScript[] weapons;
    private SpriteRenderer rendererComponent;
    private Animator animator;

    private bool spawn;

    void Awake()
    {
        // Retrieve the weapon only once
        weapons = GetComponentsInChildren<WeaponScript>();

        // Get the animator
        animator = GetComponent<Animator>();

        // Retrieve scripts
        moveScript = GetComponent<MoveScript>();

        rendererComponent = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        spawn = false;
    }

    void Update()
    {
        if (rendererComponent.IsVisibleFrom(Camera.main))
        {
            if (!spawn)
            {
                SoundEffectsHelper.Instance.MakeBossComingSound();
                // Stop the main scrolling
                foreach (ScrollingScript scrolling in FindObjectsOfType<ScrollingScript>())
                {
                    if (scrolling.isLinkedToCamera)
                    {
                        scrolling.speed = Vector2.zero;
                    }
                }
                spawn = true;
            }
            
        }
    }

    void OnTriggerEnter2D(Collider2D otherCollider2D)
    {
        // Taking damage? Change animation
        ShotScript shot = otherCollider2D.gameObject.GetComponent<ShotScript>();
        if (shot != null)
        {
            if (shot.isEnemyShot == false)
            {
                // Change animation
                animator.SetTrigger("Hit");
                // Active Rage Mode ?
                if (GetComponent<HealthScript>().hp <= rageModeHp && GetComponentInChildren<MoveScript>().speed != rageSpeed)
                {
                    // Change animation
                    animator.SetBool("Rage", true);
                    GetComponent<MoveScript>().speed = rageSpeed;
                    foreach(WeaponScript weapon in weapons)
                    {
                        if (weapon.gameObject.activeSelf && rageShootingRate > 0)
                        {
                            weapon.shootingRate = rageShootingRate;
                        } else
                        {
                            weapon.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        // Game Over.
        var gameOver = FindObjectOfType<GameOverScript>();
        if (gameOver != null)
        {
            SoundEffectsHelper.Instance.MakeVictorySound();
            gameOver.ShowButtons(true);
        }
    }
}
