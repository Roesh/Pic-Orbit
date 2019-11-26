using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Check ImageFolder for any sub-folders containing images, and display those
/// folders as GUI elements to the user. This is done by instantiating folder 
/// objects corresponding to subfolders containing at least one image. GUI displays 
/// the folder's name and the number of images in that folder. The ImageFolder iteself
/// appears as a GUI element if it contains any images in addition to sub-folders of 
/// images
/// 
/// We check the pathBasedLoader script for a valid custom path first. If a valid
/// custom path was defined there, we treat that custom directory as our ImageFolder 
/// directory instead
/// 
/// This script is attached to the folderholder gameobject and persists into the next scene
/// (Pic Orbit scene.) 
/// </summary>
public class FolderSelectionManager : MonoBehaviour {

    private GameObject folderTemplate;
    public List<folderObject> folderObjects = new List<folderObject>();
    private folderGUIBehavior GUIbehaviorScpt;
    public FileLoader directoryInquirer;
    private int numFolders;
    public FileInfo[] selectedFiles;
    public int loadType;
    public bool isUnleashed;
    public int[] intHolder;
    private GameObject invalidPathDebug;
    private string customPath;
    private string rootFolder;

    // For unleashed mode - activate by typing "unleash"
    private int currentLetterCount;
    private KeyCode[] unleashCode = { KeyCode.U, KeyCode.N, KeyCode.L, KeyCode.E, KeyCode.A, KeyCode.S, KeyCode.H };

    public class folderObject
    {
        private GameObject folderSelectable;
        private FolderObjectBehavior folderScript;
        public DirectoryInfo dInfo;
        public string folderName;        
        public int numImages;

        public folderObject(string name,int nImages,DirectoryInfo dirInfo, GameObject folderTemplate, int folderNumber)
        {
            dInfo = dirInfo;            
            folderSelectable = Instantiate(folderTemplate);                         // Instantiate a new folder object
            folderScript = folderSelectable.GetComponent<FolderObjectBehavior>();   // Get the folder behavior script
            folderScript.updateNumImages(numImages = nImages);                      // Update the number of images in the folder
            folderScript.updateFolderName(folderName = name);                       // Update the folder name
            folderSelectable.transform.parent = GameObject.Find("Canvas").transform;// Set the Canvas as the parent object
            folderSelectable.transform.localPosition= new Vector3(-285, 190, 0);    // Set the initial position
            updatePosition(folderNumber);            
        }
        public bool isFolderSelected()
        {
            return folderScript.toggleComp.isOn;
        }
        private void updatePosition(int folderNumber)
        {
            if(folderNumber == 1) { return; }
            // If we are on an even count of folders
            if (folderNumber % 2 == 0)   
            {
                Vector3 pos = folderSelectable.transform.localPosition;
                pos.x = -pos.x;
                folderSelectable.transform.localPosition = pos;
            }
            int numMoves = (folderNumber - 1) / 2;
            for (int i = 0; i < numMoves; i++) {
                folderScript.scrollMove(-1);
            }            
        }
    }

    // Use this for initialization
    void Start() {
        // Persist this object. Destroy it if you want to reload the scene
        DontDestroyOnLoad(this);
        isUnleashed = false;
        currentLetterCount = 0;
        numFolders = 0;
        GUIbehaviorScpt = FindObjectOfType<folderGUIBehavior>();

        int method = 0; // Method used to load images. Web/ local files
        char separator = Path.DirectorySeparatorChar; // Different separators for unix/ windows file paths
        string pathToRoot = null;  // the path to the root folder containing the images        
        
        invalidPathDebug = GameObject.Find("invalidPath");
        invalidPathDebug.SetActive(false);

        // Check if a different path to the images other than resources was specified
        customPath = pathToRoot = FindObjectOfType<pathBasedLoader>().path;
        if ((customPath != null) && (customPath != ""))
        {
            // If the path was invalid, set the custom path to null
            if(!Directory.Exists(pathToRoot))
            {
                customPath = FindObjectOfType<pathBasedLoader>().path = null; // Reset the path variable
                invalidPathDebug.SetActive(true);                // Activate the invalid path debug text                
            }
        }
        // If our custom path is null,
        if(customPath == null)
        {
            // Hard code Image folder location as default.
            pathToRoot = Application.dataPath + separator + "ImageFolder";
        }
        string[] items = pathToRoot.Split(separator);
        rootFolder = items[items.Length-1];
        // Load/ display type defaults to cylindrical load
        loadType = 1; // 0 - Spiral, 1 - cylinder
        folderTemplate = Resources.Load<GameObject>("Prefabs" + separator + "FolderObject");        // Get the folder template prefab from resources
        directoryInquirer = FindObjectOfType<FileLoader>();                         // Load a fileLoader script to help get directory info
        
        DirectoryInfo[] directoryInfos = directoryInquirer.GetDirectoryInfo(method, rootFolder, customPath);     // Get directory info for subfolders of ImageFolder
        
        DirectoryInfo rootFolderInfo = null;
        rootFolderInfo = new DirectoryInfo(pathToRoot);   // Get Directory info for rootFolder
        
        // Check if any of the queried directories contain any images.
        // If they do, add them to the scene (all happens in checkDirectoryAndAdd)
        checkDirectoryAndAdd(rootFolderInfo, true);
        foreach (DirectoryInfo dInfo in directoryInfos)
        {
            checkDirectoryAndAdd(dInfo);
        }
        // If we found no images, let the "No images" button stay
        if (numFolders == 0)
        {
            GUIbehaviorScpt.togglePathMenu(); //And turn on the path menu
            return;
        }
        // Otherwise
        GameObject.Find("noImages").SetActive(false);        
        // Update the gui to enable scrolling if we have more than 7 rows
        if (numFolders - 14> 0)
        {
            int maxoffset = Mathf.CeilToInt(((float)(numFolders - 14)) / 2);
            GUIbehaviorScpt.setMaxOffset(maxoffset);
        }
        //System.GC.Collect();
    }

