using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScript : MonoBehaviour {

    // 1 - Designer variables

    /// <summary>
    /// Object speed
    /// </summary>
    public Vector2 speed = new Vector2(10, 10);

    /// <summary>
    /// Moving direction
    /// </summary>
    public Vector2 direction = new Vector2(-1, 0);

    public Vector2 fixedPosition = Vector2.zero;

    private Vector2 movement;
    private bool hasFixedPosition = false;
    private Rigidbody2D rigidbodyComponent;
    private Vector2 positionTarget = Vector2.zero;

    // Use this for initialization
    void Start()
    {
    }

    void Update()
    {
        if (fixedPosition != Vector2.zero)
        {
            hasFixedPosition = true;
            positionTarget = Camera.main.ViewportToWorldPoint(fixedPosition);
            // Are we at the target? If so, find a new one
            if (GetComponent<Collider2D>().OverlapPoint(positionTarget))
            {
                // Reset, will be set at the next frame
                fixedPosition = Vector2.zero;
            }

            // Go to the point
            Vector3 newDirection = ((Vector3)positionTarget - this.transform.position);
            direction = Vector3.Normalize(newDirection);
        }

        if (fixedPosition != Vector2.zero || !hasFixedPosition)
        {
            // 2 - Movement
            movement = new Vector2(
              speed.x * direction.x,
              speed.y * direction.y);
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (rigidbodyComponent == null) rigidbodyComponent = GetComponent<Rigidbody2D>();

        // Apply movement to the rigidbody
        rigidbodyComponent.velocity = movement;
    }

    public Vector2 getPositionTarget()
    {
        return positionTarget;
    }
    
}
