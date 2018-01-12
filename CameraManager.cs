using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    Transform parent;

	// Use this for initialization
	void Start ()
    {
        parent = GameObject.Find("CodeBase").transform;
	}

    // Update is called once per frame
    void Update()
    {
        if (parent.childCount > 0)
        {
            Vector3 position = new Vector3(0, 0, 0);
            for (int i = 0; i < parent.childCount; i++)
            {
                position += parent.GetChild(i).position;
            }
            position /= parent.childCount;
            this.transform.position = new Vector2(position.x, position.y);
        }
        else
        {
            this.transform.position = new Vector2(0, 0);
        }
    }
}
