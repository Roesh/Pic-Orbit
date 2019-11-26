using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// Controls behavior of GUI elemets in Pic Orbit Scene. Placing functions that any GUI element
/// may be expected to access here
/// </summary>
public class GUIscripts : MonoBehaviour {

    private GameObject quitMenuGO;
    private floatingMenu quitMenu;    
    private GameObject volumeMenuGO;
    private floatingMenu volumeMenu;
    private GameObject creditsMenuGO;
    private floatingMenu creditsMenu;
    public class sideMenu
    {
        private GameObject sideMenuSprite;
        private RectTransform sMenuTx;
        private Vector3 sMenuClosedPos;
        private Vector3 sMenuOpenPos;

        private Image sMenuSpriteComponent;
        private Sprite sMenuClosedSprite;
        private Sprite sMenuOpenSprite;

        private float xDiff;
        private bool menuIsClosed;
        private bool menuMoving;
        public sideMenu()
        {
            sideMenuSprite = GameObject.Find("SideMenuSprite");     // Find the SideMenuSprite
            sMenuTx = sideMenuSprite.GetComponent<RectTransform>(); // Get the rect transform associated with it

            sMenuSpriteComponent = sideMenuSprite.GetComponent<Image>();
            sMenuClosedSprite = Resources.Load<Sprite>("Sprites/SideMenuClosed");
            sMenuOpenSprite = Resources.Load<Sprite>("Sprites/SideMenuOpen");
            sMenuSpriteComponent.sprite = sMenuClosedSprite;
            sMenuClosedPos = sMenuTx.position;                      // or new Vector3(-720,130,0)
            menuIsClosed = true;
            menuMoving = false;
            xDiff = Vector3.Magnitude(GameObject.Find("RHS").GetComponent<RectTransform>().position - 
                GameObject.Find("LHS").GetComponent<RectTransform>().position);
            sMenuOpenPos = sMenuClosedPos + Vector3.right * xDiff;
        }
        
        public bool getMenuMovState()
        {
            return menuMoving;
        }

        public IEnumerator moveMenuCoroutine(float time=0.4f)
        {
            menuMoving = true;
            float t_e = 0;         
            if (menuIsClosed) {                
                while (t_e < time)
                {
                    sMenuTx.position = Vector3.Lerp(sMenuClosedPos, sMenuOpenPos, t_e / time);
                    t_e += Time.deltaTime;
                    yield return null;
                }
                sMenuTx.position = sMenuOpenPos;
                menuIsClosed = false;
                sMenuSpriteComponent.sprite = sMenuOpenSprite;
            }
            else
            {
                while (t_e < time)
                {
                    sMenuTx.position = Vector3.Lerp(sMenuOpenPos, sMenuClosedPos, t_e / time);
                    t_e += Time.deltaTime;
                    yield return null;
                }
                sMenuTx.position = sMenuClosedPos;
                menuIsClosed = true;
                sMenuSpriteComponent.sprite = sMenuClosedSprite;
            }
            menuMoving = false;
        }
        public void reportVars()
        {
            Debug.Log("Position: " + sMenuTx.position + " ||Open position: " + sMenuOpenPos + "||Closed position: " + sMenuClosedPos);
            Debug.Log("Sprite closed: " + sMenuClosedSprite.name);
            Debug.Log("Sprite open: " + sMenuOpenSprite.name);
            Debug.Log("xDiff: " + xDiff);
        }
    }
    private sideMenu sideMenuObj;
    public Button movementControlsBtn;
    private GameObject movementControls;
    private bool movementControlsState;

    private GameObject userInfoPanel;  // Rect transform of panel backdropping the text displayed to a user
    private Text userInfoText;
    private Vector3 panelScale;         
    private Vector3 initImagePos;       // Initial local position of closeup image
    private IEnumerator updateUserCoroutine;
    private IEnumerator oscRoutine;
    private bool oscillating = false;
    private bool displaying;

