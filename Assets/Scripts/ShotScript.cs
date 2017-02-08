using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotScript : MonoBehaviour {

    /// <summary>
    /// Damage inflicted
    /// </summary>
    public int damage = 1;

    /// <summary>
    /// Projectile damage player or enemies?
    /// </summary>
    public bool isEnemyShot = false;

    public float rotateSpeed = 0;

    [Header("Laser pieces")]
    public GameObject laserStart;
    public GameObject laserMiddle;
    public GameObject laserEnd;

    void Start() {
        // 2 - Limited time to live to avoid any leak
        Destroy(gameObject, 7); // 7sec
        if (rotateSpeed != 0) {
            /* 
             * Rotate thet transform of the game object this is attached to by 45 degrees, 
             * taking into account the time elapsed since last frame.
             */
            transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime * rotateSpeed);
        }
    }

    // Update is called once per frame
    void Update() {

    }

    public bool isExpandable() {
        return laserStart != null;
    }
}
