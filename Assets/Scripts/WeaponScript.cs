using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour {

    //--------------------------------
    // 1 - Designer variables
    //--------------------------------

    [Header("Normal weapon")]
    /// <summary>
    /// Projectile prefab for shooting
    /// </summary>
    public Transform shotPrefab;
    /// <summary>
    /// Cooldown in seconds between two shots
    /// </summary>
    public float shootingRate = 0.25f;
    public float rotateSpeed = 0;

    [Header("Upgradable weapon")]
	public int upgradeLevel = 0;
    public bool secondaryWeapon = false;
    public float upgradeShootingRate = 0.25f;
    public float positionOffset = 0f;
    public int rotation = 90;
    //public Transform upgradeShotPrefab;

    [Header("Other")]
    public bool tryRotateShoot = false;

    [Header("IA behavior")]
    public bool autoAim = false;
    public bool forceAutoFire = false;
    public bool rotateToTarget = false;
    public GameObject autoFireTarget;

    internal bool lastEnabled = false;
    internal Vector3 lastAngles;
    internal bool upgraded = false;
    private float shootCooldown;

    // Expandable laser
    internal bool expandable = false;
    private float offsetY = 0.44f;
    private float maxLaserDistance = 15f;
    private float timer = 0f;
    private float timerMax = 0.5f;
    private float startSpriteWidth;

    private GameObject start, middle, end;

    void Start() {
        shootCooldown = 0f;
    }

    void Update() {
        if (shootCooldown > 0) {
            shootCooldown -= Time.deltaTime;
        }
        if (expandable) {
            FadeLaser();
        } else {
            DestroyLaser();
        }

        if (rotateToTarget && autoFireTarget != null) {
            var parentTransform = GetComponentInParent<EnemyScript>().gameObject.transform;
            var targetPosition = autoFireTarget.transform.position;

			var rotationAngle = Quaternion.LookRotation(targetPosition - parentTransform.position, Vector3.forward); // we get the angle has to be rotated
            rotationAngle.x = 0;
            rotationAngle.y = 0;
			parentTransform.rotation = rotationAngle; 
			// Quaternion.Slerp(parentTransform.rotation, rotationAngle, Time.deltaTime * 5); // we rotate the rotationAngle 

            //parentTransform.LookAt(autoFireTarget.transform);
            //parentTransform.eulerAngles = new Vector3(0, 0, parentTransform.eulerAngles.z);
        }
        if (rotateSpeed != 0) {
            //Rotate thet transform of the game object this is attached to by 45 degrees, taking into account the time elapsed since last frame.
            transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime * rotateSpeed);
        }
    }

    //--------------------------------
    // 3 - Shooting from another script
    //--------------------------------

    /// <summary>
    /// Create a new projectile if possible
    /// </summary>
    public bool Attack(bool isEnemy) {
        if (shotPrefab && (CanAttack || expandable)) {
            Transform shotTransform = Instantiate(shotPrefab) as Transform;
            ShotScript shot = shotTransform.GetComponent<ShotScript>();
            // The enemy property
            shot.isEnemyShot = isEnemy;

            // Create a new shot
            if (shot.isExpandable()) {
                expandable = true;
                timer = timerMax;
				// laser size
                float scale = 1.5f;
                if (secondaryWeapon || upgradeLevel > 1) scale = 0.5f;
                else if (!upgraded) scale = 0.75f;
                
				// laser direction : x vector (will rotate with player transform rotation)
                startSpriteWidth = shot.laserStart.GetComponent<Renderer>().bounds.size.x;
                InstantiateLaserPart(ref start, shot.laserStart);
                start.transform.localPosition = new Vector3(offsetY, 0f);
                InstantiateLaserPart(ref middle, shot.laserMiddle);

                float currentLaserDistance = maxLaserDistance;
                RaycastHit2D hit = RaycastDirection(this.transform.right);
                if (hit.collider != null) {
                    currentLaserDistance = Vector2.Distance(hit.point, this.transform.position) + 1.1f;
                    InstantiateLaserPart(ref end, shot.laserEnd);
                    // handle damage to enemy
					if (CanAttack) {
                        HealthScript health = hit.collider.gameObject.GetComponent<HealthScript>();
                        if (health != null) {
                            health.hitBy(shotTransform.GetComponent<Collider2D>());
                        }
                        shootCooldown = shootingRate;
                    }
                } else if (end != null)
                    Destroy(end);

                start.transform.localScale = new Vector3(
                    scale,
                    start.transform.localScale.y,
                    start.transform.localScale.z
                );
                middle.transform.localScale = new Vector3(
                    scale, //middle.transform.localScale.x,
					100 * (currentLaserDistance - startSpriteWidth),
                    middle.transform.localScale.z
                );
                middle.transform.localPosition = new Vector3((currentLaserDistance / 2f) + 0.15f, 0f);

                if (end != null) {
                    end.transform.localPosition = new Vector2(currentLaserDistance, 0f);
                    end.transform.localScale = new Vector3(
                        scale,
                        end.transform.localScale.y,
                        end.transform.localScale.z
                    );
                }
                Destroy(shotTransform.gameObject);
                return false; // no shoot sound
            } else {
                shootCooldown = shootingRate;

                // Make the weapon shot always towards it
                MoveScript move = shotTransform.gameObject.GetComponent<MoveScript>();
                if (move != null) {
                    if (autoAim && autoFireTarget.activeSelf) {
                        var heading = autoFireTarget.transform.position - transform.position;
                        heading.Normalize();
                        move.direction = heading;
                    } else {
                        move.direction = this.transform.right; // towards in 2D space is the right of the sprite
                    }
                }
				// Normal shot
				shotTransform.position = transform.position;
				if (tryRotateShoot) {
					shotTransform.transform.Rotate(transform.rotation.eulerAngles);
				}
                if (shot.autoAim) {
                    // Retreive all ennemie on screen
                    Collider2D[] hits = Physics2D.OverlapCircleAll(
                        transform.position, 10f,
                        1 << LayerMask.NameToLayer("Enemies") // colide only with layer Enemies
                    );
                    // find closest enemy
                    var curdist = 10f;
                    foreach (Collider2D collider in hits) {
                        var distance = Vector3.Distance(collider.transform.position, transform.position);
                        HealthScript health = collider.gameObject.GetComponent<HealthScript>();
                        if (health != null && distance < curdist) {
                            curdist = distance;
                            // define enemy to aim
                            shot.autoAimTarget = collider.gameObject;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Is the weapon ready to create a new projectile?
    /// </summary>
    public bool CanAttack {
        get {
            return shootCooldown <= 0f;
        }
    }

    void InstantiateLaserPart(ref GameObject part, GameObject laserPart) {
        if (part == null) {
            part = Instantiate<GameObject>(laserPart);
            part.transform.parent = gameObject.transform;
            part.transform.localPosition = Vector2.zero;
        }
    }

    RaycastHit2D RaycastDirection(Vector2 direction) {
        Vector3 newPos = new Vector3(
            this.transform.position.x,
            this.transform.position.y + offsetY,
            this.transform.position.z);
        return Physics2D.Raycast(
            newPos,
            direction,
            maxLaserDistance,
            1 << LayerMask.NameToLayer("Enemies") // colide only with layer Enemies
        );
    }

    void FadeLaser() {
        if (start != null) {
            timer -= Time.deltaTime;
            if (timer <= 0.25) {
                var reduce = new Vector3(transform.localScale.x * Time.deltaTime, 0f);
                start.transform.localScale -= reduce;
                middle.transform.localScale -= reduce;
                if (end != null) middle.transform.localScale -= reduce;

                if (middle.transform.localScale.x <= 0.05) {
                    Destroy(start.gameObject);
                    Destroy(middle.gameObject);
                    if (end != null) { Destroy(middle.gameObject); }
                    expandable = false;
                }
            }
        }
    }

    internal void DestroyLaser() {
        if (start != null) Destroy(start.gameObject);
        if (middle != null) Destroy(middle.gameObject);
        if (end != null) Destroy(end.gameObject);
    }

}
