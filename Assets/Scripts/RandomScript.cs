using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomScript : MonoBehaviour {

    public float ennemySpawnTime = 3.0f;
    public float decorsSpawnTime = 1.75f;

    public GameObject[] ennemies;
    public GameObject[] decors;
    
    public GameObject player;

    public Transform ennemyParent;
    public Transform decorsParent;

    private const float SPAWN_MIN_TIME = 0.85f;
    private const float SPAWN_MAX_TIME = 9f;
    private const float SPAWN_STEP_TIME = 0.03f;
    private const float SPAWN_RANDOM_STEP_TIME = SPAWN_MIN_TIME - (SPAWN_STEP_TIME * 10);

    // Use this for initialization
    void Start () {
        InvokeRepeating("SpawnDecors", decorsSpawnTime, decorsSpawnTime);
        // Ennemy
        StartCoroutine(SpawnEnnemy());
    }

    IEnumerator SpawnEnnemy() {
        while (true) {
            float waitTime = Random.Range(
                ennemySpawnTime - SPAWN_RANDOM_STEP_TIME, 
                ennemySpawnTime + SPAWN_RANDOM_STEP_TIME
            );
            waitTime = Mathf.Clamp(waitTime, SPAWN_MIN_TIME, SPAWN_MAX_TIME);
            yield return new WaitForSeconds(waitTime);
            var ennemy = Spawn(ennemies);
            ennemy.transform.parent = ennemyParent;
            foreach (WeaponScript weapon in ennemy.GetComponentsInChildren<WeaponScript>()) {
                if (weapon.autoAim) {
                    weapon.autoFireTarget = player;
                }
            }
            if (ennemySpawnTime > SPAWN_MIN_TIME) {
                ennemySpawnTime = ennemySpawnTime - SPAWN_STEP_TIME;
            }
        }
    }

    void SpawnDecors() {
        Spawn(decors).transform.parent = decorsParent;
    }

    GameObject Spawn(GameObject[] objects) {
        Vector3 spawnPosition = getPosition();
        return Instantiate(
            objects[Random.Range(0, objects.Length)],
            spawnPosition,
            Quaternion.identity);
    }

    private Vector3 getPosition() {
        var dist = 0;
        var leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).x;
        var rightBorder = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist)).x;
        var topBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1.15f, dist)).y;
        var bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist)).y;

        return new Vector3(
          Random.Range(leftBorder,rightBorder),
          Random.Range(bottomBorder, topBorder),
          dist
        );
    }

}
