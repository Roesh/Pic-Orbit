using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pathBasedLoader : MonoBehaviour {

    private folderGUIBehavior guiBehavior;
    private pathSaveAndLoad pathSaver;
    public string path;
    public GameObject pathInput;
    public bool savePath;

    private static pathBasedLoader _instance;
    public static pathBasedLoader Instance { get { return _instance; } }
    
    private void Awake()
    {
        savePath = true;
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        // Persist this object. Destroy it if you want to reload the scene
        DontDestroyOnLoad(this);
        path = null;
        guiBehavior = FindObjectOfType<folderGUIBehavior>();
        pathSaver = FindObjectOfType<pathSaveAndLoad>();
    }
    // Use this for initialization
    void Start () {
               
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setPath(string passedPath = "NoPath")
    {
        if (passedPath != "NoPath")
        {
            path = passedPath;
        }
        else
        {
            path = GameObject.Find("pInputText").GetComponent<Text>().text;
        }
        // Check if path is not null. If it isn't then        
        if ((path != "") || (path == null))
        {
            if (savePath) // check if we have to save the path
            {
                pathSaver.saveNewPath(path);
            }
            // Restart the scene
            guiBehavior.refreshImages();
        }
        else
        {
            // Hide path menu
            GameObject.Find("pathInputMenu").SetActive(false);
        }
    }
    public void toggleSave()
    {
        savePath = !savePath;
    }
}
