using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningScript : MonoBehaviour {

    public bool autofire = true;
    public bool borderBounce = true;

    private bool hasSpawn;
    private MoveScript moveScript;
    private WeaponScript[] weapons;
    private Collider2D coliderComponent;
    private SpriteRenderer rendererComponent;

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
            // Auto-fire ?
            foreach (WeaponScript weapon in weapons)
            {
                if (weapon.enabled  && (autofire || weapon.forceAutoFire) && weapon.CanAttack)
                {
                    if (weapon.Attack(true))
                        SoundEffectsHelper.Instance.MakeEnemyShotSound();
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
            if (borderBounce) { 
                if (transform.position.x == leftBorder)
                {
                    moveScript.direction.x = Mathf.Abs(moveScript.direction.x);
                }
                else if (transform.position.x == rightBorder)
                {
                    moveScript.direction.x = -Mathf.Abs(moveScript.direction.x);
                }
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

        CollectableScript collectable = GetComponent<CollectableScript>();
        // -- Shooting
        if (collectable == null)
        {
            foreach (WeaponScript weapon in weapons)
            {
                weapon.enabled = true;
            }
        }
    }

    public bool HasSpawn()
    {
        return hasSpawn;
    }

}
