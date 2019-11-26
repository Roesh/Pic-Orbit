using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class frameSizeMaintainer : MonoBehaviour {

    private Vector3 initialParentScale;   // Initial scale before imgsizeadjuster scales the image
    private Vector3 adjustedParentScale;  // Adjusted scale after imgsizeadjuster scales it    

    public int type; // 0 - horizontal. 1 - vertical
    public bool sizeAdjusted;

    void Awake()
    {
        storeCurrentScale();
        sizeAdjusted = false;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // Maintain the x component of the scale
        if (!sizeAdjusted)
        {
            Vector3 tempScale = transform.localScale;
            float scalingFactor = 1f;
            if (type == 0) // Horizontal
            {
                scalingFactor = transform.parent.localScale.y / initialParentScale.y;
            }
            else if (type == 1) // vertical
            {
                scalingFactor = transform.parent.localScale.x / initialParentScale.x;
            }
            tempScale.x /= scalingFactor;
            transform.localScale = tempScale;
            sizeAdjusted = true;
        }
    }

    void storeCurrentScale()
    {
        initialParentScale = transform.parent.localScale;
    }
}
