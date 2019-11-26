using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Swaps parents so that show order is preserved.
/// Prevents images in back from displaying over front ones
/// 
/// ASSUMES: Camera is on Z axis, with a negative offset, and is facing
/// down the z axis
/// </summary>
public class imgParentSwap : MonoBehaviour {

    int parentID; // -2 back parent 2, -1: back parent 1, 1: front parent 1, 2: front parent 2
    Transform rotationCenterTx;

    GameObject frontParent1;
    GameObject frontParent2;
    GameObject backParent1;
    GameObject backParent2;

    // Use this for initialization
    void Start () {
        rotationCenterTx = GameObject.Find("CarouselVert").GetComponent<Transform>();
        frontParent1 = GameObject.Find("frontParent1");
        frontParent2 = GameObject.Find("frontParent2");
        backParent1 = GameObject.Find("backParent1");
        backParent2 = GameObject.Find("backParent2");
        transform.SetParent(frontParent2.transform);
        parentID = 2;
        Update();         
    }
	
	// Update is called once per frame
	void Update () {
        //Get the valud of the difference between rotation center and current pos
        float zDiff = transform.position.z - rotationCenterTx.position.z;
        // default "option" of parent is 2, the one most forward
        int option = 2; 
        // go through this sequence to determine which bucket the image currently falls under
        if (zDiff > -10)
        {
            option = 2;
        }if (zDiff > -5.5)
        {
            option = 1;
        }if (zDiff > 0)
        {
            option = -1;
        }if (zDiff > 5.5)
        {
            option = -2;
        }
        // If the selected option matches the parent ID of the image, do nothing and return
        if(option == parentID)
        {
            return;
        }
        // Otherwise, set the parent appropriately and update the parent ID
        parentID = option;
        switch (parentID)
        {
            case -2:
                {
                    transform.SetParent(backParent2.transform);
                }
                break;
            case -1:
                {
                    transform.SetParent(backParent1.transform);
                }
                break;
            case 1:
                {
                    transform.SetParent(frontParent1.transform);
                }
                break;
            case 2:
                {
                    transform.SetParent(frontParent2.transform);
                }
                break;
        }        
    }
    /*
    void OnCollisionStay(Collision collision)
    {
        Debug.Log("Collision occured");
        GameObject collisionObj = collision.gameObject;
        if (collisionObj.name == "FrontEnd")
        {
            collisionObj.transform.parent = collisionObj.transform;
            Debug.Log("Front end hit");
        }
        if (collisionObj.name == "BackEnd")
        {
            collisionObj.transform.parent = collisionObj.transform;
            Debug.Log("Front end hit");
        }
    }*/
}
