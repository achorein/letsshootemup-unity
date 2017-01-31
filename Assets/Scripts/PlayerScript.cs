using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    /// <summary>
    /// The speed of the ship
    /// </summary>
    public Vector2 speed = new Vector2(50, 50);
    public float mouseSpeed = 0.1f;
    public int nbLife = 2;
    public GameObject lifePanel;
    public GameObject shieldUi;

    // Store the movement and the component
    private Vector2 movement;
    private Rigidbody2D rigidbodyComponent;
    private Animator animator;

    // internal
    private int shieldLevel = 0;
    private bool isInvincible = false;

    void Awake()
    {
        // Get the animator
        animator = GetComponent<Animator>();

        // Update lifes UI
        Image[] lifesUI = lifePanel.gameObject.GetComponentsInChildren<Image>();
        for (int i = 1; i <= lifesUI.Length - nbLife; i++)
        {
            lifesUI[lifesUI.Length - i].enabled = false;
        }
    }
	
	// Update is called once per frame
	void Update () {
        // Retrieve axis information
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        // Handle Mouse
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            Vector3 cursorPosition = Input.mousePosition;
            cursorPosition = Camera.main.ScreenToWorldPoint(cursorPosition);
            transform.position = Vector2.Lerp(transform.position, cursorPosition, mouseSpeed);
        }
        // Handle touch screen : Look for all fingers
        for (int i = 0; i < Input.touchCount; i++)
        {
            Vector3 cursorPosition = Input.GetTouch(i).position;
            // Touch are screens location. Convert to world
            Vector3 position = Camera.main.ScreenToWorldPoint(cursorPosition);
            transform.position = Vector2.Lerp(transform.position, position, mouseSpeed);
        }

        // Movement per direction
        movement = new Vector2(
          speed.x * inputX,
          speed.y * inputY);

        // Shooting
        bool shoot = Input.GetButton("Fire1");
        shoot |= Input.GetButton("Fire2");
        // Careful: For Mac users, ctrl + arrow is a bad idea

        if (shoot)
        {
            WeaponScript[] weapons = GetComponentsInChildren<WeaponScript>();
            foreach (WeaponScript weapon in weapons)
            {
                // false because the player is not an enemy
                if (weapon.enabled && weapon.Attack(false))
                {
                    SoundEffectsHelper.Instance.MakePlayerShotSound();
                }
            }
        }

        // Make sure we are not outside the camera bounds
        var dist = (transform.position - Camera.main.transform.position).z;
        var leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).x;
        var rightBorder = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist)).x;
        var topBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).y;
        var bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist)).y;

        transform.position = new Vector3(
          Mathf.Clamp(transform.position.x, leftBorder, rightBorder),
          Mathf.Clamp(transform.position.y, topBorder, bottomBorder),
          transform.position.z
        );

        // Handle escape and return button
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
    }


    void FixedUpdate()
    {
        // Get the component and store the reference
        if (rigidbodyComponent == null) rigidbodyComponent = GetComponent<Rigidbody2D>();

        // Move the game object
        rigidbodyComponent.velocity = movement;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        int damagePlayer = 0;
        // Ignore collision when player is invincible
        Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>(), isInvincible);
        if (!isInvincible)
        {
            // Collision with enemy
            EnemyScript enemy = collision.gameObject.GetComponent<EnemyScript>();
            if (enemy != null && collision.gameObject.GetComponent<BossScript>() == null)
            {
                // Kill the enemy
                HealthScript enemyHealth = enemy.GetComponent<HealthScript>();
                if (enemyHealth != null)
                {
                    damagePlayer = enemyHealth.hp / 3;
                    if (shieldLevel + 1 >= damagePlayer)
                    {
                        enemyHealth.Damage(enemyHealth.hp); // kill enemy
                    }
                }
                else
                {
                    damagePlayer = 1;
                }
            }
        }

        // Is this a bonus?
        CollectableScript collectable = collision.gameObject.GetComponentInChildren<CollectableScript>();
        if (collectable != null)
        {
            // Is this a shield bonus?
            ShieldScript shield = collision.gameObject.GetComponent<ShieldScript>();
            if (shield != null)
            {
                shieldLevel = shield.shieldLevel;
                updateShieldUi();
                updateLifeUi(false);
                SoundEffectsHelper.Instance.MakeShieldSound(true);
                Destroy(shield.gameObject); // Remember to always target the game object, otherwise you will just remove the script
            }

            WeaponScript[] bonusWeapons = collision.gameObject.GetComponentsInChildren<WeaponScript>();
            // Is this a weapon bonus?
            if (bonusWeapons != null && bonusWeapons.Length > 0)
            {
                WeaponScript[] weapons = GetComponentsInChildren<WeaponScript>();
                foreach (WeaponScript weapon in weapons)
                {
                    weapon.enabled = false;
                }
                for (int i = 0; i < bonusWeapons.Length; i++)
                {
                    //weapons[i].transform.rotation = bonusWeapons[i].transform.rotation;
                    weapons[i].shotPrefab = bonusWeapons[i].shotPrefab;
                    weapons[i].shootingRate = bonusWeapons[i].shootingRate;
                    weapons[i].enabled = true;
                }
            }
            Destroy(collision.gameObject); // Remember to always target the game object, otherwise you will just remove the script
        }

        // Damage the player if necessery
        if (this.takeDamage(damagePlayer))
        {
            GetComponent<HealthScript>().Damage(1);
        }
        
    }

    public bool takeDamage(int damage)
    {
        if (!isInvincible && damage > 0)
        {
            int realDamage = damage - shieldLevel;
            if (shieldLevel > 0)
            {
                shieldLevel -= damage;
                updateShieldUi();
            }
            if (realDamage > 0) {
                nbLife--;
                if (nbLife > 0)
                {
                    // Update Ui
                    animator.SetBool("loseLife", true);
                    updateLifeUi(true);
                    // Invincible
                    isInvincible = true;
                    Invoke("disableInvincible", 2f); // 2 sec
                    // Play sound
                    SoundEffectsHelper.Instance.MakeLoseSound();
                }
                else
                {
                    // kill player
                    return true;
                }
            }
        }
        return false;
    }

    private void updateShieldUi()
    {
        if (shieldLevel < 0) shieldLevel = 0;
        animator.SetInteger("shieldLevel", shieldLevel<=3 ? shieldLevel : 3);
        shieldUi.SetActive(shieldLevel > 0);
        if (shieldUi.activeSelf)
        {
            Image[] shields = shieldUi.GetComponentsInChildren<Image>();
            for (int i = 1; i < shields.Length; i++)
            {
                if (shieldLevel == i)
                {
                    shields[i].enabled = true;
                } else {
                    shields[i].enabled = false;
                }
            }
        }
    }

    private void updateLifeUi(bool loseLife)
    {
        // Update life UI
        Image[] lifesUI = lifePanel.gameObject.GetComponentsInChildren<Image>();
        for (int i = 1; i <= lifesUI.Length - nbLife; i++)
        {
            Image lifeUI = lifesUI[lifesUI.Length - i];
            Animator lifeAnimator = lifeUI.GetComponent<Animator>();
            if (loseLife)
            {
                lifeAnimator.SetBool("loseLife", true);
            }
            else if (lifeAnimator.GetBool("loseLife"))
            {
                lifeAnimator.SetBool("loseLife", false);
                lifeUI.enabled = false;
            }

        }
        
    }

    void disableInvincible()
    {
        animator.SetBool("loseLife", false);
        updateShieldUi();
        updateLifeUi(false);
        isInvincible = false;
        SoundEffectsHelper.Instance.MakeShieldSound(false);
    }

    void OnDestroy()
    {
        // Game Over.
        var gameOver = FindObjectOfType<GameOverScript>();
        if (gameOver != null)
        {
            gameOver.ShowButtons(false);
        }
    }
}
