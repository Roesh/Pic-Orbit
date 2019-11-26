using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// ImageLoad is a script attached to the image frame prefab. It has functions 
/// that adjust the scale of its child frame and sprite objects so that a maximum
/// width and height are preserved.
/// </summary>
public class ImageSizeAdjuster__Sprite : MonoBehaviour {

    public GameObject SpriteObj;    // Holds the Gameobject containing the sprite element
    public GameObject FrameObj;     // Holds the Gameobject that is the parent of the Frame solid objects  

    // IF THE MAX VALUES ARE CHANGED, THEN THE FRAME OBJECTS MUST BE REPOSITIONED.
    // POSSIBLE TODO: CREATE A SCRIPT THAT AUTOMATES THE CHANGE IN FRAMEOBJECT POSITIONS
    private float maxWidth = 600;   // Maximum width of an image (in pixels) 
    private float maxHeight = 600;

    public class scaling
    {
        public float scale;
        public Vector3 frameScale;
        public Vector2 origSize;
        public bool widthCorrected = false;
        public bool heightCorrected = false;
        public bool onlyFrameIsScaled = false;

        public scaling(Vector3 initFrameScale)
        {
            scale = 1;
            origSize = new Vector2(0, 0);
            frameScale = initFrameScale; 
        }

        public void reportVals()
        {
            Debug.Log("Sprite scale: " + scale + " ||Frame Scale: " + frameScale + " ||Original size: " + origSize+ 
                "Only frame was scaled? " + onlyFrameIsScaled+ ". Was width corrected? " + heightCorrected + 
                ". Was height corrected? " + heightCorrected);
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
        //Adjust for variation in max width and heithg
        FrameObj.transform.localScale = new Vector3(maxWidth / 420, maxHeight / 420, FrameObj.transform.localScale.z);

        scaling sc = getscalingFromTexture(tw, th);
        SpriteObj.GetComponent<SpriteRenderer>().sprite = sprite;               // Set the sprite object of the sprite renderer        
        SpriteObj.transform.localScale = new Vector3(sc.scale, sc.scale, 1);    // Rescale the sprite to fit max size
        FrameObj.transform.localScale = sc.frameScale;                          // Adjust the bounding frame of the image sprite  
        RectTransform rt = SpriteObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(4.2f/(sc.frameScale.x*sc.scale), 4.2f/ (sc.frameScale.y* sc.scale));
        return sc;
    }    

    // Get the sacaling values based on the textures width and height
    scaling getscalingFromTexture(int mtW,int mtH)
    {
        scaling sc = new scaling(FrameObj.transform.localScale);        
        float s=1;   //scale to scale the sprite object by
        sc.origSize = new Vector2(mtW, mtH);
        // If height AND width are less than the max values, then ONLY rescale
        // the frame
        if (mtH < maxHeight && mtW < maxWidth)
        {
            sc.frameScale.x = (mtW / maxWidth) * sc.frameScale.x;
            sc.frameScale.y = (mtH / maxHeight) * sc.frameScale.y;
            sc.onlyFrameIsScaled = true;
            return sc;
        }
        // If width is greater than max width, Rescale image based of width fraction
        if (mtW > maxWidth)
        {
            s = maxWidth/mtW;
            // Perform scaling for the frame bounding inverse to the image sprite scaling
            sc.frameScale.x = (1/s)* sc.frameScale.x;       
            sc.frameScale.y = (mtH / maxHeight) * sc.frameScale.y;
            sc.widthCorrected = true;
        }
        // If, after the previous transform (or if no transform was made), 
        // the height is still greater than max height, then rescale image
        // according to the height ratio btw image height and max height
        if (mtH*s > maxHeight)
        {
            s = maxHeight/(mtH);
            sc.heightCorrected = true;
        }
        sc.scale = s;
        return sc;
    }

    public void changeMaxSize(float mWidth, float mHeight)
    {
        maxWidth = mWidth;
        maxHeight = mHeight;
    }

}
