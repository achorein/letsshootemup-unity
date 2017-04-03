using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    private const int MAX_LIFE = 3;
    private const int MAX_BOMB = 1;

    private Vector2 speed = new Vector2(7, 7);
    private float mouseSpeed = 0.05f;
    private int nbLife = 2;

    // Store the movement and the component
    private Vector2 movement, cursorDistance;
    private Rigidbody2D rigidbodyComponent;
    private Animator animator;

    // internal
    internal int shieldLevel = 0;
    internal int nbBombs = 0;
    internal bool isInvincible = false;
    internal int nbHitTaken = 0;
    internal bool beginning = true;
    internal float pauseTimer = 0;

    public static int lastLife = 0;
    public static int lastShieldLevel = 0;
    public static int lastBomb = 0;
    public static String lastWeapon = "";
    public static bool lastWeaponUpgraded = false;
    public static WeaponScript[] lastWeaponBonus = null;

    void Awake() {
        // Get the animator
        animator = GetComponent<Animator>();

        cursorDistance = Vector3.zero;
        beginning = true;

        if (lastShieldLevel > 0) {
            shieldLevel = lastShieldLevel;
            updateShieldUi();
        }
        if (lastLife > MAX_LIFE) {
            nbLife = MAX_LIFE;
        } else if (lastLife > 0) {
            nbLife = lastLife;
        } else {
            lastLife = nbLife;
            GetComponent<HealthScript>().hp += (GameHelper.Instance.getCurrentShip().hp - 1);
        }
        if (lastBomb > 0) {
            nbBombs = lastBomb;
            GameHelper.Instance.pickupBomb(); // activate button
        }
        if (lastWeaponBonus != null) {
            changeWeapon(null, lastWeaponBonus);
        } else {
            lastWeapon = "";
            lastWeaponUpgraded = false;
            changeWeapon(GameHelper.Instance.primaryBonus);
        }

        // Update lifes UI
        Image[] lifesUI = GameHelper.Instance.lifePanel.gameObject.GetComponentsInChildren<Image>();
        for (int i = 1; i <= lifesUI.Length - nbLife; i++) {
            lifesUI[lifesUI.Length - i].enabled = false;
        }

        foreach (Image life in lifesUI) {
            life.sprite = Resources.Load<Sprite>(GameHelper.Instance.getCurrentShip().sprite);
        }
        GameHelper.Instance.shieldUi.GetComponent<Image>().sprite = Resources.Load<Sprite>(GameHelper.Instance.getCurrentShip().sprite);
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(GameHelper.Instance.getCurrentShip().sprite);

        mouseSpeed *= GameHelper.Instance.getCurrentShip().speed * 1.25f;
    }

    // Update is called once per frame
    void Update() {
        if (Time.timeScale != 0) {
            // Retrieve axis information
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");

            Vector2 cursorPosition = Vector2.zero;
            // Handle Mouse
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
                cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            // Handle touch screen
            if (Input.touchCount > 0) {
                cursorPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
            if (cursorPosition != Vector2.zero) {
                beginning = false;
                pauseTimer = 1f;
                if (cursorDistance == Vector2.zero) {
                    cursorDistance = cursorPosition - new Vector2(transform.position.x, transform.position.y);
                }
                transform.position = Vector2.Lerp(transform.position, cursorPosition - cursorDistance, mouseSpeed);
            } else {
                cursorDistance = Vector2.zero;
                if (!beginning) {
                    pauseTimer -= Time.deltaTime;
                    if (pauseTimer <= 0) {
                        FindObjectOfType<PauseScript>().PauseGame();
                    }
                }
            }

            // Movement per direction
            movement = new Vector2(
              speed.x * inputX,
              speed.y * inputY);
        }
        // Shooting
        bool shoot = Input.GetButton("Fire1");
        shoot |= Input.GetButton("Fire2");
        // Careful: For Mac users, ctrl + arrow is a bad idea

        if (shoot) {
            WeaponScript[] weapons = GetComponentsInChildren<WeaponScript>();
            foreach (WeaponScript weapon in weapons) {
                // false because the player is not an enemy
                if (weapon.enabled && weapon.Attack(false)) {
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
        if (Input.GetKeyUp(KeyCode.Escape)) {
			FindObjectOfType<PauseScript>().resetAd();
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
    }

    void FixedUpdate() {
        // Get the component and store the reference
        if (rigidbodyComponent == null) rigidbodyComponent = GetComponent<Rigidbody2D>();

        // Move the game object
        rigidbodyComponent.velocity = movement;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        int damagePlayer = 0;
        // Ignore collision when player is invincible
        Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>(), isInvincible);
        if (!isInvincible) {
            // Collision with enemy
            EnemyScript enemy = collision.gameObject.GetComponent<EnemyScript>();
            if (enemy != null && collision.gameObject.GetComponent<BossScript>() == null) {
                // Kill the enemy
                HealthScript enemyHealth = enemy.GetComponent<HealthScript>();
                if (enemyHealth != null) {
                    damagePlayer = enemyHealth.hp / 3;
                    if (shieldLevel + 1 >= damagePlayer) {
                        enemyHealth.Damage(enemyHealth.hp); // kill enemy
                    }
                }
                if (damagePlayer < 1) {
                    damagePlayer = 1;
                }
            }
        }

        // Is this a bonus?
        CollectableScript collectable = collision.gameObject.GetComponentInChildren<CollectableScript>();
        if (collectable != null) {
            // Is this a shield bonus?
            ShieldScript shield = collision.gameObject.GetComponent<ShieldScript>();
            if (shield != null) {
                shieldLevel = shield.shieldLevel;
                lastShieldLevel = shieldLevel;
                updateShieldUi();
                updateLifeUi(false);
                SoundEffectsHelper.Instance.MakeShieldSound(true);
                Destroy(shield.gameObject); // Remember to always target the game object, otherwise you will just remove the script
            } else {
                SoundEffectsHelper.Instance.MakePickupSound();
            }
            // Is this a weapon bonus?
            changeWeapon(collision.gameObject);
            // Is this a bomb bonus?
            BombScript bomb = collision.gameObject.GetComponent<BombScript>();
            if (bomb != null) {
                if (nbBombs < MAX_BOMB) {
                    nbBombs++;
                    GameHelper.Instance.pickupBomb();
                }
                SoundEffectsHelper.Instance.MakePickupSound();
            }

            GameHelper.Instance.collectBonus(collectable.getId());
            Destroy(collision.gameObject); // Remember to always target the game object, otherwise you will just remove the script
        }

        // Damage the player if necessery
        if (this.takeDamage(damagePlayer)) {
            GetComponent<HealthScript>().Damage(1);
        }

    }

    private void changeWeapon(GameObject gameObject) {
        changeWeapon(gameObject, null);
    }

    /// <summary>
    /// Load player weapon (with upgrade)
    /// </summary>
    /// <param name="gameObject">prefab instance of any bonus</param>
    /// <param name="otherWeapons">old weapon to reuse</param>
    private void changeWeapon(GameObject gameObject, WeaponScript[] otherWeapons) {
        WeaponScript[] bonusWeapons = otherWeapons;
        if (bonusWeapons == null) {
            bonusWeapons = gameObject.GetComponentsInChildren<WeaponScript>();
        }
        // handle bonus
        if (bonusWeapons != null && bonusWeapons.Length > 0) {
            bool upgraded = false;
            if ((gameObject != null && gameObject.tag == lastWeapon) // take same weapon bonus
                || (otherWeapons != null && lastWeaponUpgraded == true)) // upgrade from older weapon
            {
                // upgrade weapon
                GameHelper.Instance.upgradeWeapon(lastWeapon);
                upgraded = true; // upgrade now
                lastWeaponUpgraded = true; // upgrade on next level
            } else {                
                lastWeaponUpgraded = false; // don't upgrade on next level
            }
            lastWeaponBonus = bonusWeapons;
            WeaponScript[] weapons = GetComponentsInChildren<WeaponScript>();
            foreach (WeaponScript weapon in weapons) {
                weapon.enabled = false;
            }
            for (int i = 0; i < bonusWeapons.Length; i++) {
               if (upgraded) {
                    // need to upgrade weapon
                    if (bonusWeapons[i].upgradeShotPrefab != null) {
                        weapons[i].shotPrefab = bonusWeapons[i].upgradeShotPrefab;
                    } else {
                        weapons[i].shotPrefab = bonusWeapons[i].shotPrefab;
                    }
                    weapons[i].shootingRate = bonusWeapons[i].upgradeShootingRate;
                    weapons[i].gameObject.transform.eulerAngles = new Vector3(weapons[i].transform.eulerAngles.x, weapons[i].transform.eulerAngles.y, bonusWeapons[i].rotation);
                    weapons[i].transform.position = new Vector3(transform.position.x + bonusWeapons[i].positionOffset, transform.position.y, transform.position.z);
                    weapons[i].enabled = true;
                } else {
                    // normal weapon
                    weapons[i].shotPrefab = bonusWeapons[i].shotPrefab;
                    weapons[i].enabled = bonusWeapons[i].upgrade == false;
                    weapons[i].shootingRate = bonusWeapons[i].shootingRate;
                    weapons[i].gameObject.transform.eulerAngles = new Vector3(weapons[i].transform.eulerAngles.x, weapons[i].transform.eulerAngles.y, bonusWeapons[i].rotation);
                    weapons[i].transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                }
                weapons[i].expandable = bonusWeapons[i].expandable;
                weapons[i].upgraded = upgraded;
            }
            if (gameObject != null) lastWeapon = gameObject.tag;
        }
    }

    public bool takeDamage(int damage) {
        if (!isInvincible && damage > 0) {
            nbHitTaken++;
            int realDamage = damage - shieldLevel;
            if (shieldLevel > 0) {
                shieldLevel -= damage;
                lastShieldLevel = shieldLevel;
                updateShieldUi();
            }
            if (realDamage > 0) {
#if UNITY_ANDROID
                if (GameHelper.Instance.playerPref.vibrationOn) {
                    Handheld.Vibrate();
                }
#endif
                lastWeapon = "";
                lastWeaponUpgraded = false;
                changeWeapon(GameHelper.Instance.primaryBonus);
                nbLife--;
                lastLife = nbLife;
                if (nbLife > 0) {
                    // Update Ui
                    animator.SetBool("loseLife", true);
                    updateLifeUi(true);
                    // Invincible
                    isInvincible = true;
                    Invoke("disableInvincible", 2f); // 2 sec
                    // Play sound
                    SoundEffectsHelper.Instance.MakeLoseLifeSound();
					GameHelper.Instance.showRedScreen(true);
					Invoke("resetScreenColor", 0.25f);
                    //Time.timeScale = 0.5f;
                    //Invoke("resetTimeScale", 1f); // 1 sec but scaled
                } else {
                    // kill player
                    return true;
                }
            }
        }
        return false;
    }

    private void updateShieldUi() {
        if (shieldLevel < 0) shieldLevel = 0;
        animator.SetInteger("shieldLevel", shieldLevel <= 3 ? shieldLevel : 3);
        GameHelper.Instance.shieldUi.SetActive(shieldLevel > 0);
        if (GameHelper.Instance.shieldUi.activeSelf) {
            Image[] shields = GameHelper.Instance.shieldUi.GetComponentsInChildren<Image>();
            for (int i = 1; i < shields.Length; i++) {
                if (shieldLevel == i) {
                    shields[i].enabled = true;
                } else {
                    shields[i].enabled = false;
                }
            }
        }
    }

    private void updateLifeUi(bool loseLife) {
        // Update life UI
        Image[] lifesUI = GameHelper.Instance.lifePanel.gameObject.GetComponentsInChildren<Image>();
        for (int i = 1; i <= lifesUI.Length - nbLife; i++) {
            Image lifeUI = lifesUI[lifesUI.Length - i];
            Animator lifeAnimator = lifeUI.GetComponent<Animator>();
            if (loseLife) {
                lifeAnimator.SetBool("loseLife", true);
            } else if (lifeAnimator.GetBool("loseLife")) {
                lifeAnimator.SetBool("loseLife", false);
                lifeUI.enabled = false;
            }

        }

    }

    void disableInvincible() {
        animator.SetBool("loseLife", false);
        updateShieldUi();
        updateLifeUi(false);
        isInvincible = false;
        SoundEffectsHelper.Instance.MakeShieldSound(false);
    }

    void resetTimeScale() {
        Time.timeScale = 1;
    }

	void resetScreenColor() {
		GameHelper.Instance.showRedScreen(false);
	}

    void OnDestroy() {
        // Game Over.
        var gameOver = FindObjectOfType<GameOverScript>();
        if (gameOver != null && gameOver.ready) {
            gameOver.EndGame(false);
        }
    }
    
    internal static void reset() {
        PlayerScript.lastShieldLevel = 0;
        PlayerScript.lastLife = 0;
        PlayerScript.lastWeaponBonus = null;
        PlayerScript.lastWeapon = "";
        PlayerScript.lastWeaponUpgraded = false;
    }
}
