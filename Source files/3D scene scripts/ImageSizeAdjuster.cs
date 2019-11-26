using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// ImageLoad is a script attached to the image frame prefab. It has functions 
/// that adjust the scale of its child frame and sprite objects so that a maximum
/// width and height are preserved.
/// </summary>
public class ImageSizeAdjuster : MonoBehaviour {

    private RectTransform rt;
    private float maxWidth = 600;   // Maximum width of an image (in pixels) 
    private float maxHeight = 600;

    public class scaling
    {
        public Vector3 scale;
        public Vector2 origSize;
        public float scaleDownFactor;
        public bool widthMaxxed = false;
        public bool heightMaxxed = false;
        public bool frameIsScaledDown = false;

        public scaling(Vector3 initscale)
        {            
            origSize = new Vector2(0, 0);
            scale = initscale;             
        }

        public void reportVals()
        {
            Debug.Log("Scaling: " + scale + "||Original size: " + origSize+ 
                "Only frame was scaled? " + frameIsScaledDown + ". Was width only maxxed out? " + widthMaxxed+ 
                ". Was height (and possibly width) maxxed out? " + heightMaxxed);
        }
    }
    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}  

    // This function is the setup function that opens the image and loads the sprite with it
    public scaling initiateSprite(Sprite sprite,int tw, int th)
    {
        //Adjust for variation in initial max width and heitght w.r.t 420 reference
        rt = GetComponent<RectTransform>();
        rt.localScale = new Vector3(maxWidth / 420, maxHeight / 420, transform.localScale.z);
        int i = 0;
        GetComponent<Image>().sprite = sprite;                                                      // Attach the sprite to the image UI        
        
        // Obtain the scaling needed from the texture
        
        scaling sc = getscalingFromTexture(tw, th);
        // Scale the image frame
        foreach (maintainFrameSize mfsScript in FindObjectsOfType<maintainFrameSize>())
        {
            mfsScript.setSize();
        }
        transform.localScale = sc.scale;
        foreach (maintainFrameSize mfsScript in FindObjectsOfType<maintainFrameSize>())
        {
            mfsScript.restoreSize();
        }
        return sc;
        //return new scaling(rt.localScale);
    }    

    // Get the sacaling values based on the textures width and height
    scaling getscalingFromTexture(int mtW,int mtH)
    {
        // The texture will be mapped to an area that is
        // x: maxWidth units
        // y: maxHeight units
        scaling sc = new scaling(rt.localScale);         // Initialize a new scaling object using the current scaling
        float s=1;   //scale to scale the sprite object by
        sc.origSize = new Vector2(mtW, mtH);

        // If height AND width are less than the max values, then ONLY rescale the frame
        if (mtH < maxHeight && mtW < maxWidth)
        {
            sc.scale.x = (mtW / maxWidth) * sc.scale.x;
            sc.scale.y = (mtH / maxHeight) * sc.scale.y;
            sc.frameIsScaledDown = true;
            sc.scaleDownFactor = 1;     // We're scaling down to the size of the frame, therefore the image itself wasn't scaled
            return sc;
        }
        // If height OR width OR both are greater than max values
        else
        {
            // If width is proportionally greaterer than max width, Rescale height
            if (mtW/maxWidth > mtH/maxHeight)
            {
                sc.scaleDownFactor = maxWidth / mtW;     //Scaled TO maxWidth from mtW (actual pixels width)
                sc.scale.y = (mtH / maxHeight) * sc.scaleDownFactor * sc.scale.y;
                sc.widthMaxxed = true;
            }            
            else // Rescale width
            {
                sc.scaleDownFactor = maxHeight / mtH;     //Scaled TO maxHeight from mtW (actual pixels height)
                sc.scale.x = (mtW / maxWidth) * sc.scaleDownFactor * sc.scale.x;
                sc.heightMaxxed = true;
            }
        }
        return sc;
    }

    public void changeMaxSize(float mWidth, float mHeight)
    {
        maxWidth = mWidth;
        maxHeight = mHeight;
    }

}
