using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// ImageManager summary
/// This script is responsible managing the images in the scene 
/// When the scene opens, it uses imageLoader to query the specified image directory 
/// for files, then asks the imageLoader to load all the images in that directory 
/// as 2dTextures. It then loads ImageFrame prefab Objects, and passes in values to
/// initialize their position based on an algorithm (to be developed). It also passes in the 
/// 2DTexture that that ImageFrame prefab should showcase
/// ImageManager summary
public class MusicManager : MonoBehaviour {

    private FileInfo[] fileInfos;       // Holds file info    
    public int numScores;               // the number of musical scores loaded
    private int maxScores;              // The maximum number of musical scores to load
    public int currentScore;
    private FileLoader scoreLoader;     // File loader script used to access the music
    public GameObject boomBox;
    private AudioSource audioSource;    // audio source object to play clips
    private GUIscripts guiScpt;
    inputControls ic;
    private bool musicExists;

    // Variable method holds which location music files are stored in
    // 0-Application.DataPath, 1-Application.persistentDatapath,
    // 2-roesh website, 3-google drive folder
    public int method;
    public List<musicObject> musicObjects = new List<musicObject>();

    /// imageObject Summary
    /// This class is used to store a single image object's data, such as its
    /// display name, its file name, its 2Dtexture and the gameobjects that
    /// display it
    /// imageObject Summary
    public class musicObject
    {
        public string scoreName;       // Music display name  
        public string path;            // Path to music from 
        public AudioClip score;        // Audioclip object that holds the audio        
        public AudioSource aS;        
        
        public musicObject(string name, string fPath, AudioSource aSo)
        {
            // Set class variables
            scoreName = name;
            path = fPath;
            aS = aSo;
        }

        // Instantiate a new object from the loaded image frame prefab
        // Offset the clone by the desired value
        public AudioClip loadScore()
        {
            return (score);
        }
        public void loadScoreFromFileLoader(AudioClip ac)
        {
            score = ac;         
            aS.clip = score;        // Set the audio clip
            aS.time = 0;            // Reset the time of play
            aS.PlayDelayed(0.2f);   // Play the clip after a short delay
        }
        public void genReport()
        {
            Debug.Log("Name: " + scoreName + " ||Length (sec): " + score.length);
        }
    }
    

    // Use this for initialization
    void Start () {
        ic = FindObjectOfType<inputControls>();
        guiScpt = FindObjectOfType<GUIscripts>();

        method = 0;     //Search method is using Application.DataPath
#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_IPHONE
        method = 1;
#endif
        numScores = 0;
        currentScore = 0;
        maxScores = 10;
        boomBox = GameObject.Find("BoomBox");
        audioSource = FindObjectOfType<AudioSource>();  // Get the Audio source object from the scene

        scoreLoader = FindObjectOfType<FileLoader>();               // Find an imageloader script
        string cPath;
        // If the user specified a custom folder path for the images, check to see if there are music files there
        if ((cPath = FindObjectOfType<pathBasedLoader>().path) != null)
        {
            fileInfos = scoreLoader.getDirInfo(method, "MusicFolder",cPath);
            createScoreObjects();
        }
        // If there were no valid music files in the custom path, or if a custom path was not specified
        if(musicObjects.Count == 0)
        {
            fileInfos = scoreLoader.getDirInfo(method, "MusicFolder");   // Get the music directory info, using method
        }
        createScoreObjects();        
        // Image manager now loads music as well
    }
	
    public void startInitialMusic()
    {
        // If we loaded at least one clip, play the first one by default
        if (musicObjects.Count > 0)
        {
            musicExists = true;
        }
        else
        {
            musicExists = false;
        }
        if (musicExists)
        {
            playScore();
            guiScpt.updateDispUI(musicObjects[currentScore].scoreName, 1);
        }
        else
        {
            // guiScpt.updateDispUI("No music found in MusicFolder", 0,true);
        }
    }