    private GameObject closeUpImgCanvas;
    private GameObject closeUpImg;
    private Vector2 closeUpImgBotLeftBound;     // x: left, y: bot
    private Vector2 closeUpImgTopRightBound;    // x: right, y top
    private bool imageCloseUpActive;
    private int closeUpImgIndex;
    private Vector3 initScale;
    private Vector3 initRot;
    private Vector3 imgActualScale;
    private bool persistentScaleEnabled;
    private float persistentScale;

    private GameObject UI_canvas;
    private inputControls ic;
    private bool startSetupComplete = false;
        
    /// <summary>
    /// The floating menu is used to instantiate menus like the quit and volume menus
    /// Only one of these menus may be active at one time. Use the APIs for opening and closing
    /// floating menus when linking Button actions
    /// </summary>
    public class floatingMenu {
        private GameObject MenuContainer;
        private bool isActive;

        public floatingMenu(GameObject menuContainerObj)
        {
            MenuContainer = menuContainerObj;
            MenuContainer.tag = "FltMnu";
            MenuContainer.SetActive(false);
            isActive = false;
        }
        public void deactivate() {
            MenuContainer.SetActive(isActive=false);            
        }
        public void toggleState()
        {
            isActive = !MenuContainer.activeSelf;
            if (isActive) {
                activate();
            }
            else
            {
                deactivate();
            }
        }
        public void activate()
        {
            // Floating menus deactivate other open floating menus so that only 1 is present
            // on screen at a time
            foreach(GameObject fltmnu in GameObject.FindGameObjectsWithTag("FltMnu"))
            {
                fltmnu.SetActive(false);
            }
            MenuContainer.SetActive(isActive = true);
        }


    }

