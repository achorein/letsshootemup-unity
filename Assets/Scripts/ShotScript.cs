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

    void Start()
    {
        // 2 - Limited time to live to avoid any leak
        Destroy(gameObject, 7); // 7sec
    }

    // Update is called once per frame
    void Update () {
		
	}
}