	// Update is called once per frame
	void Update () {
        if (musicExists)
        {
            if (Input.GetKeyDown(ic.nextSongBtn))
            {
                playNextScore();
            }
            else if (Input.GetKeyDown(ic.prevSongBtn))
            {
                playPrevScore();
            }
            else if (Input.GetKeyDown(ic.pauseSongBtn))
            {   // If score is currently playing, then pause it
                if (musicObjects[currentScore].aS.isPlaying)
                {
                    musicObjects[currentScore].aS.Pause();
                    guiScpt.updateDispUI(musicObjects[currentScore].scoreName, 2);
                }
                else // Otherwise, resume the game
                {
                    musicObjects[currentScore].aS.UnPause();
                    guiScpt.updateDispUI(musicObjects[currentScore].scoreName, 3);
                }
            }
            else if (Input.GetKeyDown(ic.skipForwardBtn))
            {
                skip(5f);
            }
            else if (Input.GetKeyDown(ic.skipBackwardBtn))
            {
                skip(-5f);
            }
        }
    }

    /// <summary>
    /// This function calls FileLoader's ascynchrounous sound file loading coroutine
    /// Once its done, fileloader messages this script and the music object plays the returned clip
    /// </summary>
    /// <param name="scoreNum">The index of the music object to play the clip for</param>    
    void playScore(int scoreNum = 0)
    {
        if (musicObjects[currentScore].aS.isPlaying)
        {
            musicObjects[currentScore].aS.Stop();
        }
        // This ends up calling musicObject class' loadScoreFromFileLoader method
        StartCoroutine(scoreLoader.LoadClip(musicObjects[scoreNum].path, scoreNum, this));
    }
    
    /// <summary>
    /// Plays the next music clip
    /// </summary>
    public void playNextScore()
    {
        if (++currentScore > numScores-1)
        {
            currentScore = 0;
        }
        playScore(currentScore);
        guiScpt.updateDispUI(musicObjects[currentScore].scoreName, 1);
    }

    /// <summary>
    /// Play the previous music clip
    /// </summary>
    public void playPrevScore()
    {
        if (--currentScore < 0)
        {
            currentScore = numScores-1;
        }
        playScore(currentScore);
        guiScpt.updateDispUI(musicObjects[currentScore].scoreName, 1);
    }

    public void skip(float amount){
        float seekTime = musicObjects[currentScore].aS.time + amount; // add time to be playing at
        seekTime = Mathf.Clamp(seekTime, 0, musicObjects[currentScore].aS.clip.length);
        musicObjects[currentScore].aS.time = seekTime;
        Debug.Log(seekTime);        
    }

    /// <summary>
    /// Create the music score objects from the file info array
    /// </summary>
    void createScoreObjects()
    {
        switch (method)
        {
            case 0:
                foreach (FileInfo f in fileInfos)
                {
                    string fPath = f.FullName;
                    // if the file has an audio extension of aif, wav, mp3 or ogg, then create an object with its info
                    if ((fPath.EndsWith(".aif") || fPath.EndsWith(".wav")|| fPath.EndsWith(".ogg"))&&!fPath.Contains("~Ignored"))
                    {   
                        {   // If we haven't reached the limit for num scores in scene, keep adding music
                            if (numScores < maxScores)
                            {
                                string dispName = f.Name.Split('.')[0];                                
                                musicObjects.Add(new musicObject(dispName,fPath,audioSource));
                                numScores++;
                            }
                            else {
                                break;
                            }
                        }
                    }
                }
                break;
            case 1:
                foreach (FileInfo f in fileInfos)
                {
                    string fPath = f.FullName;
                    // if the file has an audio extension of aif, wav, mp3 or ogg, then create an object with its info
                    if ((fPath.EndsWith(".aif") || fPath.EndsWith(".wav") || fPath.EndsWith(".ogg")) && !fPath.Contains("~Ignored"))
                    {
                        {   // If we haven't reached the limit for num scores in scene, keep adding music
                            if (numScores < maxScores)
                            {
                                string dispName = f.Name.Split('.')[0];
                                musicObjects.Add(new musicObject(dispName, fPath, audioSource));
                                numScores++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                break;
            case 2:
                break;
            case 3:
                break;
        }
        
    }

    public void changeVolume()
    {

    }
}