    // Use this for initialization
	void Start () {
        // Find the floating menus (quit, volume and more to come) and assign them
        quitMenuGO = GameObject.Find("QuitMenu");
        quitMenu = new floatingMenu(quitMenuGO);
        volumeMenuGO = GameObject.Find("VolumeMenu");
        volumeMenu = new floatingMenu(volumeMenuGO);
        creditsMenuGO = GameObject.Find("CreditsMenu");
        creditsMenu = new floatingMenu(creditsMenuGO);
        
        // Create a sideMenu object
        sideMenuObj = new sideMenu();

        // Get the user display text objects
        userInfoPanel= GameObject.Find("userTextBackPanel");
        userInfoText = userInfoPanel.GetComponentInChildren<Text>();
        panelScale = userInfoPanel.transform.localScale;
        userInfoText.enabled = false;   //Disable them
        userInfoPanel.SetActive(false);
                
        // Close up image setup        
        closeUpImg = GameObject.Find("CloseUpImg");
        if (closeUpImg != null)
        {
            closeUpImgBotLeftBound = GameObject.Find("bot_left_Bound").transform.position;
            closeUpImgTopRightBound = GameObject.Find("top_right_Bound").transform.position;
            initImagePos = closeUpImg.transform.localPosition;
            initScale = closeUpImg.transform.localScale;
            initRot = closeUpImg.transform.eulerAngles;
            closeUpImgCanvas = GameObject.Find("CloseUpImg_Canvas");
            closeUpImgCanvas.SetActive(imageCloseUpActive = false);
            persistentScaleEnabled = true;
            persistentScale = 1f;
        }
        closeUpImgIndex = 0;

        ic = GetComponent<inputControls>();
        startSetupComplete = true;

        UI_canvas = GameObject.Find("UI_Canvas");
        //movementControls = GameObject.Find("movementControls");
        //movementControls.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(ic.quitbtn))           // If user pressed quit button key
        {
            quitMenu.toggleState();
        }
        if (Input.GetKeyDown(KeyCode.F1))           // If user pressed Left Alt key
        {
            toggleSideMenu();
        }
        if (Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(ic.backBtn))           // If user pressed prevScene key
            {
                if (GameObject.Find("folderHolderWasHere"))
                {
                    SceneManager.LoadScene("ImageFolderSelection");
                }
            }
        }        
        // If the close up controls are enabled, process their inputs
        if (ic.closeUpControlsOn)
        {
            // Toggle UI components in close up UI
            if (Input.GetKeyDown(ic.toggleUI))
            {
                GameObject.Find("CloseCloseUpImgButton").GetComponent<Image>().enabled =
                    !UI_canvas.activeSelf;
            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0.1f)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    zoomInCloseUp();                    
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    moveImgCloseUp("right");
                }
                else
                {
                    moveImgCloseUp("down");
                }

            }
            if(Input.GetAxis("Mouse ScrollWheel") < -0.1f)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    zoomOutCloseUp();                    
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    moveImgCloseUp("left");
                }
                else
                {
                    moveImgCloseUp("up");
                }
            }
            if (Input.GetKey(ic.zoomOutCloseUp))
            {   // Zoom out
                zoomOutCloseUp();
            }
            if (Input.GetKey(ic.zoomInCloseUp))
            {   // Zoom in
                zoomInCloseUp();
            }
            if (Input.GetKey(ic.moveImgUp)){
                moveImgCloseUp("up");
            }
            if (Input.GetKey(ic.moveImgDown))   {
                moveImgCloseUp("down");
            }
            if (Input.GetKey(ic.moveImgLeft) || (Input.GetAxis("Mouse ScrollWheel") < -0.3f
                && Input.GetKey(KeyCode.LeftShift)))   {
                moveImgCloseUp("left");
            }
            if (Input.GetKey(ic.moveImgRight) || (Input.GetAxis("Mouse ScrollWheel") > 0.3f
                && Input.GetKey(KeyCode.LeftShift)))  {
                moveImgCloseUp("right");
            }
            if (Input.GetKey(ic.zoomResetCloseUp))
            {   // Reset scale
                closeUpImg.transform.localScale = imgActualScale;
                closeUpImg.transform.eulerAngles = initRot;
                closeUpImg.transform.localPosition = initImagePos;
            }            
            if (Input.GetKeyDown(KeyCode.F4))
            {
                if(persistentScaleEnabled = !persistentScaleEnabled)
                {

                }
            }
            if (Input.GetKey(ic.rotImgUp))      { rotImgCloseUp("up");      }
            if (Input.GetKey(ic.rotImgDown))    { rotImgCloseUp("down");    }
            if (Input.GetKey(ic.rotImgLeft))    { rotImgCloseUp("left");    }
            if (Input.GetKey(ic.rotImgRight))   { rotImgCloseUp("right");   }
            if (Input.GetKey(ic.rotImgClock))   { rotImgCloseUp("clockwise");       }
            if (Input.GetKey(ic.rotImgCClock))  { rotImgCloseUp("counterclockwise");}
            if (Input.GetKey(ic.backBtn))       { closeCloseUpImage();      }
            if (Input.GetKeyDown(ic.oscKey)&&ic.oscEnabled)
            {                
                if (!oscillating)
                {
                    oscRoutine = oscillate();
                    StartCoroutine(oscRoutine);
                }
                else
                {   // If we are oscillating currently, stop oscillating
                    StopCoroutine(oscRoutine);
                    oscillating = false;
                    resetOsc();
                }
            }
        }
        /*if (Screen.fullScreen)
        {
            Screen.SetResolution(Screen.width, Screen.height, true);
        }*/
        if (Input.GetKeyDown(ic.toggleUI))
        {
            toggleUICanvas();
        }
    }

    // Toggles the side menu
    public void toggleSideMenu()
    {
        if (!sideMenuObj.getMenuMovState()) //If the menu isn't moving
        {
            StartCoroutine(sideMenuObj.moveMenuCoroutine(0.2f));  // Start the movement coroutine
        }
    }  
    
    /// <summary>
    /// Perform an action to change the display state of the volume menu
    /// </summary>
    /// <param name="action">0 - deactivate, 1 - activate, 2 - toggle state</param>
    public void performVolumeMenuAction(int action)
    {
        switch (action)
        {
            case 0:
                volumeMenu.deactivate();
                return;
            case 1:
                volumeMenu.activate();
                return;
            case 2:
                volumeMenu.toggleState();
                return;
        }
        
    }
    /// <summary>
    /// Perform an action to change the display state of the quit menu
    /// </summary>
    /// <param name="action">0 - deactivate, 1 - activate, 2 - toggle state</param>
    public void performQuitMenuAction(int action)
    {
        switch (action)
        {
            case 0:
                quitMenu.deactivate();
                return;
            case 1:
                quitMenu.activate();
                return;
            case 2:
                quitMenu.toggleState();
                return;
        }
    }
    public void performCreditsMenuAction(int action)
    {
        switch (action)
        {
            case 0:
                creditsMenu.deactivate();
                return;
            case 1:
                creditsMenu.activate();
                return;
            case 2:
                creditsMenu.toggleState();
                return;
        }
    }

    /// <summary>
    /// Display a message related to music
    /// </summary>
    /// <param name="dispText">string containing song name</param>
    /// <param name="msgType"> 0 - pure text message
    /// 1 - update song message. 
    /// 2 - song paused message.
    /// 3 - song resumed message.
    /// </param>
    public void updateDispUI(string dispText, int msgType)
    {
        IEnumerator dispRoutine=null;
        if (msgType == 0)
        {
            dispRoutine = displayTextToUser(dispText, 3, false);
        }
        if (msgType == 1)
        {
            dispRoutine = displayTextToUser("Now playing: " + dispText, 2.2f,false);
        }
        if (msgType == 2)
        {
            dispRoutine = displayTextToUser(dispText + " paused", 1, true);
        }
        if (msgType == 3)
        {
            dispRoutine = displayTextToUser(dispText + " resumed", 1, true);
        }
        // Quit if disp routine is null
        if (dispRoutine == null)
        {
            return;
        }
        // If a message is being displayed, discard the current message and display this one
        // instead
        if (displaying)
        {
            StopCoroutine(updateUserCoroutine);
            cleanUpDispText();
        }
        updateUserCoroutine = dispRoutine;
        StartCoroutine(updateUserCoroutine);               
    }
    private void cleanUpDispText()
    {
        userInfoText.enabled = false;
        userInfoPanel.transform.localScale = panelScale;
        userInfoPanel.SetActive(false);
        displaying = false;
    }
    /// <summary>
    /// Coroutine that displays information to the user via text GUI
    /// </summary>
    /// <param name="displayText">Text to display</param>
    /// <param name="dispTime">Amount of time to display the text</param>
    /// <param name="instantDisplay">Skip the open and close animations?</param>
    /// <returns></returns>
    private IEnumerator displayTextToUser(string displayText, float dispTime = 3,bool instantDisplay = false)
    {
        while(!startSetupComplete)
        {
            yield return null;
        }
        displaying = true;
        float t_e = 0;  //elapsed time
        float t_expand = 0.8f;
        float t_contract = 0.6f;
        userInfoPanel.SetActive(true);
        userInfoText.enabled = false;
        Vector3 zeroX = new Vector3(0, panelScale.y, panelScale.z);

        // Panel open animation
        if (!instantDisplay)
        {
            while (t_e < t_expand)
            {
                userInfoPanel.transform.localScale = Vector3.Lerp(zeroX, panelScale, t_e / t_expand);
                t_e += Time.deltaTime;
                yield return null;
            }
            t_e = 0; //Reset time
        }

        userInfoPanel.transform.localScale = panelScale;
        userInfoText.text = displayText;
        userInfoText.enabled = true;        
        yield return new WaitForSecondsRealtime(dispTime);
        userInfoText.enabled = false;

        // Panel close animation
        if (!instantDisplay)
        {
            while (t_e < t_contract)
            {
                userInfoPanel.transform.localScale = Vector3.Lerp(panelScale, zeroX,  t_e / t_contract);
                t_e += Time.deltaTime;
                yield return null;
            }
            userInfoPanel.transform.localScale = panelScale;
        }        
        userInfoPanel.SetActive(false);
        displaying = false;
    }   

    // Toggle movement control buttons
    public void toggleMovementControl()
    {
        movementControlsState = !movementControlsState;
        if(movementControlsBtn != null)
        {
            if (movementControlsState)
                movementControlsBtn.GetComponentInChildren<Text>().text = "Hide Controls";
            else
            {
                movementControlsBtn.GetComponentInChildren<Text>().text = "Show Controls";
            }
        }
        movementControls.SetActive(movementControlsState);
    }
    // Close-up image processing functions
    public Vector3 openCloseUpImage(Sprite imgSprite,int width, int height)
    {
        if (width == 0 || height == 0) { return Vector3.one; }; // Invalid height/ width prompts return        
        float scalingFrac = 1;
        Vector3 finalScale;
        closeUpImg.GetComponent<Image>().sprite = imgSprite;
        // Get the scale we need to use based on the image's height and width
        if (width > height)
        {
            scalingFrac = (float)height / width;
            finalScale = new Vector3(initScale.x * 1, initScale.y * scalingFrac, initScale.z);            
        }
        else
        {
            scalingFrac = (float)width / height;
            finalScale = new Vector3(initScale.x * scalingFrac, initScale.y * 1, initScale.z);
        }        
        closeUpImg.transform.localScale = finalScale; // Adjust the scale as necessary
        // If persistent scaling (zoom percent used for last image is used for this image) is on
        if (persistentScaleEnabled)
        {
            closeUpImg.transform.localScale *= persistentScale;
        }
        else//Otherwise reset the position and orientation
        {
            closeUpImg.transform.localEulerAngles = Vector3.zero;
            closeUpImg.transform.localPosition = Vector3.zero;
        }
        closeUpImgCanvas.SetActive(imageCloseUpActive=true);
        ic.closeUpControlsOn = true;
        ic.carouselControlsOn = false;
        return finalScale;
    }
    public void closeCloseUpImage()
    {
        if (oscillating)
        {
            StopCoroutine(oscRoutine);
            oscillating = false;
            resetOsc();
        }
        closeUpImgCanvas.SetActive(imageCloseUpActive = false);
        ic.carouselControlsOn = true;
        ic.closeUpControlsOn = false;
    }
    public void imgClickHandler(string imgName, Sprite sp,int imgWidth, int imgHeight, int index)
    {
        closeUpImgIndex = index;
        if (imageCloseUpActive) // Close any open image
        {
            closeCloseUpImage();
        }
        imgActualScale = openCloseUpImage(sp,imgWidth,imgHeight);
    }

    public void zoomInCloseUp(float zoomScaleSpeed = 1.05f,float maxScale = 4)
    {
        Vector3 tempScale = closeUpImg.transform.localScale;
        if ((tempScale += tempScale * zoomScaleSpeed *Time.deltaTime).x < imgActualScale.x*maxScale)
        {
            closeUpImg.transform.localScale = tempScale;
            persistentScale = tempScale.x / imgActualScale.x;
        }
    }
    public void zoomOutCloseUp(float zoomScaleSpeed = 0.95f, float minScale = 0.8f)
    {
        Vector3 tempScale = closeUpImg.transform.localScale;
        if ((tempScale -= tempScale* zoomScaleSpeed * Time.deltaTime).x > imgActualScale.x*minScale)
        {
            closeUpImg.transform.localScale = tempScale;
            persistentScale = tempScale.x / imgActualScale.x;
        }
    }
    public void moveImgCloseUp(string direction, float moveSpeed = 1000f)
    {
        Vector3 tempPos = closeUpImg.transform.position;
        switch (direction) {
            case "down":
                if ((tempPos += Vector3.up * moveSpeed * Time.deltaTime).y 
                    < closeUpImgTopRightBound.y+20*closeUpImg.transform.localScale.x)
                {
                    closeUpImg.transform.position = tempPos;
                }
                break;
            case "up":
                if ((tempPos -= Vector3.up * moveSpeed * Time.deltaTime).y 
                    > closeUpImgBotLeftBound.y - 20 * closeUpImg.transform.localScale.x)
                {
                    closeUpImg.transform.position = tempPos;
                }
                break;
            case "left":
                if ((tempPos += Vector3.right * moveSpeed * Time.deltaTime).x 
                    < closeUpImgTopRightBound.x + 20 * closeUpImg.transform.localScale.x)
                {
                    closeUpImg.transform.position = tempPos;
                }
                break;
            case "right":
                if ((tempPos -= Vector3.right * moveSpeed * Time.deltaTime).x 
                    > closeUpImgBotLeftBound.x -20 * closeUpImg.transform.localScale.x)
                {
                    closeUpImg.transform.position = tempPos;
                }
                break;             

        }
    }
    public void rotImgCloseUp(string direction, float rotSpeed = 180)
    {
        Vector3 pos = closeUpImg.transform.position;
        switch (direction)
        {
            case "down":
                closeUpImg.transform.RotateAround(pos,Vector3.right, rotSpeed * Time.deltaTime);                
                break;
            case "up":
                closeUpImg.transform.RotateAround(pos, Vector3.right, -rotSpeed * Time.deltaTime);
                break;
            case "left":
                closeUpImg.transform.RotateAround(pos, Vector3.up, rotSpeed * Time.deltaTime);
                break;
            case "right":
                closeUpImg.transform.RotateAround(pos, Vector3.up, -rotSpeed * Time.deltaTime);
                break;
            case "clockwise":
                closeUpImg.transform.RotateAround(pos, Vector3.forward, rotSpeed * Time.deltaTime);
                break;
            case "counterclockwise":
                closeUpImg.transform.RotateAround(pos, Vector3.forward, -rotSpeed * Time.deltaTime);
                break;
        }
    }
    public int nextImgCloseUp(int nImages)
    {
        return (int)Mathf.Repeat(++closeUpImgIndex, nImages);
    }
    public int prevImgCloseUp(int nImages)
    {
        return (int)Mathf.Repeat(--closeUpImgIndex, nImages);
    }
    private void resetOsc()
    {
        closeUpImg.transform.localScale = imgActualScale;
        closeUpImg.transform.eulerAngles = initRot;
        closeUpImg.transform.localPosition = initImagePos;
    }
    // coroutine that oscillates images in close up image view. Deprecating since Pic orbit rotates images and close up image
    // view is primarily meant for static viewing. See CameraControls for the alternate osc
    private IEnumerator oscillate()
    {     
        oscillating = true;
        Vector3 initPos = closeUpImg.transform.position;
        Vector3 initScale = closeUpImg.transform.localScale;
        GameObject bd = GameObject.Find("bs");                
        Vector3 bdInitPos = bd.transform.position;
        float scaleDiff = initScale.x / imgActualScale.x;
        float pi = Mathf.PI;
        float oscFreq = pi / 2;
        float rampRate = pi;
        float maxFreq = 4 * pi;
        float theta = 0;
        while (true)
        {
            if (Input.GetKey(ic.oscUp) && ic.oscEnabled)
            {
                oscFreq = Mathf.Clamp(oscFreq + rampRate * Time.deltaTime, 0, maxFreq);
            }
            if (Input.GetKey(ic.oscDown) && ic.oscEnabled)
            {
                oscFreq = Mathf.Clamp(oscFreq - rampRate * Time.deltaTime, 0, maxFreq);
            }
            theta += oscFreq * Time.deltaTime;
            float freqRatio = oscFreq / maxFreq;
            float bigOscAmp = (120 - freqRatio * 60)/scaleDiff;
            float bigOscSin = Mathf.Sin(theta);
            float bigOsc = bigOscAmp* bigOscSin;
            
            float littleOscAmp = Mathf.Pow(freqRatio, 2f) * (30 * (1 - freqRatio) + 10)/ scaleDiff;
            float littleOscSin = Mathf.Sin(theta * (1 + freqRatio / 2));
            float littleOsc = littleOscAmp * littleOscSin;
            closeUpImg.transform.position = initPos + Vector3.up * (bigOsc + littleOsc);
            closeUpImg.transform.localScale = initScale * (1 + 0.005f*(littleOsc));
            if(oscFreq > 0.67f)
            {
                bd.transform.position = bdInitPos + (Vector3.down + Vector3.forward) * littleOsc;                
            }
            yield return null;
        }
    }
    
    public void toggleUICanvas()
    {
        UI_canvas.SetActive(!UI_canvas.activeSelf);
    }
    public void DebugSayHi()
    {
        Debug.Log("Hi! You just called the DebugSayHi function!");
    }
}

