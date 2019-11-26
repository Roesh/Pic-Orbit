using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour {

    private Rigidbody rb;
    private readonly Vector3 dForce = Vector3.up * 8f;
    private Vector3 dTorque;
    private Vector3 dTorqueX;
    public bool inCollision;

    private inputControls ic;
    private float initialFocalLength;
    public float maxFocalLength = 70f;
    public float minFocalLength = 30f;
    private float dFLength;
    private float tFLength;

    private Vector3 bottomLimit;
    [SerializeField] private Vector3 topLimit;
    private bool oscillating;
    private IEnumerator oscRoutine;
    private IEnumerator magModRoutine;
    [SerializeField] private float animTime;
    [SerializeField] private float minAnimTime;
    [SerializeField] private float maxAnimTime;
    [SerializeField] private float motionMagMod; // change the magnitude of motion after a Ctrl or Alt key press
    public float scaleChange;

    public float eulerX;

    private void Awake()
    {
        oscillating = false;
        animTime = 28f;
        minAnimTime = 7;
        maxAnimTime = 50;
        dTorque = new Vector3(0, 0.5f, 0);
        dTorqueX = new Vector3(0.5f, 0, 0);
        tFLength = 2;
        dFLength = (maxFocalLength - minFocalLength) / tFLength;
        motionMagMod = 1f;
        bottomLimit = new Vector3(transform.localPosition.x, 13f, transform.localPosition.z);
        topLimit = new Vector3(transform.localPosition.x, 82f, transform.localPosition.z);
    }

    // Use this for initialization
    void Start () { 
        rb = GetComponent<Rigidbody>();
        ic = FindObjectOfType<inputControls>();
        initialFocalLength = Camera.main.focalLength;
        magModRoutine = motionPauseRoutine();

        // Removed startosc from start as ImageManager will call this
    }
	
	// Update is called once per frame
	void Update () {
        int controlCheckSum = 0;                                // Used to determine in Ctrl/Alt or shift were held down;
        float vertInputValue = Input.GetAxis("Vertical");       // Get the vertical axis value for movement
        float horInputValue = Input.GetAxis("Horizontal");      // Get the horiztonal axis value for movement
        float scrollInputlValue = Input.GetAxis("Mouse ScrollWheel"); // Get the scrollwheels value for movement

        // If the player can currently control the image carousel, peform actions of move the rigid body
        // attached to the camera, adjust the camera's properties and move the carousel itself
        if (ic.carouselControlsOn)  
        {            
            if(Mathf.Abs(vertInputValue) > Mathf.Abs(scrollInputlValue))
            {
                // Replace the scroll wheel's value with the vert axis value
                // If the motion of the keyboard is larger, that is what should be respected
                scrollInputlValue = vertInputValue; 
            }
            // If L Shift was held down, Add a torque in the vertical axis
            if (Input.GetKey(KeyCode.LeftShift)) 
            {
                rb.AddRelativeTorque(dTorque * scrollInputlValue);                
                motionPauseForCamMove();
                controlCheckSum++;
            }
            // if L Alt was held down, Add a torque in the horizontal axis
            if (Input.GetKey(KeyCode.LeftAlt)) 
            {                
                rb.AddRelativeTorque(-dTorqueX * scrollInputlValue); // Note that scrollInputlValue = vertInputValue possibly                
                motionPauseForCamMove();
                controlCheckSum++;
            }
            // if L Ctrl was held down, Zoom in/out
            if (Input.GetKey(KeyCode.LeftControl))
            {
                Camera.main.focalLength += scrollInputlValue * dFLength * Time.deltaTime;
                Camera.main.focalLength = Mathf.Clamp(Camera.main.focalLength, minFocalLength, maxFocalLength);
                motionPauseForCamMove();
                controlCheckSum++;
            }
            if(controlCheckSum==0) // If none of the keys above were held down,
            {
                float sign = 1f;/* // Sign change snippet
                eulerX = Camera.main.transform.eulerAngles.x;
                if ( eulerX > 42.5f && eulerX < 225)
                {
                    sign = -1f;
                }*/
                rb.AddForce(dForce * scrollInputlValue * sign* motionMagMod);   // Add a force in the vertical axis           
            }
            rb.AddRelativeTorque(dTorque * horInputValue);    // Add a torque in the vertical axis -always respected
            // Restore angles coroutine
            if (Input.GetKeyDown(KeyCode.F5))
            {
                StartCoroutine(restoreAngles());
            }            
        }
        // We are able to adjust the animation time even if we arent in carousel contorl more
        else if (oscillating)
        {
            scaleChange = 1/animTime;
            if (Input.GetKey(KeyCode.PageUp))
            {
                animTime -= 0.8f*animTime*Time.deltaTime;                
            }
            if (Input.GetKey(KeyCode.PageDown))
            {
                animTime += 0.8f * animTime*Time.deltaTime;                
            }
            animTime = Mathf.Clamp(animTime, minAnimTime, maxAnimTime);
            scaleChange *= animTime;
        }
        // Disabling auto motion done via F6, or if the user moves the mousewheel or axes
        if (Input.GetKeyDown(KeyCode.F6))
        {
            if (!oscillating && ic.carouselControlsOn)
            {
                startOsc();
            }
            else
            {
                stopOsc();
            }
        }
        if (Mathf.Abs(vertInputValue)>0.2f || Mathf.Abs(horInputValue) > 0.2f || Mathf.Abs(scrollInputlValue) > 0.3f)
        {
            stopOsc();
        }
    } 
    private void motionPauseForCamMove() {
        if (motionMagMod == 1f)
        {
            StartCoroutine(magModRoutine = motionPauseRoutine());
        }
    }

    private IEnumerator restoreAngles()
    {
        //Remove player control
        ic.carouselControlsOn = false;
        rb.isKinematic = true;
        float t_e = 0f;     // elapsed time
        float t = 0.5f;    // time for animation
        Vector3 initAngles = gameObject.transform.localEulerAngles;
        float initFocalLength = Camera.main.focalLength;
        while (t_e < t)
        {
            t_e += Time.deltaTime;
            gameObject.transform.localEulerAngles = Vector3.Slerp(initAngles, Vector3.zero, t_e / t);
            Camera.main.focalLength = Mathf.Lerp(initialFocalLength,initialFocalLength,t_e/t);
            yield return null;
        }
        gameObject.transform.localEulerAngles = Vector3.zero;
        // return control to the player
        ic.carouselControlsOn = true;
        rb.isKinematic = false;
    }

    // Pic orbit oscillate 
    private IEnumerator oscillate2()
    {
        float t_e = 0f;     // elapsed time        
        Vector3 initAngles = gameObject.transform.localEulerAngles;
        float initFocalLength = Camera.main.focalLength;
        while (true)
        {            
            while (t_e < animTime)
            {
                t_e += Time.deltaTime;
                t_e *= scaleChange;
                gameObject.transform.localPosition = Vector3.Lerp(bottomLimit, topLimit, t_e / animTime);                  
                yield return null;
            }
            gameObject.transform.localPosition = topLimit;
            t_e = 0;
            yield return new WaitForSecondsRealtime(0.1f);
            while (t_e < animTime)
            {
                t_e += Time.deltaTime;
                t_e *= scaleChange;
                gameObject.transform.localPosition = Vector3.Lerp(topLimit, bottomLimit, t_e / animTime);
                yield return null;
            }
            gameObject.transform.localPosition = bottomLimit;            
            t_e = 0;
            yield return new WaitForSecondsRealtime(0.1f);
        }        
    }
    public void startOsc()
    {
        //Remove player control
        ic.carouselControlsOn = false;
        rb.isKinematic = true;
        oscillating = true;
        oscRoutine = oscillate2();
        StartCoroutine(oscRoutine);
    }
    private void stopOsc()
    {
        StopCoroutine(oscRoutine);
        // return control to the player
        oscillating = false;
        ic.carouselControlsOn = true;
        rb.isKinematic = false;
    }
    public void adjustOsc(int totalRows,int rowsPopulated)
    {
        float ratio = (float)rowsPopulated / totalRows;
        if(ratio == 1) {
            return;
        }
        minAnimTime *= ratio;
        maxAnimTime *= ratio;
        animTime *= ratio;
        if(ratio != 1)
        topLimit.y = topLimit.y*ratio + 6f;
        //topLimit = new Vector3(topLimit.x, topLimit.y * ratio, topLimit.z);
    }

    private IEnumerator motionPauseRoutine()
    {
        motionMagMod = 0;
        float initDrag = rb.drag;
        float finalDrag = (rb.drag += 10);
        float t_e = 0f;     // elapsed time 
        float t = 0.75f;
        while (t_e < t)
        {
            motionMagMod = Mathf.Lerp(0, 1, t_e / t);
            rb.drag = Mathf.Lerp(finalDrag, initDrag, t_e / t);
            t_e += Time.deltaTime;
            yield return null;
        }
        rb.drag = initDrag;
        motionMagMod = 1.0f;
    }
    void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.name == "Floor")
        {
            inCollision = true;
            rb.AddForce(dForce * 7);
        }

        if (col.gameObject.name == "Ceiling")
        {
            inCollision = true;
            rb.AddForce(-dForce * 7);
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.gameObject.name == "Floor")
        {
            inCollision = true;
            rb.AddForce(dForce * 3);
        }

        if (col.gameObject.name == "Ceiling")
        {
            inCollision = true;
            rb.AddForce(-dForce * 3);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        inCollision = false;
    }

    public void addCamForce(float scale)
    {
        rb.AddForce(dForce * scale);
    }
}
