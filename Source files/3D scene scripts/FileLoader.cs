using System.Collections;
// Modifying Calls to system.io based on platform. Webgl does not support these calls
#if UNITY_WEBGL
#else
using System.IO;
#endif
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


/// <summary>
/// This script is responsible for loading files and images from
/// locations including a local file (PC/ Mobile) or a web repository
/// It has functions that return textures
/// </summary>
public class FileLoader : MonoBehaviour
{
#if UNITY_WEBGL
    const string urlDeRoesh = "http://198.74.57.59/Master/";
    public string[] dirInfo; // Holds directory information in string array
                             // String array will consist of full filenames of images
#else
    public FileInfo[] dirInfo;      // Holds directory information in FileInfo array(valid for Method 0,1)
#endif
    private char separator;

    private void Awake()
    {
#if UNITY_WEBGL
        separator = '/';
#elif UNITY_EDITOR
        separator = '/';
#else
        separator = Path.DirectorySeparatorChar;  // Depending on OS and web, separator is "/" or "\"
#endif
    }

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary> Collects file info for all image files in the directory searched. When running in WEBGL
    /// mode, only method 2, Roshan's website is supported</summary>
    /// <param name="method">Integer to set filesearch method- 0: Application.DataPath,
    /// 1: Application.PersistentDatapath, 2: Roshan's Website, 3: Public google drive folder</param>
    /// <param name="folder"> The folder to user (ImageFolder, MusicFolder)</param>
    /// <param name="customPath"> if the user specified a VALID directory string, enter it here</param>  
    /// <returns>WEBGL mode - Returns string array with names of files in "folder" which should be a path on the 
    /// server
    /// FileInfo array which has info about files in "folder" variable
    /// </returns>
#if UNITY_WEBGL
    public string getDirInfo(int method, string folder, string customPath = null)
    {
        string fileNames = null;
        if(method != 2)
        {
            return null;
        }
        string path;
        if ((path = customPath) == null) // If we're not using a custom path
        {
            path = urlDeRoesh+"Default/";    // Full path to the images folder
        }
        WWW www = new WWW(path + "fileNames.txt");
        if (www.error != null)
        {
            Debug.Log("Ooops, something went wrong...");
        }
        else
        {
            fileNames = www.text;
            Debug.Log(fileNames);
        }
        return null;
    }
    
#else
    public FileInfo[] getDirInfo(int method, string folder, string customPath = null)
    {
        
        FileInfo[] fileInfos = null;
        string path;
        DirectoryInfo dir;
        // How to obtain all the files in a folder: https://answers.unity.com/questions/16433/get-list-of-all-files-in-a-directory.html
        // Data path variables: https://docs.unity3d.com/ScriptReference/Application-dataPath.html
        switch (method) {
            case 0:
                // Set the path equal to the custom Path. If it is null, meaning the user did not specify a valid custom path
                // Then set the path to our original path
                if ((path = customPath) == null) // If we're not using a custom path
                {
                    path = Application.dataPath + separator + folder;    // Full path to the images folder
                }
                // If the directory does not exist already, create it
                if (!Directory.Exists(path))                         
                {
                    Directory.CreateDirectory(path);
                }                
                dir = new DirectoryInfo(path);                       // Get the directory info and store it in dirInfo
                fileInfos = dir.GetFiles("*.*", (SearchOption)1);    // Search through subfolders as well for images      
                path = Application.dataPath + separator + folder + separator + "~Ignored" + folder;
                // If the unused directory does not exist already, create it
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                break;
            case 1:
                path = Application.persistentDataPath + separator + folder;    // Full path to the images folder
                GameObject.Find("debugText").GetComponent<Text>().text = path;
                if (!Directory.Exists(path))                                   // If the directory does not exist already, create it
                {
                    Directory.CreateDirectory(path);
                }
                dir = new DirectoryInfo(path);                                 // Get the directory info and store it in dirInfo
                fileInfos = dir.GetFiles("*.*", (SearchOption)1);              // Search through subfolders as well for images      
                path = Application.dataPath + separator + folder + separator + "~Ignored" + folder;
                // If the unused directory does not exist already, create it
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                break;
            case 2:
                break;
            case 3:
                break;
            default:
                break;
        }
        return fileInfos;
    }
#endif
    
