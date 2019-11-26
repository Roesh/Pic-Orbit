using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI; //~~~~~~!REHOUSE

/// ImageManager summary
/// This script is responsible managing the images in the scene 
/// When the scene opens, it uses imageLoader script to query the specified image directory 
/// for files, then asks the imageLoader to load all the images in that directory 
/// as 2dTextures. It then loads ImageFrame prefab Objects, and passes in values to
/// initialize their position based on an algorithm (to be developed). It also passes in the 
/// 2DTexture that that ImageFrame prefab should showcase as a sprite object
/// ImageManager summary
public class ImageManager__sprite : MonoBehaviour {

    private FileInfo[] fileInfos;
    private int numImages;              //!!! number starts from 0. ACTUAL NUMBER OF IMAGES IS 1+ NUMIMAGES. Same for max images
    private int maxImages;
    private FileLoader imgLoader;
    public GameObject carouselVertGo;
    private float yDiff;
    private float thetaDiff;
    // Variable method holds which location are images stored in
    // 0-Application.DataPath, 1-Application.persistentDatapath,
    // 2-roesh website, 3-google drive folder
    public int method;
    private GameObject imgFrameObject;
    public List<imageObject> imageObjects = new List<imageObject>();
    private GUIscripts guiscpt; 

    /// imageObject Summary
    /// This class is used to store a single image object's data, such as its
    /// display name, its file name, its 2Dtexture and the gameobjects that
    /// display it
    /// imageObject Summary
    public class imageObject
    {
        private string imgName;         // Image display name  
        private string fullPath;        // Full path to image
        private Texture2D imgTexture;
        private Sprite sprite;
        private GameObject imgFrameObj;  // Gameobject that stores the gameobject of this image
        private ImageSizeAdjuster__Sprite.scaling originalScale;
        private float yDiff;
        private float thetaDiff;
        private string fileName;    // File name
        private GameObject carousel;

        public imageObject(string name, string fPath, Sprite sp, Texture2D tex, float ydiff, float thetadiff, GameObject imgFrameObject, GameObject Carousel)
        {
            // Set class variables
            imgName = name;
            fullPath = fPath;
            imgTexture = tex;
            sprite = sp;
            yDiff = ydiff;
            thetaDiff = thetadiff;
            carousel = Carousel;
            // Load a prefab img frame gameobject onto the scene
            loadNewImgFramePrefab(imgFrameObject);                    
            // Initialize the cloned prefab with an image. Frame resizing is taken care of by a script on the prefab
            originalScale = imgFrameObj.GetComponent<ImageSizeAdjuster__Sprite>().initiateSprite(sp,tex.width,tex.height);
            //genReport();
        }

        // Instantiate a new object from the loaded image frame prefab
        // Offset the clone by the desired value
        private void loadNewImgFramePrefab(GameObject imgFrameObject)
        {
            imgFrameObj = Instantiate(imgFrameObject);
            attachToCarousel();
            imgFrameObj.transform.position = imgFrameObj.transform.position + Vector3.up * yDiff;
            imgFrameObj.transform.RotateAround(carousel.transform.position, Vector3.down, thetaDiff);
            //attachToManager();
        }

        // Function to Attach this image object to the carousel
        void attachToCarousel()
        {            
            imgFrameObj.transform.parent = carousel.transform;
        }

        // Attach the button to a function using a listener and delegate 
        /*void attachToManager()
        {
            imgFrameObj.GetComponent<Button>().onClick.AddListener(
                delegate {
                    GameObject.Find("Manager").GetComponent<GUIscripts>().
                    imgClickHandler(imgName,sprite,imgTexture.width,imgTexture.height);
                });
        }*/

        public void genReport()
        {
            Debug.Log("Name: " + imgName + " ||Vertical offset: " + yDiff + " ||Angle offset: " + thetaDiff);
            originalScale.reportVals();
        }
    }
    

