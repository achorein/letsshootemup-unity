using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossScript : MonoBehaviour {

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
