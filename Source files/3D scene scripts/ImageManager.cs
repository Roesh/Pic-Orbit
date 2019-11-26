using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI; 

/// ImageManager summary
/// This script is responsible managing the images in the scene 
/// When the scene opens, it uses imageLoader script to query the specified image directory 
/// for files, then asks the imageLoader to load all the images in that directory 
/// as 2dTextures. It then loads ImageFrame prefab Objects, and passes in values to
/// initialize their position based on an algorithm (to be developed). It also passes in the 
/// 2DTexture that that ImageFrame prefab should showcase as a sprite object

/// If the folderholder gameobject is present (which is the likely scenario if we are using the Folder 
/// Selection scene that Pic Orbit versions ship with), we use the images from the list of file paths
/// specified in the folderholder's FolderSelectionManager script as opposed to using the images in 
/// the ImageFolder
/// 
/// This version is meant to be used with clickable images and references the Image resizer
/// and Image template as opposed to the sprite resizer and sprite template
/// ImageManager summary
/// 
/// The latest edit tries to modify the loading sequence so that image loading occurs within a coroutine. SCripts for 
/// Camera control and Carousel animation are edited so that this script initiates them once all the images are loaded
public class ImageManager : MonoBehaviour {

    private FileInfo[] fileInfos;
    // numImages is accurate to how many images have been loaded ONLY after the call to incrementThetaAndTDiff is performed
    private int numImages;              
    private int maxImages;
    private FileLoader imgLoader;
    public GameObject carouselVertGo;
    public bool unleashed;
    private int loadType; // 0 - Spiral, 1 - cylinder
    private float yBase;
    private float yDiff;
    private float thetaDiff;
    // Constants for image distances
    private const float imageSetDiff = 80;
    private int numRowsPopulated;
    // Variable method holds which location are images stored in
    // 0-Application.DataPath, 1-Application.persistentDatapath,
    // 2-roesh website, 3-google drive folder
    public int method;
    private GameObject imgFrameObject;
    private GameObject arenaUnitObject;
    public List<imageObject> imageObjects = new List<imageObject>();
    private GUIscripts guiscpt;
    private inputControls ic;
    private CameraControls cControls;

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
        public GameObject imgFrameObj; // Gameobject that stores the gameobject of this image
        private ImageSizeAdjuster.scaling originalScale; 
        private float yDiff;
        private float thetaDiff;
        private string fileName;        // File name
        private GameObject carousel;
        private int index;              // unique index of the image, starts from 0

        public imageObject(string name, string fPath, Sprite sp, Texture2D tex, float ydiff, 
            float thetadiff, GameObject imgFrameObject, GameObject Carousel, int ind)
        {
            // Set class variables
            imgName = name;
            index = ind;
            fullPath = fPath;
            imgTexture = tex;
            sprite = sp;
            yDiff = ydiff;
            thetaDiff = thetadiff;
            carousel = Carousel;
            // Load a prefab img frame gameobject onto the scene
            loadNewImgFramePrefab(imgFrameObject);                    
            // Initialize the cloned prefab with an image. Frame resizing is taken care of by a script on the prefab
            originalScale = imgFrameObj.GetComponent<ImageSizeAdjuster>().initiateSprite(sp,tex.width,tex.height);
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
            // Making the camera look at the newly added image once every 8 images, and if we're still
            // low on image count (a high image count does not show much)
            if (index < 32)
            {/*
                if (index < 4 && index % 4==0){ // Do nothing
                }
                else if (index < 8 && index % 6 == 0)
                {
                    GameObject.Find("CamRB").transform.LookAt(imgFrameObj.transform);                    
                }
                else if (index < 16 && index % 7 == 0)
                {
                    GameObject.Find("CamRB").transform.LookAt(imgFrameObj.transform);
                }*/
                if (index % 8 == 0)
                {
                    GameObject.Find("CamRB").transform.LookAt(imgFrameObj.transform);
                }
            }
            attachToManager();
        }

        // Function to Attach this image object to the carousel
        void attachToCarousel()
        {
            //imgFrameObj.transform.parent = carousel.transform;
            imgFrameObj.transform.SetParent(carousel.transform);
        }

