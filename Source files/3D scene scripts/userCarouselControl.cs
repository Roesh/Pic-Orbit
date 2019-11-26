using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class userCarouselControl : MonoBehaviour {

    public float d_velocity;
    CarouselKinematics ck;

    // Use this for initialization
    void Start () {
        ck = GetComponent<CarouselKinematics>();
        d_velocity = 10f;
    }
	
	// Update is called once per frame
	void Update () {
        /*if (ic.carouselControlsOn)  // Make sure user is allowed to control carousel
        {
            //TODO: make the velocity increase/ decrease gradual
            if (Input.GetKeyDown(ic.speedUpBtn))    // step up velocity
            {
                if (!ck.getIsVelocityRamping())  // As long as the velocity is not being ramped
                {
                    ck.addRampVel(d_velocity, 0.5f);
                }
                ck.reportCarouselVars();
            }
            if (Input.GetKeyDown(ic.slowDownBtn))
            {
                if (!ck.getIsVelocityRamping()) // As long as the velocity is not being ramped
                {
                    ck.addRampVel(-d_velocity, 0.5f);
                }
                ck.reportCarouselVars();
            }
        }*/
    }
}
