using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MonoBehaviour {

    public int rageModeHp = 20;
    public Vector2 rageSpeed = new Vector2(2, 1);

    private Animator animator;

    void Awake()
    {
        // Get the animator
        animator = GetComponent<Animator>();
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
                if (GetComponent<HealthScript>().hp < rageModeHp && GetComponentInChildren<MoveScript>().speed != rageSpeed)
                {
                    GetComponent<MoveScript>().speed = rageSpeed;
                    GetComponentInChildren<WeaponScript>().shootingRate *= 2;
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
            gameOver.ShowButtons(true);
        }
    }
}
