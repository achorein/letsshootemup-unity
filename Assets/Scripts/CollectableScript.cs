using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableScript : MonoBehaviour {

    private SpriteRenderer rendererComponent;

    public int points = 50;
    public float rotateSpeed = 2;

    void Awake()    
    {
        rendererComponent = GetComponent<SpriteRenderer>();
    }

    //Update is called every frame
    void Update()
    {
        if (rotateSpeed > 0)
        {
            //Rotate thet transform of the game object this is attached to by 45 degrees, taking into account the time elapsed since last frame.
            transform.Rotate(new Vector3(0, 0, 45) * Time.deltaTime * rotateSpeed);
        }
    }

    public string getId()
    {
        return rendererComponent.sprite.name;
    }

    void OnDestroy()
    {
        if (rendererComponent.IsVisibleFrom(Camera.main))
        {
            GameHelper.Instance.UpdateScore(points);
        }
    }

}
