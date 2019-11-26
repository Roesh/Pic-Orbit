using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FolderObjectBehavior : MonoBehaviour {

    float topBound;
    float botBound;
    float yDiff;

    private GameObject orphanage; // This object acts as a parent to folders that are no longer in view
    private GameObject canvasObj;

    public Toggle toggleComp;
    public Text numImagesText;    
    public Text folderNameText;

    private bool specificImageFilesSelected;

    private void Awake()
    {
        orphanage = GameObject.Find("orphanage");
        canvasObj = GameObject.Find("Canvas");
        topBound = GameObject.Find("FolderTopBound").transform.localPosition.y;
        botBound = GameObject.Find("FolderBotBound").transform.localPosition.y;
        yDiff = (topBound - botBound) / 6;

        toggleComp = GetComponent<Toggle>();
        toggleComp.isOn = false;
    }
    // Use this for initialization
    void Start () {
        Text numText = GameObject.Find("selectedNumber").GetComponent<Text>();
        numText.color = Color.white;
        GameObject.Find("maxImagesWarning").GetComponent<Text>().enabled = false;
        specificImageFilesSelected = false;
    }
	
	// Update is called once per frame
	void Update () {

    }

    //
    private void OnGUI()
    {
        /*if (GUILayout.Button("Move up"))
        {
            scrollMove(1);
        }*/
    }

    /// <summary>
    /// Moves the folder object a certain amount based on the distance between 
    /// the top and bottom objects and in the passed direction
    /// </summary>
    /// <param name="direction">+1 to move up. -1 to move down</param>
    public void scrollMove(int direction)
    {
        // If we were in the orphange to prevent viewing, set the parent 
        // to the canvas to compare the true y coordinates
        if (transform.parent == orphanage.transform)
        {
            transform.SetParent(canvasObj.transform);
        }
        // Add the y diff and Get the true y value
        transform.localPosition += direction * Vector3.up * yDiff;        
        float currentY = transform.localPosition.y;
        // If the value is out of bounds (greater than top, smaller than bot)
        // Return to orhpanage
        if (currentY > topBound+1 || currentY < botBound-1)
        {
            transform.SetParent(orphanage.transform);
        }
    }

    /// <summary>
    /// Set the status of this folder, true if it will
    /// be used in pic orbit.
    /// This will update the number of images you have selected so make sure
    /// that this folder is not already checked/uncheked before calling it
    /// It will update the checked status of the checkbox
    /// </summary>
    /// <param name="used">true: use this folder in pic orbit. False: don't</param>
    public void setFolderUsed(bool used)
    {        
        Text numText = GameObject.Find("selectedNumber").GetComponent<Text>();
        if (used)
        {
            //Debug.Log(folderNameText.text + " incremented by " + numImagesText.text);
            int numImages = int.Parse(numText.text) + int.Parse(numImagesText.text);
            numText.text = numImages.ToString();
            // If greater than 32 display that num images exceeds display capacity
            if(numImages > 32)
            {                
                numText.color = Color.yellow;
                if (!FindObjectOfType<FolderSelectionManager>().isUnleashed)
                {
                    GameObject.Find("maxImagesWarning").GetComponent<Text>().enabled = true;
                }
            }
        }
        else
        {
            //Debug.Log(folderNameText.text + " decremented by " + numImagesText.text);
            int numImages = int.Parse(numText.text) - int.Parse(numImagesText.text);
            numText.text = numImages.ToString();
            if (numImages <= 32)
            {
                numText.color = Color.white;
                GameObject.Find("maxImagesWarning").GetComponent<Text>().enabled = false;
            }
        }         
    }

    public void setFolderUsedExternal(bool used)
    {
        toggleComp.isOn = used; // This should call the setFolderUsed function
    }

    public void updateNumImages(int num)
    {
        numImagesText.text = num.ToString();
    }

    public void updateFolderName(string folderName) {
        folderNameText.text = folderName;
    }
        
    void reportDistanceVals()
    {
        Debug.Log("Top bound: " + topBound);
        Debug.Log("Bot bound: " + botBound);
        Debug.Log("yDiff: " + yDiff);
    }
}