    /// <summary> Collects file info for all image files in the directory searched.</summary>
    /// <param name="method">Integer to set filesearch method- 0: Application.DataPath,
    /// 1: Application.PersistentDatapath, 2: Roshan's Website, 3: Public google drive folder</param>
    /// <param name="folder"> The folder to user (ImageFolder, MusicFolder)</param>    
    /// <param name="customPath"> if the user specified a VALID directory string, enter it here</param>    
    /// <returns>FileInfo array which has info about files in string folder</returns>
    public DirectoryInfo[] GetDirectoryInfo(int method, string folder,string customPath = null)
    {
        DirectoryInfo[] dirInfos = null;
        string path;
        DirectoryInfo dInfo;
        switch (method)
        {
            case 0:
                if ((path = customPath) == null) // If we're not using a custom path
                {
                    path = Application.dataPath + separator + folder;      // Full path to the images folder
                }
                // If the directory does not exist already, create it. If the user specified a path that did not exist, we wont create 
                // it because customPath should be null                
                if (!Directory.Exists(path))                           
                {
                    Directory.CreateDirectory(path);
                }
                dInfo = new DirectoryInfo(path);                       // Get the directory info and store it in dirInfo
                dirInfos = dInfo.GetDirectories();
                break;
            case 1:
                path = Application.persistentDataPath + separator + folder;     // Full path to the images folder
                if (!Directory.Exists(path))                                    // If the directory does not exist already, create it
                {
                    Directory.CreateDirectory(path);
                }
                dInfo = new DirectoryInfo(path);                                // Get the directory info and store it in dirInfo
                dirInfos = dInfo.GetDirectories();
                break;        
        }
        return dirInfos;
    }
    public void listDirInfo()
    {
        foreach (FileInfo f in dirInfo)
        {            
            Debug.Log(f.FullName);
        }
    }

    /// <summary>
    /// Load PNG used to load a 2d texture from a file. Works with method 1, Application.DataPath
    /// </summary>
    /// <param name="filePath">Full path to file</param>
    /// <returns></returns>
    public static Texture2D LoadImg(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        // Make sure file referenced by filePath string exists in the system 
        // and that it isn't in an ignored folder
#if UNITY_STANDALONE
        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(4,4,TextureFormat.DXT1, false);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
#endif

        return tex;
    }

    public IEnumerator LoadClip(string path, int val, MusicManager mg)
    {
        WWW www = new WWW("file://" + path);

        AudioClip clip = www.GetAudioClip();
        while (!(clip.loadState == AudioDataLoadState.Loaded))
        {
            yield return www;
        }
        mg.musicObjects[val].loadScoreFromFileLoader(clip);
    }


    /// <summary>
    /// Load PNG used to load a 2d texture from the web. Works with method 3, roshan's website
    /// </summary>
    /// <param name="filePath">Full path to file</param>
    /// <returns></returns>
    public IEnumerator LoadImgWeb(string url, ImageManager mg)
    {
        Texture2D tex;
        tex = new Texture2D(4, 4, TextureFormat.DXT1, false);       // Create a new texture
        string name = url.Split('/')[url.Split('/').Length-1];      // Parse the file name from the url
        name = name.Split('.')[0];
            
        using (WWW www = new WWW(url))
        {
            yield return www;                   // Coroutine waits 
            www.LoadImageIntoTexture(tex);      // Load the image into the image
            mg.receiveImgFromWeb(tex, name);    // Ask the image manager to accept the loaded image
        }     
    }
}

