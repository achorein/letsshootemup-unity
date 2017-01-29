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
    public int nbLife = 2;
    public GameObject lifePanel;

    // Store the movement and the component
    private Vector2 movement;
    private Rigidbody2D rigidbodyComponent;
    private Animator animator;

    // internal
    private float invincibleTime = 0f;
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
            Vector3 mousePosition = Input.mousePosition;
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = Vector2.Lerp(transform.position, mousePosition, 0.1f);
        }
        // Handle touch screen : Look for all fingers
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            // Touch are screens location. Convert to world
            Vector3 position = Camera.main.ScreenToWorldPoint(touch.position);
            transform.position = Vector2.Lerp(transform.position, position, 0.1f);
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

        // is ShieldUp or we just lose one life
        if (isInvincible)
        {
            invincibleTime -= Time.deltaTime;
            if (invincibleTime <= 0)
            {
                animator.SetBool("shieldUp", false);
                animator.SetBool("loseLife", false);
                SoundEffectsHelper.Instance.MakeShieldSound(false);
                isInvincible = false;
            }
        }

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

    void OnTriggerEnter(Collision2D collision)
    {

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        bool damagePlayer = false;
        // Ignore collision when player isinvincible
        Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>(), isInvincible);
        if (!isInvincible)
        {
            // Collision with enemy
            EnemyScript enemy = collision.gameObject.GetComponent<EnemyScript>();
            if (enemy != null && collision.gameObject.GetComponent<BossScript>() == null)
            {
                // Kill the enemy
                HealthScript enemyHealth = enemy.GetComponent<HealthScript>();
                if (enemyHealth != null) enemyHealth.Damage(enemyHealth.hp);

                damagePlayer = true;
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
                invincibleTime = shield.invincibleCoolDown;
                animator.SetBool("shieldUp", true);
                isInvincible = true;
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
        if (damagePlayer && this.takeDamage(100))
        {
            HealthScript playerHealth = this.GetComponent<HealthScript>();
            playerHealth.Damage(1);
        }
        
    }

    public bool takeDamage(int damage)
    {
        if (invincibleTime <= 0)
        {
            print(nbLife);
            nbLife--;
            if (nbLife > 0)
            {
                // Update life UI
                Image[] lifesUI = lifePanel.gameObject.GetComponentsInChildren<Image>();
                for (int i = 1; i <= lifesUI.Length - nbLife; i++)
                {
                    lifesUI[lifesUI.Length - i].enabled = false;
                }
                // Change animation
                animator.SetBool("loseLife", true);
                // Play sound
                SoundEffectsHelper.Instance.MakeLoseSound();
                // Makeplayerinvincible for 2 secondes
                invincibleTime = 2f;
                isInvincible = true;
            }
            else
            {
                // kill player
                return true; 
            }
        }
        return false;
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