        // Attach the button to a function using a listener and delegate 
        void attachToManager()
        {
            imgFrameObj.GetComponent<Button>().onClick.AddListener(
                delegate {
                    GameObject.Find("Manager").GetComponent<GUIscripts>().
                    imgClickHandler(imgName,sprite,imgTexture.width,imgTexture.height,index);
                });
        }
        public void openImageInCloseUp()
        {
            GameObject.Find("Manager").GetComponent<GUIscripts>().
                    imgClickHandler(imgName, sprite, imgTexture.width, imgTexture.height, index);
        }
        public void genReport()
        {
            Debug.Log("Name: " + imgName + " ||Vertical offset: " + yDiff + " ||Angle offset: " + thetaDiff);
            originalScale.reportVals();
        }
    }
    

    // Use this for initialization
    void Start () {
        // Initialize y base, y diff and thetaDiff to 0        
        yBase = 0;
        yDiff = 0;
        thetaDiff = 0;
        numRowsPopulated = 0;
        method = 0;     // Default Search method is using Application.DataPath
#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_IPHONE
        method = 1;     // Search method is using Application.persistentDataPath if on mobile
#endif
        numImages = 0;
        maxImages = 32;
        // Default display type is cylinder. This is overridden by the default in FolderSelectionManager
        loadType = 1;   // 0 - Spiral, 1 - cylinder
        // Get the base prefab gameobject from the resources folder
        imgFrameObject = Resources.Load<GameObject>("Prefabs/ImageSpriteTemplate");
        arenaUnitObject = Resources.Load<GameObject>("Prefabs/BaseArenaUnit");
        imgLoader = FindObjectOfType<FileLoader>();
        guiscpt = FindObjectOfType<GUIscripts>();
        ic = FindObjectOfType<inputControls>();
        cControls = FindObjectOfType<CameraControls>();

        // Check if we have a folderHolder. If yes, use the file information that it provides
        // to load images
        GameObject folderHolder;
        if ((folderHolder = GameObject.Find("folderHolder")) != null)
        {
            FolderSelectionManager fm = folderHolder.GetComponent<FolderSelectionManager>();
            fileInfos = fm.selectedFiles;
            loadType = fm.loadType;
            unleashed = fm.isUnleashed;
            fm.farewell();
        }
        else
        {
            // Otherwise use the imageloader script to get files
            fileInfos = imgLoader.getDirInfo(method, "ImageFolder");    // Get the image directory info, using method            
        }
        // Initial code calls placeImageFrames and cControls adjustement in sequence
        /*placeImageFrames();
        cControls.adjustOsc(8, numRowsPopulated);*/
        // New code calls the same functions, along with calls to other scripts to initiate them once done, in a coroutine
        StartCoroutine(imagePlacementRoutine());
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Tab) && ic.closeUpControlsOn)
        {
            int index = 0;
            if (Input.GetKey(KeyCode.LeftShift))
            {   // If shift tabbing, move backward
                index = guiscpt.prevImgCloseUp(numImages);
            }
            else
            {   // Move forward
                index = guiscpt.nextImgCloseUp(numImages);
            }
            imageObjects[index].openImageInCloseUp();
        }
	}

    
    void placeImageFrames(){ // Removed switch statement since only method 0, regular directory path is supported
        foreach (FileInfo f in fileInfos)
        {
            string fPath = f.FullName;
            string fExtn = f.Extension;

            // if the file has a valid image extension of png or jpg or jpeg, and is not in the ignored folder 
            // then create a new image in the scene 
            if (isValidFileExtn(fExtn) && !fPath.Contains("~Ignored"))
            {
                Texture2D imgtex = FileLoader.LoadImg(fPath);                        
                Sprite sp = Sprite.Create(imgtex, new Rect(0, 0, imgtex.width, imgtex.height), new Vector2(0.5f, 0.5f));                        
                if (sp != null) // make sure a sprite has been created
                {   
                    // New feature for unleashed mode 4/27/2019. Ignore max images, use it as max number of images in set instead.
                    // When 
                    // Add a new "arena" comprising the cylinder and pillars on top of the current one, move the ceiling up to reach
                    // the newly added arena height
                    // 
                    // yDiff variable now accounts for this added height via the yBase variable
                    // yBase is incremented with every arena addition 
                    if (unleashed)
                    {                                
                        string dispName = f.Name.Split('.')[0];
                        imageObjects.Add(new imageObject(dispName, fPath, sp, imgtex, yDiff, thetaDiff, imgFrameObject, carouselVertGo, numImages));
                        incrementThetaAndTDiff();
                        if (numImages % maxImages == 1 && numImages != 1)
                        {
                            yBase += imageSetDiff;
                            GameObject.Find("Ceiling").transform.Translate(Vector3.down * imageSetDiff);
                            GameObject newArenaObj = Instantiate(arenaUnitObject, GameObject.Find("Arena").transform);
                            newArenaObj.transform.Translate(Vector3.up * imageSetDiff * (numImages - 1) / maxImages);
                        }
                    }
                    else  // Limited images mode
                    { 
                        // If we haven't reached the limit for num images in scene, keep adding images
                        if (numImages < maxImages)
                        {
                            string dispName = f.Name.Split('.')[0];
                            imageObjects.Add(new imageObject(dispName, fPath, sp, imgtex, yDiff, thetaDiff, imgFrameObject, carouselVertGo, numImages));
                            incrementThetaAndTDiff();
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }            
    }

    IEnumerator imagePlacementRoutine()
    {
        foreach (FileInfo f in fileInfos)
        {
            string fPath = f.FullName;
            string fExtn = f.Extension;

            // if the file has a valid image extension of png or jpg or jpeg, and is not in the ignored folder 
            // then create a new image in the scene 
            if (isValidFileExtn(fExtn) && !fPath.Contains("~Ignored"))
            {
                Texture2D imgtex = FileLoader.LoadImg(fPath);
                Sprite sp = Sprite.Create(imgtex, new Rect(0, 0, imgtex.width, imgtex.height), new Vector2(0.5f, 0.5f));
                if (sp != null) // make sure a sprite has been created
                {
                    // New feature for unleashed mode 4/27/2019. Ignore max images, use it as max number of images in set instead.
                    // When 
                    // Add a new "arena" comprising the cylinder and pillars on top of the current one, move the ceiling up to reach
                    // the newly added arena height
                    // 
                    // yDiff variable now accounts for this added height via the yBase variable
                    // yBase is incremented with every arena addition 
                    if (unleashed)
                    {
                        string dispName = f.Name.Split('.')[0];
                        imageObjects.Add(new imageObject(dispName, fPath, sp, imgtex, yDiff, thetaDiff, imgFrameObject, carouselVertGo, numImages));
                        incrementThetaAndTDiff();
                        if (numImages % maxImages == 1 && numImages != 1)
                        {
                            yBase += imageSetDiff;
                            GameObject.Find("Ceiling").transform.Translate(Vector3.down * imageSetDiff);
                            GameObject newArenaObj = Instantiate(arenaUnitObject, GameObject.Find("Arena").transform);
                            newArenaObj.transform.Translate(Vector3.up * imageSetDiff * (numImages - 1) / maxImages);
                        }
                    }
                    else  // Limited images mode
                    {
                        // If we haven't reached the limit for num images in scene, keep adding images
                        if (numImages < maxImages)
                        {
                            string dispName = f.Name.Split('.')[0];
                            imageObjects.Add(new imageObject(dispName, fPath, sp, imgtex, yDiff, thetaDiff, imgFrameObject, carouselVertGo, numImages));
                            incrementThetaAndTDiff();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            yield return null;
        }
        // wait 1 seconds after the last image loads
        yield return new WaitForSeconds(1f);
        GameObject.Find("CamRB").transform.eulerAngles = Vector3.zero; // reset the euler angles
        FindObjectOfType<MusicManager>().startInitialMusic();          // Start music manager
        cControls.adjustOsc(8, numRowsPopulated);                      // Start camera oscillator after adjusting osc speed
        FindObjectOfType<CameraControls>().startOsc();                 // 
        FindObjectOfType<CarouselKinematics>().initiateInitialRotation(); // Begin rotating the carousel       
    }
    
    void incrementThetaAndTDiff()
    {
        // Add to num rows populated before incrementing because there
        // may not be another image added after the increment
        int tempNum = (int)yDiff / 10 + 1;
        if (tempNum > numRowsPopulated)
        {
            numRowsPopulated = tempNum;
        }
        switch (loadType) {
            case 0:
                yDiff += 10;        // difference of 10 units vertically between 2 images
                thetaDiff += 60;    // Difference of 60 degrees between 2 images stacked
                numImages++;        // incremenet the number of images in the scene
                if (yDiff - yBase == 80)    // reset the difference values if we have stacked 8 images
                {
                    yDiff = yBase;
                    thetaDiff = 90 * (numImages%maxImages) / 8; // Add an offset to the first image
                }
                break;
            case 1:
                thetaDiff += 90;    // Difference of 90 degrees between 4 images on plane 1
                numImages++;        // increment the number of images in the scene
                if (numImages%4==0)    // reset the difference values if we have distributed 4 images
                {
                    yDiff += 10;
                    thetaDiff += 30;
                }
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
                imageObjects.Add(new imageObject(name, fPath, sp, imgtex, yDiff, thetaDiff, imgFrameObject, carouselVertGo,numImages));
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

    private IEnumerator imageRotations(int val)
    {
        Random.InitState(val);
        yield return new WaitForSeconds(2);
        float tw = 1; // time to delay after current spin
        float po2 = Mathf.PI / 2;
        int numRots = 2;
        int rIndex;
        int prevIndex = 0;
        int numImages = imageObjects.Capacity-1;
        Debug.Log(numImages);
        while (true)
        {
            // time variables
            float t_elapsed = 0;
            float t = 1.8f + Random.value * 0.25f;
            // andgle variables
            int r = Random.Range(0, numRots - 1);
            float diff = Mathf.Lerp(360, numRots * 360, r / (numRots - 1));
            // See if the large spin should occur
            int decider;
            if (Random.value < 0.1)
            {
                decider = 0;
            }
            else
            {
                decider = 1;
            }
            if (Random.value > 0.5)
            {
                decider += 2;
            }
            switch (decider)
            {
            case 0: // multi rot
                while (t_elapsed < t)
                {
                    float angle = Mathf.Lerp(0, po2, t_elapsed / t);
                    float currAngle = diff * Mathf.Sin(angle);
                    foreach (imageObject img in imageObjects)
                    {
                        Vector3 initrot = img.imgFrameObj.transform.localEulerAngles;
                        img.imgFrameObj.transform.localEulerAngles = Vector3.right * currAngle + initrot;
                    }
                    t_elapsed += Time.deltaTime;
                    yield return null;
                }
                tw = Random.value * 2 + 4;
                break;
            case 1:
                rIndex = Random.Range(0, numImages);
                if (rIndex == prevIndex)
                {
                    rIndex = Random.Range(0, numImages);
                }
                while (t_elapsed < t)
                {
                    float angle = Mathf.Lerp(0, po2, t_elapsed / t);
                    float currAngle = diff * Mathf.Sin(angle);
                    Vector3 initrot = imageObjects[rIndex].imgFrameObj.transform.localEulerAngles;
                    imageObjects[rIndex].imgFrameObj.transform.localEulerAngles = Vector3.right * currAngle + initrot;
                    t_elapsed += Time.deltaTime;
                    yield return null;
                }
                tw = Random.value * 0.5f + 0.25f;
                prevIndex = rIndex;
                break;
            case 2:
                while (t_elapsed < t)
                {
                    float angle = Mathf.Lerp(0, po2, t_elapsed / t);
                    float currAngle = diff * Mathf.Sin(angle);
                    foreach (imageObject img in imageObjects)
                    {                        
                        img.imgFrameObj.transform.localEulerAngles = Vector3.down * currAngle;
                    }
                    t_elapsed += Time.deltaTime;
                    yield return null;
                }
                tw = Random.value * 4 + 3;
                break;
            case 3:
                rIndex = Random.Range(0, numImages);
                if (rIndex == prevIndex)
                {
                    rIndex = Random.Range(0, numImages);
                }
                while (t_elapsed < t)
                {
                    float angle = Mathf.Lerp(0, po2, t_elapsed / t);
                    float currAngle = diff * Mathf.Sin(angle);
                    Vector3 initrot = imageObjects[rIndex].imgFrameObj.transform.localEulerAngles;
                    imageObjects[rIndex].imgFrameObj.transform.localEulerAngles = Vector3.down * currAngle + initrot;
                    t_elapsed += Time.deltaTime;
                    yield return null;
                }
                tw = Random.value * 0.25f + 2f;
                prevIndex = rIndex;
                break;
            default:
                break;
            }
            yield return new WaitForSeconds(tw);
        }
    }
    private bool isValidFileExtn(string fExtn)
    {
        if (fExtn == ".png" || fExtn == ".jpg" || fExtn == ".jpeg")
        {
            return true;
        }
        if (fExtn == ".PNG" || fExtn == ".JPG" || fExtn == ".JPEG")
        {
            return true;
        }
        return false;
    }
}

