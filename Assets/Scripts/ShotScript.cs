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

    [Header("IA behavior")]
    public bool autoAim;
    public GameObject autoAimTarget;

    void Start() {
        // Limited time to live to avoid any leak
        Destroy(gameObject, 7); // 7sec
    }

    // Update is called once per frame
    void Update() {
        // autoaim
        if (autoAimTarget != null) {
            // move to target
            var heading = autoAimTarget.transform.position - transform.position;
            heading.Normalize();
            var move = transform.gameObject.GetComponent<MoveScript>();
            move.direction = heading;
            // rotate to target
            var parentTransform = gameObject.transform;
            var targetPosition = autoAimTarget.transform.position;
            var rotationAngle = Quaternion.LookRotation(targetPosition - parentTransform.position, Vector3.back); // we get the angle has to be rotated
            rotationAngle.x = 0;
            rotationAngle.y = 0;
            parentTransform.rotation = rotationAngle;
        } else if (rotateSpeed != 0) {
            /* Rotate thet transform of the game object this is attached to by 45 degrees, taking into account the time elapsed since last frame. */
            transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime * rotateSpeed);
        }
    }

    public bool isExpandable() {
        return laserStart != null;
    }
}
