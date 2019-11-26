using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class folderGUIBehavior : MonoBehaviour {

    private FolderSelectionManager fsManager;
    private pathBasedLoader pathLoader;
    private pathSaveAndLoad pathTxtSaver;
    private GameObject loadingObject;
    private GameObject selectOne;
    private GameObject arcSideMenu;
    private GameObject pathInputMenu;
    public GameObject[] selectionStats;
    public GameObject unleashedText;
    private bool arcMenuClosed;
    private bool arcMenuMoving;
    private int currentOffset;   // How many rows have we scrolled down?
    private int maxOffset;       // How many extra rows do we have?
    private KeyCode[] numberInputs = {KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
        KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8};

    private void Awake()
    {
        currentOffset = 0;
        maxOffset = 0;
        arcMenuClosed = true;
        arcMenuMoving = false;
        loadingObject = GameObject.Find("loadingObject");        
        selectOne = GameObject.Find("selectOne");        
        arcSideMenu = GameObject.Find("arcMenu");        
        pathInputMenu = GameObject.Find("pathInputMenu");
        fsManager = FindObjectOfType<FolderSelectionManager>();
        pathLoader = FindObjectOfType<pathBasedLoader>();
        pathTxtSaver = FindObjectOfType<pathSaveAndLoad>();
    }
    
    // Use this for initialization
    void Start () {
        pathInputMenu.SetActive(false);
        selectOne.SetActive(false);
        loadingObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (pathInputMenu.activeSelf)
            {
                pathLoader.setPath();
            }
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            pathInputMenu.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pathInputMenu.SetActive(false);
        }
        // Refresh the entries
        if (Input.GetKeyDown(KeyCode.F5))
        {
            refreshImages();
        }
        // Toggle arced Side menu
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleArcMenu();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetAxis("Mouse ScrollWheel") > 0.3f)
        {
            moveUpPressed();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel") < -0.3f)
        {
            moveDownPressed();
        }
        if((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.A))
        {
            selectAllPressed();
        }
        if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.X))
        {
            selectNonePressed();
        }
        if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.D))
        {
            doneBtnPressed();
        }
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)){
            int maxBtns = FindObjectOfType<pathSaveAndLoad>().getNumBtns();
            if(maxBtns > numberInputs.Length)
            {
                maxBtns = numberInputs.Length;
            }
            for(int i = 0; i < maxBtns; i++)
            {
                if (Input.GetKeyDown(numberInputs[i]))
                {
                    savedPathClicked(i);
                }
            }
        }
    }

    public void setMaxOffset(int maxoffset)
    {
        maxOffset = maxoffset;
    }
    public void moveUpPressed()
    {
        if (currentOffset > 0)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("folderObject"))
            {
                go.GetComponent<FolderObjectBehavior>().scrollMove(-1);
            }
            currentOffset--;
        }
        //Debug.Log("c: " + currentOffset + " |max: " + maxOffset);
    }

    public void moveDownPressed()
    {
        if (maxOffset - currentOffset > 0)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("folderObject"))
            {
                go.GetComponent<FolderObjectBehavior>().scrollMove(1);
            }
            currentOffset++;
        }
    }

    public void selectAllPressed()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("folderObject"))
        {
            // If folder is not checked, then check it
            if (!go.GetComponent<FolderObjectBehavior>().toggleComp.isOn)
            { 
                go.GetComponent<FolderObjectBehavior>().setFolderUsedExternal(true);
            }
        }
        
    }
    public void selectNonePressed()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("folderObject"))
        {
           
            // If folder is checked, then uncheck it
            if (go.GetComponent<FolderObjectBehavior>().toggleComp.isOn)
            {
                go.GetComponent<FolderObjectBehavior>().setFolderUsedExternal(false);
            }
        }
    }

    public void doneBtnPressed()
    {
        if (fsManager.collectSelectedImagesFromCheckedFolders()) {
            StopAllCoroutines();
            loadingObject.SetActive(true);            
            SceneManager.LoadScene("picOrbit");
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(selectOneRoutine());
        }        
        //SceneManager.LoadScene(1);
    }

    private IEnumerator selectOneRoutine() {
        for(int i = 0; i < 2; i++)
        {
            selectOne.SetActive(true);
            yield return new WaitForSecondsRealtime(0.2f);
            selectOne.SetActive(false);
            yield return new WaitForSecondsRealtime(0.1f);
        }
        selectOne.SetActive(true);
        yield return new WaitForSecondsRealtime(2.5f);
        selectOne.SetActive(false);
    }
    public void exitBtnPressed()
    {
        if (pathInputMenu.activeSelf)
        {
            pathInputMenu.SetActive(false);
        }
        else
        {
            exitGame();
        }
    }
    public void exitGame()
    {
        Application.Quit();
    }

    public void ToggleArcMenu()
    {
        // If arc menu is not moving, then move it
        if (!arcMenuMoving)
        {
            int direction;
            // Direction is reversed if arc menu is open already
            if (!arcMenuClosed)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
            StartCoroutine(rotateArcMenu(direction,0.2f));
        }
    }
    // DO NOT PREMATURELY TERMINATE THIS COROUTINE
    private IEnumerator rotateArcMenu(int direction, float t = 0.6f)
    {
        arcMenuMoving = true;
        float t_e = 0;
        Vector3 initRot = arcSideMenu.transform.localEulerAngles;
        Vector3 finalRot = initRot + Vector3.back * 90*direction;
        while (t_e < t)
        {
            arcSideMenu.transform.localEulerAngles = Vector3.Lerp(initRot, finalRot, t_e / t);
            t_e += Time.deltaTime;
            yield return null;
        }
        arcSideMenu.transform.localEulerAngles = finalRot;
        arcMenuMoving = false;
        arcMenuClosed = !arcMenuClosed;
    }
    public void refreshImages()
    {
        Destroy(GameObject.Find("folderHolder"));
        //GameObject.Find("loadingObject").SetActive(true);
        SceneManager.LoadScene("ImageFolderSelection");
    }
    public void togglePathMenu()
    {
        pathInputMenu.SetActive(!pathInputMenu.activeSelf);
    }
    public void specifyPathClicked()
    {
        pathLoader.setPath();
    }
    public void savedPathClicked(int btnNumber)
    {
        pathLoader.setPath(pathTxtSaver.pathsArray[btnNumber]);
    }
    public void toggleUnleash(bool unleashOn)
    {
        if (unleashOn)
        {
            foreach(GameObject go in selectionStats)
            {
                go.GetComponent<Text>().enabled = false;
            }
            unleashedText.GetComponent<Text>().enabled = true;
        }
        else{
            foreach (GameObject go in selectionStats)
            {
                go.GetComponent<Text>().enabled = true;
            }
            unleashedText.GetComponent<Text>().enabled = false;
        }
    }
}
