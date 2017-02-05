using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour {

    public int points = 10;

    [Header("IA behavior")]
    public float limitY = -1;
    public float rotateSpeed = 0;

    [Header("Boss pattern")]
    public bool moveThenShoot = false;
    // Boss pattern (not really an AI)
    public float minAttackCooldown = 0.5f;
    public float maxAttackCooldown = 2f;
    public float attackDuration = 0;

    // Internal
    private float aiCooldown;
    private bool isAttacking;

    private bool spawn;

    private MoveScript moveScript;
    private SpriteRenderer rendererComponent;
    private WeaponScript[] weapons;

    void Awake()
    {
        // Retrieve the weapon only once
        weapons = GetComponentsInChildren<WeaponScript>();

        // Retrieve scripts to disable when not spawn
        moveScript = GetComponent<MoveScript>();

        rendererComponent = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Default behavior
        isAttacking = false;
        aiCooldown = maxAttackCooldown;
        spawn = false;
    }

    void Update()
    {
        if (rendererComponent.IsVisibleFrom(Camera.main))
        {
            if (!spawn)
            {
                foreach (WeaponScript weapon in weapons)
                {
                    print("enable weapon ");
                    weapon.enabled = true;
                }
                spawn = true;
            }

            if (moveThenShoot)
            {
                // AI
                //------------------------------------
                // Move or attack. permute. Repeat.
                aiCooldown -= Time.deltaTime;
                if (aiCooldown <= 0f)
                {
                    isAttacking = !isAttacking;
                    if (isAttacking && attackDuration > 0)
                    {
                        aiCooldown = attackDuration;
                    }
                    else
                    {
                        aiCooldown = Random.Range(minAttackCooldown, maxAttackCooldown);
                    }
                    moveScript.fixedPosition = Vector2.zero;
                }

                // Attack
                //----------
                if (isAttacking)
                {
                    // Stop any movement
                    moveScript.direction = Vector2.zero;

                    foreach (WeaponScript weapon in weapons)
                    {
                        if (weapon.enabled && weapon.CanAttack)
                        {
                            weapon.Attack(true);
                            SoundEffectsHelper.Instance.MakeEnemyShotSound();
                        }
                    }
                }
                // Move
                //----------
                else
                {
                    // Define a target?
                    if (moveScript.fixedPosition == Vector2.zero)
                    {
                        // Get a point on the screen
                        moveScript.fixedPosition = new Vector2(Random.Range(0.0f, 1f), Random.Range(0.2f, 1f));
                    }
                }
            }

            // 6 - Make sure we are not outside the camera bounds on right and left side
            var dist = (transform.position - Camera.main.transform.position).z;

            var newPosition = new Vector3(
              transform.position.x,
              transform.position.y,
              transform.position.z
            );

            if (limitY > 0)
            {
                var topBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist)).y;
                var bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, limitY, dist)).y;

                newPosition.y = Mathf.Clamp(transform.position.y, bottomBorder, topBorder);

                if (transform.position.y <= bottomBorder)
                {
                    moveScript.direction.y = Mathf.Abs(moveScript.direction.y);
                }
                else if(transform.position.y >= topBorder)
                {
                    moveScript.direction.y = -Mathf.Abs(moveScript.direction.y);
                }
            }

            transform.position = newPosition;

            if (rotateSpeed != 0)
            {
                //Rotate thet transform of the game object this is attached to by 45 degrees, taking into account the time elapsed since last frame.
                transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime * rotateSpeed);
            }
        }
    }

    void OnDestroy()
    {
        if (rendererComponent.IsVisibleFrom(Camera.main))
        {
            GameHelper.Instance.UpdateScore(points);
        }
    }

}
