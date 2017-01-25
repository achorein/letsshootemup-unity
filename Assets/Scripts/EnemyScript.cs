using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour {

    private bool hasSpawn;
    private MoveScript moveScript;
    private WeaponScript[] weapons;
    private Collider2D coliderComponent;
    private SpriteRenderer rendererComponent;

    public float limitY = -1;
    public int points = 10;

    void Awake()
    {
        // Retrieve the weapon only once
        weapons = GetComponentsInChildren<WeaponScript>();

        // Retrieve scripts to disable when not spawn
        moveScript = GetComponent<MoveScript>();

        coliderComponent = GetComponent<Collider2D>();

        rendererComponent = GetComponent<SpriteRenderer>();
    }

    // 1 - Disable everything
    void Start()
    {
        hasSpawn = false;

        // Disable everything
        // -- collider
        coliderComponent.enabled = false;
        // -- Moving
        moveScript.enabled = false;
        // -- Shooting
        foreach (WeaponScript weapon in weapons)
        {
            weapon.enabled = false;
        }
    }

    void Update()
    {
        // 2 - Check if the enemy has spawned.
        if (hasSpawn == false)
        {
            if (rendererComponent.IsVisibleFrom(Camera.main))
            {
                Spawn();
            }
        }
        else
        {
            // Auto-fire
            foreach (WeaponScript weapon in weapons)
            {
                if (weapon != null && weapon.enabled && weapon.CanAttack)
                {
                    if (weapon.Attack(true))
                    {
                        SoundEffectsHelper.Instance.MakeEnemyShotSound();
                    }
                }
            }

            // 4 - Out of the camera ? Destroy the game object.
            if (rendererComponent.IsVisibleFrom(Camera.main) == false)
            {
                Destroy(gameObject);
            }


            // 6 - Make sure we are not outside the camera bounds on right and left side
            var dist = (transform.position - Camera.main.transform.position).z;

            var leftBorder = Camera.main.ViewportToWorldPoint(
              new Vector3(0, 0, dist)
            ).x;

            var rightBorder = Camera.main.ViewportToWorldPoint(
              new Vector3(1, 0, dist)
            ).x;

            var newPosition = new Vector3(
              Mathf.Clamp(transform.position.x, leftBorder, rightBorder),
              transform.position.y,
              transform.position.z
            );

            if (limitY > 0)
            {
                var topBorder = Camera.main.ViewportToWorldPoint(
                  new Vector3(0, 1, dist)
                ).y;

                var bottomBorder = Camera.main.ViewportToWorldPoint(
                  new Vector3(0, limitY, dist)
                ).y;

                newPosition.y = Mathf.Clamp(transform.position.y, bottomBorder, topBorder);

                if (transform.position.y == bottomBorder || transform.position.y == topBorder)
                {
                    moveScript.direction *= -1;
                }
            }

            if (transform.position.x == leftBorder || transform.position.x == rightBorder)
            {
                moveScript.direction *= -1;
            }

            transform.position = newPosition;
        }

    }

    // 3 - Activate itself.
    private void Spawn()
    {
        hasSpawn = true;

        // Enable everything
        // -- Collider
        coliderComponent.enabled = true;
        // -- Moving
        moveScript.enabled = true;
        // -- Shooting
        foreach (WeaponScript weapon in weapons)
        {
            weapon.enabled = true;
        }
    }

    void OnDestroy()
    {
        GameHelper.Instance.UpdateScore(points);
    }

}
