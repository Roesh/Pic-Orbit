using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarouselKinematics : MonoBehaviour {

    private float rotSpeed;     // Holds the rate of rotation of the carousel at any point in time
    public float initSpeed;     // Holds the initial rotation speed
    public float finalSpeed;    // Holds the final rotation speed

    private bool ramping;           // Whether the carousel is changing speed right now
    private IEnumerator curVelRamp; // Ienumerator that holds the velocity change process
    private Transform carTx;        // The transform component of the carousel gameobject

    public float maxRotSpeed;
    public float rotSpeedRampRate;
    private inputControls ic;

    /// <summary>
    /// Constantly rotate about the y axis at rotSpeed speed
    /// </summary>
    /// <returns></returns>
    private IEnumerator constRotate()
    {
        Vector3 pos = carTx.position;
        while (true)
        {
            carTx.RotateAround(pos,Vector3.up,Time.deltaTime*rotSpeed);
            yield return null;
        }
    }
    private IEnumerator RampRotSpeed(float initrotSpeed, float finalRotSpeed, float t)
    {
        ramping = true;
        float te = 0;                   // Elapsed time
        while (te < t)
        {
            te += Time.deltaTime;       // Add elapsed time
            rotSpeed = Mathf.Lerp(initrotSpeed, finalRotSpeed, te / t);
            yield return null;
        }
        ramping = false;
    }

    // Use this for initialization
    void Start () {
        carTx = gameObject.transform;
        rotSpeed = 0;        
        initSpeed = 0;
        finalSpeed = 10;
        maxRotSpeed = 40;
        rotSpeedRampRate = 10;
        ic = FindObjectOfType<inputControls>();

        //initiateInitialRotation(); ImageManager will start carousel oscillation after loading images
	}
	
    public void initiateInitialRotation()
    {
        StartCoroutine("constRotate");
        curVelRamp = RampRotSpeed(initSpeed, finalSpeed, 2);
        StartCoroutine(curVelRamp);
    }

	// Update is called once per frame
	void Update () {
        if (!ramping)
        {
            if (Input.GetKey(ic.speedUpBtn))    // step up velocity
            {
               if((rotSpeed += rotSpeedRampRate * Time.deltaTime) > maxRotSpeed)
                {
                    rotSpeed = maxRotSpeed;
                }
            }
            if (Input.GetKeyDown(ic.speedZeroBtn))    // step up velocity
            {
                StartCoroutine(RampRotSpeed(rotSpeed, 0, 0.2f));
            }
            if (Input.GetKey(ic.slowDownBtn))
            {
                if ((rotSpeed -= rotSpeedRampRate * Time.deltaTime) < -maxRotSpeed)
                {
                    rotSpeed = -maxRotSpeed;
                }
            }
        }
	}

    public void addRampVel(float d_velocity,float dt)
    {
        // Stop the previous coroutine if we're ramping
        if (ramping)
        {
            StopCoroutine(curVelRamp);
        }
        initSpeed = rotSpeed;
        finalSpeed = Mathf.Clamp(rotSpeed + d_velocity, -maxRotSpeed, maxRotSpeed);
        if (!rotSpeed.Equals(finalSpeed))
        {
            curVelRamp = RampRotSpeed(initSpeed, finalSpeed, dt);
            StartCoroutine(curVelRamp);
        }        
    }

    public bool getIsVelocityRamping()
    {
        return ramping;
    }

    public void reportCarouselVars()
    {
        Debug.Log("current velocity: " + rotSpeed + " ||Approaching velocity: " + finalSpeed+ " ||Ramping? "+ramping);
    }

    public float getRspeed()
    {
        return rotSpeed;
    }
    public void setRSpeed(float speed)
    {
        if (!ramping)
        {
            rotSpeed = Mathf.Clamp(speed, -maxRotSpeed, maxRotSpeed);
        }
    }
}