    // Use this for initialization
    void Start () {
        // Initialize y diff to 0
        yDiff = 0;
        thetaDiff = 0;
        method = 0;     //Search method is using Application.DataPath
        numImages = 0;
        maxImages = 32;
        // Get the base prefab gameobject from the resources folder
        imgFrameObject = Resources.Load<GameObject>("Prefabs/ImageSpriteTemplate");
        imgLoader = FindObjectOfType<FileLoader>();
        guiscpt = FindObjectOfType<GUIscripts>();

        // Check if we have a folderHolder. If yes, use the file information that it provides
        // to load images
        GameObject folderHolder;
        if ((folderHolder = GameObject.Find("folderHolder")) != null)
        {
            FolderSelectionManager fm = folderHolder.GetComponent<FolderSelectionManager>();
            fileInfos = fm.selectedFiles;
            fm.farewell();
        }
        else
        {
            // Otherwise use the imageloader script to get files
            fileInfos = imgLoader.getDirInfo(method, "ImageFolder");    // Get the image directory info, using method            
        }
        placeImageFrames();
        //StartCoroutine(imgLoader.LoadImgWeb("http://roesh.000webhostapp.com/Default/1.jpg",this));
    }

    // Update is called once per frame
    void Update () {
		
	}

    
   void placeImageFrames()
    {
        switch (method)
        {
            case 0:
                foreach (FileInfo f in fileInfos)
                {
                    string fPath = f.FullName;
                    // if the file has an image extension of png or jpg or jpeg, and is not in the ignored folder 
                    // then create a new image in the scene 
                    if ((fPath.EndsWith(".png") || fPath.EndsWith(".jpg") || fPath.EndsWith(".jpeg")) && !fPath.Contains("~Ignored"))
                    {
                        Texture2D imgtex = FileLoader.LoadImg(fPath);                        
                        Sprite sp = Sprite.Create(imgtex, new Rect(0, 0, imgtex.width, imgtex.height), new Vector2(0.5f, 0.5f));                        
                        if (sp != null) // make sure a sprite has been created
                        {   // If we haven't reached the limit for num images in scene, keep adding images
                            if (numImages < maxImages)
                            {
                                string dispName = f.Name.Split('.')[0];
                                //Debug.Log(dispName);
                                imageObjects.Add(new imageObject(dispName, fPath, sp, imgtex, yDiff, thetaDiff, imgFrameObject, carouselVertGo));
                                yDiff += 10;        // difference of 10 units vertically between 2 images
                                thetaDiff += 60;    // Difference of 60 degrees between 2 images stacked
                                numImages++;        // incremenet the number of images in the scene
                                if (yDiff == 80)    // reset the difference values if we have stacked 8 images
                                {
                                    yDiff = 0;
                                    thetaDiff = 90*numImages/8; // Add an offset to the first image
                                }
                            }
                            else{
                                return;
                            }
                        }
                    }
                }
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
        }
        
    }

    // TODO:: lOADING SCREEN while image loads via coroutine
    public void receiveImgFromWeb(Texture2D imgtex, string name)
    {
        string fPath = "Web";
        Sprite sp = Sprite.Create(imgtex, new Rect(0, 0, imgtex.width, imgtex.height), new Vector2(0.5f, 0.5f));
        if (sp != null) // make sure a sprite has been created
        {   // If we haven't reached the limit for num images in scene, keep adding images
            if (numImages < maxImages)
            {                
                imageObjects.Add(new imageObject(name, fPath, sp, imgtex, yDiff, thetaDiff, imgFrameObject, carouselVertGo));
                imageObjects[0].genReport();
                yDiff += 10;        // difference of 10 units vertically between 2 images
                thetaDiff += 60;    // Difference of 60 degrees between 2 images stacked
                numImages++;        // incremenet the number of images in the scene
                if (yDiff == 80)    // reset the difference values if we have stacked 8 images
                {
                    yDiff = 0;
                    thetaDiff = 90 * numImages / 8; // Add an offset to the first image
                }
            }
            else
            {
                return;
            }

        }
    }
}

