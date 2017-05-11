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
    private GameOverScript gameOverScript;
    private int maxUpgrade = 10;

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
    public static WeaponScript[] lastWeaponBonus = null;

    void Awake() {
        // Get external components
        animator = GetComponent<Animator>();
        gameOverScript = FindObjectOfType<GameOverScript>();
        maxUpgrade = (gameOverScript.currentLevel>0)?gameOverScript.currentLevel:10;

        cursorDistance = Vector3.zero;
        beginning = true;

        // restore previous level state
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
            lastWeaponBonus = new WeaponScript[10];
            changeWeapon(GameHelper.Instance.primaryBonus, null, true);
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
            // Retrieve axis information (keyboard)
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");
            if (inputX > 0 || inputY > 0 ) {
                pauseTimer = 1f;
            }

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

    /// <summary>
    /// triggered when player collides with an object
    /// </summary>
    /// <param name="collision"></param>
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

    /// <summary>
    /// Load player weapon (with upgrade)
    /// </summary>
    /// <param name="gameObject">prefab instance of any bonus</param>
    /// <param name="otherWeapons">old weapon to reuse</param>
    private void changeWeapon(GameObject gameObject, WeaponScript[] otherWeapons=null, bool reinit=false) {
        // retrive current player weapons
        WeaponScript[] weapons = GetComponentsInChildren<WeaponScript>();
        if (otherWeapons != null) { // restore weapon from previous game level
            copyWeapons(otherWeapons, weapons, true);
            return;
        }
        // check if gameobject contains weapon (bonus)
        WeaponScript[] bonusWeapons = gameObject.GetComponentsInChildren<WeaponScript>();

        // handle bonus
        if (bonusWeapons != null && bonusWeapons.Length > 0) {
            // inspect current weapons
            string primaryWeapon = weapons[0].shotPrefab.tag;
            string secondaryWeapon = "";
            int upgradeLevel = 0;
			foreach (WeaponScript weapon in weapons) {
				if (weapon.enabled && !weapon.secondaryWeapon) {
                    if (weapon.upgradeLevel > upgradeLevel) {
                        upgradeLevel = weapon.upgradeLevel;
                    } else if (upgradeLevel == 0 && weapon.upgraded && weapon.upgradeLevel == upgradeLevel) {
                        upgradeLevel = weapon.upgradeLevel + 1;
                    }
                } else if (weapon.enabled && weapon.secondaryWeapon && weapon.shotPrefab) {
                    //print("old secondary : " + weapon.shotPrefab.tag);
                    secondaryWeapon = weapon.shotPrefab.tag;
                }
            }
            // replace current weapon
            if (reinit || (upgradeLevel == 0 && bonusWeapons[0].shotPrefab.tag != primaryWeapon) /* no upgraded yet */) {
                print("replace primary weapon with new one " + gameObject.tag + " (" + reinit + ") " + upgradeLevel);
                if (upgradeLevel > 0) upgradeLevel--;
                replacePrimaryWeapon(bonusWeapons, weapons, upgradeLevel);
            } else {
                if (bonusWeapons[0].shotPrefab.tag == primaryWeapon) {
                    print("upgrade primary weapon " + gameObject.tag + " => " + (upgradeLevel + 1));
                    // upgrade primary weapon
                    if (upgradeLevel < maxUpgrade) {
                        GameHelper.Instance.upgradeWeapon(primaryWeapon);
                        for (int i = 0; i < bonusWeapons.Length; i++) {
                            updateWeapon(weapons[i], bonusWeapons[i], upgradeLevel + 1);
                        }
                    }
                } else if (bonusWeapons[0].shotPrefab.tag == secondaryWeapon) {
                    print("switch secondary and primary weapon " + gameObject.tag + " (" + upgradeLevel + ")");
                    // switch secondary and primary weapon
                    replacePrimaryWeapon(bonusWeapons, weapons, upgradeLevel);
                    //replaceSecondaryWeapon(bonusWeapons, weapons, upgradeLevel); // how to retreive old principal ?
                } else { // replace secondary weapon with new one
                    print("replace secondary weapon with new one " + gameObject.tag + " (old: " + secondaryWeapon + ")");
                    replaceSecondaryWeapon(bonusWeapons, weapons, upgradeLevel);
                }
            }
            copyWeapons(weapons, lastWeaponBonus);
        }
    }

    private void copyWeapons(WeaponScript[] weaponsToCopy, WeaponScript[] weapons, bool restore=false) {
        for (int i = 0; i < weaponsToCopy.Length; i++) {
            if (weapons[i] == null) weapons[i] = new WeaponScript();
            var currentWeapon = weapons[i];
            var currentWeaponToCopy = weaponsToCopy[i];
            if ((!restore && currentWeaponToCopy.enabled) || (restore && currentWeaponToCopy.lastEnabled)) {
                if (restore) currentWeapon.enabled = true;
                else currentWeapon.lastEnabled = true;
                if (restore) print("restore weapon " + currentWeaponToCopy.shotPrefab.tag + ", level: " + currentWeaponToCopy.upgradeLevel + ", upgraded: " + currentWeaponToCopy.upgraded + ", sec: " + currentWeaponToCopy.secondaryWeapon + ", posOffset: " + currentWeaponToCopy.positionOffset);
                // upgrade level
                currentWeapon.upgradeLevel = currentWeaponToCopy.upgradeLevel;
                currentWeapon.upgraded = currentWeaponToCopy.upgraded;
                currentWeapon.secondaryWeapon = currentWeaponToCopy.secondaryWeapon;
                // standard weapon properties
                currentWeapon.shotPrefab = currentWeaponToCopy.shotPrefab;
                currentWeapon.expandable = currentWeaponToCopy.expandable;
                // shooting rate
                currentWeapon.shootingRate = currentWeaponToCopy.shootingRate;
                // position
                currentWeapon.positionOffset = currentWeaponToCopy.positionOffset;
                if (restore) {
                    // position
                    if (currentWeapon.upgraded || currentWeapon.secondaryWeapon) {
                        currentWeapon.transform.position = new Vector3(transform.position.x + currentWeapon.positionOffset, transform.position.y, transform.position.z);
                    } else {
                        currentWeapon.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                    }
                }
                
                // rotation
                if (restore) currentWeapon.gameObject.transform.eulerAngles = currentWeaponToCopy.lastAngles;
                else currentWeapon.lastAngles = currentWeaponToCopy.gameObject.transform.eulerAngles;
            } else if (restore) {
                currentWeapon.enabled = false;
                currentWeapon.lastEnabled = false;
            }
        }
    }

    private void replacePrimaryWeapon(WeaponScript[] bonusWeapons, WeaponScript[] weapons, int upgradeLevel) {
        // disable all weapons
        foreach (WeaponScript weapon in weapons) {
            weapon.DestroyLaser();
            weapon.enabled = false;
        }
        // add standard weapon 
        for (int i = 0; i < bonusWeapons.Length; i++) {
            updateWeapon(weapons[i], bonusWeapons[i], upgradeLevel);
        }
    }

    private void replaceSecondaryWeapon(WeaponScript[] bonusWeapons, WeaponScript[] weapons, int upgradeLevel) {
        foreach (WeaponScript weapon in weapons) { 
            // disable old secondary weapon
            if (weapon.enabled && weapon.secondaryWeapon) {
                weapon.DestroyLaser();
                weapon.enabled = false;
            }
        }
        // add secondary weapon 
        int secondaryCount = 1;
        foreach (WeaponScript currentBonusWeapons in bonusWeapons) {
            if (currentBonusWeapons.secondaryWeapon) {
                updateWeapon(weapons[weapons.Length - secondaryCount], currentBonusWeapons, upgradeLevel, true);
                secondaryCount++;
            }
        }
    }

    private void updateWeapon(WeaponScript currentWeapon, WeaponScript currentBonusWeapons, int upgradeLevel, bool secondary=false) {
        if (!secondary && currentBonusWeapons.upgradeLevel > upgradeLevel) return; // do not replace current weapon when not necessary
        if (secondary && !currentBonusWeapons.secondaryWeapon) return; // do not replace current weapon when not necessary
        if (!secondary && currentBonusWeapons.secondaryWeapon) return; // do not replace current weapon when not necessary

        // upgrade level
        currentWeapon.upgradeLevel = currentBonusWeapons.upgradeLevel;
        currentWeapon.upgraded = upgradeLevel > 0 && !secondary; // || currentBonusWeapons.upgradeLevel < upgradeLevel;
        currentWeapon.secondaryWeapon = secondary && currentBonusWeapons.secondaryWeapon;
        // standard weapon properties
        currentWeapon.shotPrefab = currentBonusWeapons.shotPrefab;
        currentWeapon.expandable = currentBonusWeapons.expandable;
        currentWeapon.enabled = true;
        // shooting rate
        if (currentWeapon.upgraded) {
            currentWeapon.shootingRate = currentBonusWeapons.upgradeShootingRate;
        } else {
            currentWeapon.shootingRate = currentBonusWeapons.shootingRate;
        }
        // position
        currentWeapon.positionOffset = currentBonusWeapons.positionOffset;
        if (currentWeapon.upgraded || secondary) {
            currentWeapon.transform.position = new Vector3(transform.position.x + currentBonusWeapons.positionOffset, transform.position.y, transform.position.z);
        } else {
            currentWeapon.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }
        // rotation
        currentWeapon.gameObject.transform.eulerAngles = new Vector3(currentWeapon.transform.eulerAngles.x, currentWeapon.transform.eulerAngles.y, currentBonusWeapons.rotation);
    }

    /// <summary>
    /// Handle dommage on player (reduce shield ? kill player ?)
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
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
                // reinit weapon with primary
                changeWeapon(GameHelper.Instance.primaryBonus, null, true);
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

    /// <summary>
    /// Refresh UI status bar
    /// </summary>
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

    /// <summary>
    /// Refresh UI status bar
    /// </summary>
    /// <param name="loseLife"></param>
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
        if (gameOverScript != null && gameOverScript.ready) {
            gameOverScript.EndGame(false);
        }
    }
    
    internal static void reset() {
        PlayerScript.lastShieldLevel = 0;
        PlayerScript.lastLife = 0;
        PlayerScript.lastWeaponBonus = null;
    }
}
