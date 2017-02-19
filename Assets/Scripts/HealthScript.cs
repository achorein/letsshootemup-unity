using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour {

    /// <summary>
    /// Total hitpoints
    /// </summary>
    public int hp = 1;

    /// <summary>
    /// Enemy or player?
    /// </summary>
    public bool isEnemy = true;

    /// <summary>
    /// Inflicts damage and check if the object should be destroyed
    /// </summary>
    /// <param name="damageCount"></param>
    public void Damage(int damageCount) {
        hp -= damageCount;

        if (hp <= 0) {
            // 'Splosion!
            SpecialEffectsHelper.Instance.Explosion(transform.position);
            SoundEffectsHelper.Instance.MakeExplosionSound();

            // Dead!
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D otherCollider) {
        hitBy(otherCollider);
    }

    internal void hitBy(Collider2D otherCollider) {
        // Is this a shot?
        ShotScript shot = otherCollider.gameObject.GetComponent<ShotScript>();
        if (shot != null) {
            // Avoid friendly fire
            if (shot.isEnemyShot != isEnemy) {
                PlayerScript playerScript = GetComponent<PlayerScript>();
                if (playerScript == null) {
                    // enemy take a shot
                    int damageMultiplicator = (GameHelper.Instance.getCurrentShip().damage > 1) ? 2 : 1;
                    if (shot.damage * damageMultiplicator >= hp) {
                        GameHelper.Instance.enemeyKill(GetComponent<EnemyScript>().points);
                    } else {
                        GetComponent<EnemyScript>().runHitAnimation();
                    }
                    Damage(shot.damage * damageMultiplicator);
                } else if (playerScript.takeDamage(shot.damage)) {
                    // player take a shot (with no shield)
                    Damage(shot.damage);
                }

                // Destroy the shot
                Destroy(shot.gameObject); // Remember to always target the game object, otherwise you will just remove the script
            }
        }
    }
}
