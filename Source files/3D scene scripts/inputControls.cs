using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class inputControls : MonoBehaviour {
    
    //============================= Begin controls ==================================//

    // ---Folder selection controls:   -------------------------------------------------==

    folderGUIBehavior uses____;
    // [Return] Open selected folders in Pic orbit
    // [F5] Refresh/update folders    
    // [F1] Opens arc menu contains refresh, path input menu and 
    // [O] Opens path input menu
    // [Escape] Close path input menu
    // [Up Arrow],[Down Arrow]/[Mouse Scroll Wheel] Scroll through folders
    // [Alt]+[A], Select All
    // [Alt]+[X], Select None
    // [Alt]+[D], Select None
    // [Alt]+[1-8], Open folder saved in positions 1-8 in the saved paths file
    FolderSelectionManager uses_____;
    // [Ctrl]+[U], Unleash mode on
    // Typing "unleash", unleash mode on


    ///*** Constants ***///
    pathSaveAndLoad contains;
    // numBtns = 8 - number of system paths to images that can be saved between sessions
    FileLoader contains_;
    // urlDeRoesh = "http://198.74.57.59/Master/"


    // ---Pic orbit scene controls:    -------------------------------------------------==

    // Menu controls
    public KeyCode quitbtn = KeyCode.Escape;    // Opens quit menu
    public KeyCode backBtn = KeyCode.Backspace; // Removes floating menus (Volume/ Exit)
    public KeyCode toggleUI = KeyCode.F2;       // Hides 
    materialSwapBehavior uses;
    // [Crtl+Q] - toggle background (Light/Dark background)
    GUIscripts uses_;
    // [F1] - toggling side Menu (Open/Close)
    // [Right Ctrl + Backspace] - Return to folder selection scene     
    ImageManager uses__;
    // [Tab] / [Shift] + [Tab] - Open Next / Previous close up image
    CameraControls uses___;
    // [F5] - Restore camera angles and zoom    
    // [F6] - Toggle auto-motion
    // [PgUp] / [PgDn] - Increase decrease auto motion speed

    // Music control
    public KeyCode prevSongBtn = KeyCode.Keypad4;   // Play next song (loops to last)
    public KeyCode pauseSongBtn = KeyCode.Keypad5;  // Pause current song
    public KeyCode nextSongBtn = KeyCode.Keypad6;   // Play next song (loops to first)
    public KeyCode skipForwardBtn = KeyCode.KeypadMultiply; // Skip forward by 5 seconds
    public KeyCode skipBackwardBtn = KeyCode.KeypadDivide;  // Skip backward by 5 seconds

    // Carousel control
    public KeyCode speedUpBtn = KeyCode.Keypad7;    // Speed up image rotation
    public KeyCode speedZeroBtn = KeyCode.Keypad8;  // Stop image rotation
    public KeyCode slowDownBtn = KeyCode.Keypad9;   // Slow down image rotation
    // [Up Arrow],  [W], [Scroll up]    - move cam up
    // [Down Arrow],[S], [Scroll down]  - move cam down
    // [Left Arrow],[A]                 - rotate cam left
    // [Right Arrow],[D]                - rotate cam right

    // Image close-up control        
    public KeyCode zoomOutCloseUp   = KeyCode.Keypad2;  // Zoom out
    public KeyCode zoomResetCloseUp = KeyCode.Keypad1;  // Reset image
    public KeyCode zoomInCloseUp    = KeyCode.Keypad3;  // Zoom in    
    public KeyCode moveImgUp        = KeyCode.W;        // Scroll up
    public KeyCode moveImgLeft      = KeyCode.A;        // Scroll right    
    public KeyCode moveImgDown      = KeyCode.S;        // Scroll down
    public KeyCode moveImgRight     = KeyCode.D;        // Scroll leftlt
    // [Scroll up] / [Scroll down] - Pan image up/down: GUIscripts
    // [Left Control] + ([Scroll up] / [Scroll down]) - Zoom in/out: GUIscripts
    // [Left Shift] + ([Scroll up] / [Scroll down]) - Pan image left/right: GUIscripts
    public KeyCode rotImgUp         = KeyCode.UpArrow;  // Rotate image up
    public KeyCode rotImgLeft       = KeyCode.LeftArrow;// Rotate image left
    public KeyCode rotImgDown       = KeyCode.DownArrow;// Rotate image down
    public KeyCode rotImgRight      = KeyCode.RightArrow;// Rotate image right
    public KeyCode rotImgClock      = KeyCode.PageUp;// Rotate image right
    public KeyCode rotImgCClock     = KeyCode.PageDown;// Rotate image right
    // [F4]  - Toggle persistent scaling between images: GUIscripts
    // [Backspace] - Exit close up control

    //============================= End controls ==================================//

    public KeyCode oscKey = KeyCode.Z;
    public KeyCode oscUp = KeyCode.KeypadMinus;
    public KeyCode oscDown = KeyCode.KeypadPlus;
    public bool carouselControlsOn;
    public bool closeUpControlsOn;
    public bool oscEnabled;
    
    // Use this for initialization
    void Start () {

        carouselControlsOn = true; // First point of control is carousel
        closeUpControlsOn = false;
        oscEnabled = false;
    }
	
	// Update is called once per frame
	void Update () {
        // Check if osc debug is enabled and oscValid object is in scene
        if (Input.GetKey(KeyCode.Alpha5) && Input.GetKey(KeyCode.Alpha3) && Input.GetKey(KeyCode.Alpha0))
        {            
            if (GameObject.Find("oscValid") != null)
            {
                oscEnabled = true;
                // turn on Bs
                GameObject.Find("bs").GetComponent<Image>().enabled =true;
            }
        }

    }
}
