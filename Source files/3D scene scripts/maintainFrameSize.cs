using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class maintainFrameSize : MonoBehaviour {

    public int type; // 0 - horizontal, 1 - vertical
    private Vector3 size;
       
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void restoreSize()
    {
        
    }

    public void setSize()
    {
        size = transform.localScale;
        if(type == 0)
        {
            
        }
    }
}