    // Update is called once per frame
    void Update() {
        if (currentLetterCount < 7 && Input.GetKeyDown(unleashCode[currentLetterCount]))
        {
            currentLetterCount++;
            if(currentLetterCount == 7)
            {
                setUnleashed();
            }
        }
        if (!isUnleashed && ((Input.GetKey(KeyCode.LeftControl)|| Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.U)))
        {
            setUnleashed();
        }
	}

    void checkDirectoryAndAdd(DirectoryInfo dInfo,bool isRoot = false)
    {
        // Quit if we are looking at the ignored folder
        if(dInfo.FullName.Contains("~Ignored")){
            return;
        }
        // Initialize num images and folder name
        int numImages = 0;
        string folderName = dInfo.Name;
        SearchOption sOption;
        // If we are not in ImageFolder, use the (SearchOption)1 options
        if (!isRoot)
        {
            sOption = (SearchOption)1;  // All directories
        }
        else
        {
            sOption = (SearchOption)0; // Top directory only
        }
        // Loop through each file in this directory based on search option
        foreach (FileInfo finfo in dInfo.GetFiles("*.*", sOption))
        {
            string fExtn = finfo.Extension;            
            // Other location of image checking in this manner is pic orbit scene, with 
            // ImageManager script
            if (isValidFileExtn(fExtn)) // If an image is found,
            {
                numImages++; // increment num images
            }
        }
        // If at least one image was found, add this folder as a selectable folder object to the canvas
        if(numImages > 0)
        {
            numFolders++; // Increment number of folders
            folderObjects.Add(new folderObject(folderName, numImages, dInfo, folderTemplate,numFolders));            
        }        
    }

    /// <summary>
    /// Called when "done" button is pressed and user requests to open
    /// pic orbit scene. 
    /// Loads fileInfo data from all selected directories into fileinfos
    /// </summary>
    /// <returns>Returns false if no folders are selected to be used
    /// by the user. Returns true otherwise</returns>
    public bool collectSelectedImagesFromCheckedFolders()
    {
        int numSelectedFolders = 0;
        List<FileInfo> finfos = new List<FileInfo>();
        // Loop over each folder object
        foreach(folderObject fo in folderObjects)
        {
            // Check if the user marked it to be used in the pic orbit scene
            if (fo.isFolderSelected())
            {
                SearchOption sOption;
                // If we are searching the root folder, then dont look at nested folders for images
                if(fo.folderName == rootFolder)
                {
                    sOption = 0;
                }
                else
                {
                    sOption = (SearchOption)1;
                }
                // Loop over each file in the folder
                foreach (FileInfo fInfo in fo.dInfo.GetFiles("*.*", sOption))
                {
                    finfos.Add(fInfo); // Add that file's file info to the list
                }
                numSelectedFolders++;
            }
        }
        // If we didn't find any selected folders, quit and don't advance
        if (numSelectedFolders < 1)
        {
            return false;            
        }
        // Otherwise, transfer the file info into an array so that the next scene
        // can access it
        selectedFiles = new FileInfo[finfos.Count];
        for(int i = 0; i < finfos.Count; i++)
        {
            selectedFiles[i] = finfos[i];
        }
        return true;
    }
    public void setLoadType(int type)
    {
        loadType = type;        
    }
    public void farewell()
    {
        new GameObject("folderHolderWasHere");
        Destroy(GameObject.Find("folderHolder"));
        Destroy(GameObject.Find("pathHolder"));
    }
    private bool isValidFileExtn(string fExtn)
    {
            if (fExtn == ".png" || fExtn == ".jpg" || fExtn == ".jpeg")
            {
                return true;
            }
            if(fExtn == ".PNG" || fExtn == ".JPG" || fExtn == ".JPEG")
            {
                return true;
            }
            return false;
    }
    public void setUnleashed()
    {
        isUnleashed = true;
        GUIbehaviorScpt.toggleUnleash(true);
    }
}
