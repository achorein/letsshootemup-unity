using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour {

    private MoveScript moveScript;
    private SpriteRenderer rendererComponent;

    public float limitY = -1;
    public int points = 10;
    public float rotateSpeed = 0;

    void Awake()
    {
        // Retrieve scripts to disable when not spawn
        moveScript = GetComponent<MoveScript>();

        rendererComponent = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (rendererComponent.IsVisibleFrom(Camera.main))
        {
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

            if (rotateSpeed > 0)
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
