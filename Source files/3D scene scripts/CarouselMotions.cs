using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarouselMotions : MonoBehaviour {

    private Rigidbody carouselRB;
    private Vector3 addedTorque;
    private float maxDtorque;
    private float targetAngVelocity;
    private float correctiveTorque;

    private IEnumerator targetVelocityRamp(float initVel,float finalVel, float time)
    {
        float t_e = 0f; //time elapsed
        while (t_e < time)
        {
            targetAngVelocity = Mathf.Lerp(initVel, finalVel, t_e / time);
            t_e += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator correctiveTorqueRamp(float inittq, float finaltq, float time)
    {
        float t_e = 0f; //time elapsed
        while (t_e < time)
        {
            correctiveTorque = Mathf.Lerp(inittq, finaltq, t_e / time);
            t_e += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator setConstTorque(float dtorque)
    {
        addedTorque = Vector3.up*dtorque;
        while (true)
        {
            carouselRB.AddTorque(addedTorque);
            yield return null;
        }
    }
    private IEnumerator matchAngVelFeedback()
    {     
        while (true) {
            // Get the angular velocity in y direction. Calculate difference between target velocity and actual velocity
            float actualVelocity = carouselRB.angularVelocity.y;
            float velocityDiff = (targetAngVelocity - actualVelocity);
            // Exit the coroutine if we find that the difference between velocities is miniscule
            if (Mathf.Abs(velocityDiff) < 0.001f)
            {
                yield return new WaitForSeconds(0.1f);
            }
            else // Add corrective torque
            {
                float multVal = correctiveTorque * velocityDiff;
                addedTorque = Vector3.up * Mathf.Clamp(multVal,-maxDtorque,maxDtorque);
                carouselRB.AddTorque(addedTorque);
                yield return null;
            }
        }
    }
    private IEnumerator reportStats()
    {
        while (true)
        {
            Debug.Log("Ang. Velocity: " + carouselRB.angularVelocity.y+ "|Target Vel: "+targetAngVelocity+"|Added torque: " + addedTorque.y);            
            yield return new WaitForSeconds(.1f);
        }
    }

	// Use this for initialization
	void Start () {
        carouselRB = GetComponent<Rigidbody>(); // Get the rigid body component  
        // Initialize variables
        targetAngVelocity = 0;
        correctiveTorque = 0.000001f;
        maxDtorque = 0.001f;
        // Turn on desired velocity ramp and velocity matching torque feedback
        //carouselFeedbackTorque();
        carouselVelocityRamp(0f, 0.5f, 1f, 10f, 3);        
        // Turn on velocity reporting
        StartCoroutine(reportStats());

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // Public access function to coroutine velocity ramp. Use this in conjunction with 
    // Feedback velocity tracking to perform smooth velocity ramping
    // The corrective torque magnitude also scales as needed to prevent high frequency oscillations
    // near 0
    public void carouselVelocityRamp(float initVel, float finalVel, float initTq, float finalTq, float time)
    {        
        StartCoroutine(targetVelocityRamp(initVel, finalVel, time));
        StartCoroutine(correctiveTorqueRamp(initTq, finalTq, time));
    }
    // Public access function to coroutine constant torque
    public void carouselTorqueConst(float torque)
    {
        StartCoroutine(setConstTorque(torque));
    }
    // Public access function to coroutine feedback Torque
    public void carouselFeedbackTorque()
    {
        StartCoroutine(matchAngVelFeedback());
    }
}
