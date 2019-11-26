using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;

public class pathSaveAndLoad : MonoBehaviour {
        
    private const string fName = "savedPaths.txt";
    private const string placeHolderTxt = "%newPath%";    
    private const char txtSeparator = ';';
    private char directorySepChar;
    private const int numBtns = 8; // Total number of buttons to save
    public int getNumBtns()
    {
        return numBtns;
    }

    public GameObject[] pathSavedBtns = new GameObject[numBtns]; // Path button objects
    private string pathToTxt = "";
    private string rawInput;
    public int numCorrectInputs;
    public string[] pathsArray = new string[numBtns];
    public string[] folderNames = new string[numBtns];

    private void Awake()
    {
        directorySepChar = Path.DirectorySeparatorChar;
#if UNITY_EDITOR
        directorySepChar = '/';
#endif
        pathToTxt = Application.persistentDataPath + directorySepChar + fName;
        numCorrectInputs = 0;
        for(int i = 0; i < numBtns; i++)
        {
            pathSavedBtns[i] = GameObject.Find("Text" + i.ToString());
        }
        // Verify file existence, if file does not exist as yet, create it.
        if (!File.Exists(pathToTxt))
        {
            writeToFile(true); // Restore/Create file anew
        }// If file exists, check it for integrity. If corrupted, restore it to default state
        else {
            if (!validateAndStoreTxt(rawInput = ReadTxt()))
            {
                writeToFile(true);  // Restore/create file anew
            }
        }
        for (int i = 0; i < numBtns; i++)
        {
            if (i < numCorrectInputs)
            {
                pathSavedBtns[i].GetComponent<Text>().text = folderNames[i];
            }
            else
            {
                pathSavedBtns[i].GetComponentInParent<Button>().interactable = false;
                pathSavedBtns[i].GetComponent<Text>().text = "";
            }
        }
    }
    /*private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Overwrite File"))
        {
            writeToFile(true);
            Debug.Log("File overwritten");
        }
        if(GUI.Button(new Rect(10, 210, 150, 100), "Read"))
        {
            Debug.Log(rawInput = ReadTxt());
            Debug.Log(validateAndStoreTxt(rawInput));            
        }
        if (GUI.Button(new Rect(10, 410, 150, 100), "Delete"))
        {
            resetTxt();
            Debug.Log("Deleted text");
        }
    }*/
    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Resets the .txt file and writes values that were extracted upon loading and 
    /// values that were added during this session
    /// </summary>
    /// <param name="resetPaths">Optional, specify true if you want to create a clean </param>
    public void writeToFile(bool resetPaths = false)
    {
        string stringToWrite;
        // If resetting paths or no paths to write, ignore newPath
        if (resetPaths || pathsArray.Length < 1)
        {
            stringToWrite = placeHolderTxt;
            for (int i = 1; i < numBtns; i++)
            {
                stringToWrite += (txtSeparator + placeHolderTxt);
            }        
        }
        else // Otherwise, add the new path to the string of paths
        {
            int placeholders = numBtns - numCorrectInputs;
            stringToWrite = pathsArray[0];
            // Write the valid paths we extracted (and maybe added 1 to)
            for (int i = 1; i < numCorrectInputs; i++)
            {
                stringToWrite += (txtSeparator + pathsArray[i]);
            }
            // Write the extra placeholders
            for(int i = numCorrectInputs; i < numBtns; i++)
            {
                stringToWrite += (txtSeparator + placeHolderTxt);
            }
        }
        
        //Write the text to the test.txt file
        File.WriteAllText(pathToTxt, stringToWrite);
        /*StreamWriter writer = new StreamWriter(pathToTxt, true);        
        writer.WriteLine(stringToWrite);
        writer.Close();*/
    }
    string ReadTxt()
    {
        string returnString="";
        
        //Read the text from directly from the .txt file
        StreamReader reader = new StreamReader(pathToTxt);
        returnString = reader.ReadToEnd();
        reader.Close();
        return (returnString);
    }
    bool validateAndStoreTxt(string rawText)
    {        
        char[] sep = { txtSeparator };
        char[] folderSep = { Path.DirectorySeparatorChar };
        if (rawText.Split(sep).Length == numBtns)
        {
            for (int i = 0; i < numBtns; i++)
            {
                pathsArray[i] = rawText.Split(sep)[i];
                if (Directory.Exists(pathsArray[i]))
                {
                    string[] tempPathArray = pathsArray[i].Split(folderSep);
                    folderNames[numCorrectInputs] = tempPathArray[tempPathArray.Length-1];
                    numCorrectInputs++;
                }
            }
            return true;
        }
        else
        {
            return false;
        }        
    }
    public void saveNewPath(string path)
    {        
        if (numCorrectInputs == numBtns)
        {
            // State that it cant be saved?
            return;
        }
        // Check if directory exists
        if (Directory.Exists(path))
        {
            // If the path is a duplicate of one we already have, then dont save
            foreach (string tempPath in pathsArray)
            {
                if (tempPath == path)
                {
                    return;
                }
            }
            // Save the path
            pathsArray[numCorrectInputs] = path;
            // Perform the folder name extraction
            char[] folderSep = { txtSeparator };
            string[] tempPathArray = pathsArray[numCorrectInputs].Split(folderSep);
            folderNames[numCorrectInputs] = tempPathArray[tempPathArray.Length - 1];
            //pathSavedBtns[numCorrectInputs].GetComponentInParent<Button>().interactable = true; // Set the new button active
            //pathSavedBtns[numCorrectInputs].GetComponent<Text>().text = folderNames[numCorrectInputs]; // Edit the buttons text            
            numCorrectInputs++;       // Increment num correct buttons     
            writeToFile();
        }
    }
    /// <summary>
    /// Recreate/reset the pathToTxt.txt file
    /// </summary>
    public void resetTxt()
    {
        writeToFile(true);
        Awake();
    }
}
