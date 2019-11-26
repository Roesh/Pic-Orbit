using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileScripts : MonoBehaviour {

#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_IPHONE
    private GUIscripts guiScpts;
    private CarouselKinematics carouselKinematics;
    private CameraControls camControls;

    // Use this for initialization
    void Start () {
        guiScpts = FindObjectOfType<GUIscripts>();
        carouselKinematics = FindObjectOfType<CarouselKinematics>();
        camControls = FindObjectOfType<CameraControls>();
	}
	
	// Update is called once per frame
	void Update () {
		 //Check if Input has registered more than zero touches
        if (Input.touchCount == 1)
        {
            //Store the first touch detected.
            Touch myTouch = Input.touches[0];
                
            //Check if the phase of that touch equals Began
            if (myTouch.phase == TouchPhase.Moved)
            {                
                float rSpeed = carouselKinematics.getRspeed();
                float rSpeedAdd = myTouch.deltaPosition.x / 7;
                carouselKinematics.setRSpeed(rSpeed-rSpeedAdd);
                float fScale = myTouch.deltaPosition.y / 22.5f;
                camControls.addCamForce(-fScale);
            }                
        }            
    }
#endif
}
