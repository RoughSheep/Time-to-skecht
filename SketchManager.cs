using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SketchManager : MonoBehaviour
{
    // Managers
    private Transform _AppManager;
    private Transform _ActionsManager;
    private Transform _GrabManager;
    private Transform _HistoryManager;

    // Headset & Controllers
    private Transform headset;
    private Transform leftController;
    private Transform rightController;

    // Elements on controllers
    private Transform leftSketchStarter;
    private Transform rightSketchStarter;
    private Transform leftBrush;
    private Transform rightBrush;
    private Transform leftEraser;
    private Transform rightEraser;
    private Transform leftGroupSelector;
    private Transform rightGroupSelector;

    private Transform leftObjectSelector;
    private Transform rightObjectSelector;

    private Transform leftAudioSource;
    private Transform rightAudioSource;

    // Particles effects
    private ParticleSystem leftBrushEffect;
    private ParticleSystem rightBrushEffect;
    private ParticleSystem leftEraserEffect;
    private ParticleSystem rightEraserEffect;

    // Work & trash folders
    private Transform workFolder;
    private Transform selectFolder;

    // Sketching
    [Header("Sketch & Erase")]
    public GameObject sketchPrefab;
    public GameObject eraserPrefab;
    private GameObject mySketch;
    private LineRenderer myLineRenderer;

    private bool isSketching = false;
    private bool isErasing = false;

    private int pointsOnMySketch = 0;
    private Color sketchColor = Color.white;
    private Color lastSketchColor = Color.white;
    private float sketchSize = 0.01f;
    private float eraseSize = 0.01f;
    private int sketchId = 0;
    private float sketchTimer = 0;

    // Symmetric sketch
    [Header("Symmetric sketch")]
    public GameObject newSymmetryPlane;
    private GameObject mySymmetryPlane;
    private GameObject symmetryLeftBrush;
    private GameObject symmetryRightBrush;
    private GameObject mySymmetricSketch;
    private LineRenderer mySymmetricLineRenderer;
    private bool symmetryMode = false;

    // Sketch folder
    [Header("Sketch folder")]
    public GameObject sketchFolderPrefab;
    private GameObject mySketchFolder;
    int folderNumber = 0;
    private bool isSelectingGroup = false;

    // Select Object folder
    //[Header("Selec folder")]
    private GameObject myOriginalFolder;
    private bool isSelectingObject = false;

    private GameObject mySelectFolder;

    private Transform[,] currentSelectedObjects = new Transform[1000, 1000];
    private Transform TheLastSelectedObject;

    private int i = 0;
    private int j = 0;


    // Sketching menu
    [Header("Menu")]
    public GameObject newSketchingMenu;
    private GameObject sketchingMenu;
    private bool sketchingMenuIsOpen = false;
    private string currentMode = "SketchManager";
    private string previousSketchingTool = "";

    // Pointer on menu
    [Header("Pointer")]
    public GameObject newPointer;
    private GameObject leftPointer;
    private GameObject rightPointer;
    private bool pointerIsOpen = false;

    // Player
    private bool rightHander = true;
    //private bool wasRightHander = true;

    // Menu cleaner
    private Transform menuCleaner;

    // Helpers on controllers
    private bool isHelping = false;
    private Transform mainHelperOnLeftController;
    private Transform mainHelperOnRightController;
    private List<Transform> leftHelpers = new List<Transform>();
    private List<Transform> rightHelpers = new List<Transform>();

    // Other
    private bool coroutineIsRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        // Get other managers
        _AppManager = GameObject.Find("_AppManager").transform;
        _ActionsManager = GameObject.Find("_ActionsManager").transform;
        _GrabManager = GameObject.Find("_GrabManager").transform;
        _HistoryManager = GameObject.Find("_HistoryManager").transform;

        // Get the working folder
        if (workFolder == null && GameObject.Find("WorkFolder"))
        {
            workFolder = GameObject.Find("WorkFolder").transform;
        }

        //Get the selecting folder
        selectFolder = GameObject.Find("SelectFolder").transform;

        // Get the menu cleaner
        menuCleaner = GameObject.Find("UX_MenuCleaner").transform;

        // Init controllers
        if (leftBrush == null || rightBrush == null)
        {
            InitControllers();
        }

        // Init Sketching menu
        InitSketchingMenu();

        // Init the symmetry mode
        InitSymmetryMode();
    }

    // Update is called once per frame
    void Update()
    {
        // DEBUG ONLY
        /*if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            DisplaySketchFoldersByColor(true);
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            DisplaySketchFoldersByColor(false);
        }*/

        // Read the sketching function if the user pressed the trigger
        if (isSketching == true && isSelectingObject == false)
        {
            IsPressingTrigger();
        }
        else if (isSketching == true && isSelectingObject == true)
        {
            Transform New_Object = rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject();
            if (TheLastSelectedObject != New_Object && New_Object != null)
            {
                Debug.Log("New_Object=     " + New_Object);
                //Debug.Log("TheLastSelectedObject       " + TheLastSelectedObject);
                IsPressingTrigger();
            }
        }


    }



    #region INIT the SKETCH MANAGER (Controllers, Menu & Controllers elements)
    public void InitManager()
    {
        // Init controllers
        if (leftBrush == null || rightBrush == null)
        {
            InitControllers();
        }

        // Init Sketching menu
        InitSketchingMenu();

        // Init the symmetry mode
        InitSymmetryMode();
    }

    /// <summary>
    /// Init controllers, pointers & elements
    /// </summary>
    private void InitControllers()
    {
        if (GameObject.FindGameObjectWithTag("LeftController"))
        {
            // Get the left controller
            leftController = GameObject.FindGameObjectWithTag("LeftController").transform;

            // Init the pointer on left controller
            leftPointer = Instantiate(newPointer, new Vector3(0, 0, 0), Quaternion.identity);
            leftPointer.name = "Sketcher_LeftPointer";
            leftPointer.transform.parent = leftController;
            leftPointer.transform.localPosition = new Vector3(0, 0, 0); // Only for HTC Vive
            leftPointer.SetActive(false);

            // Get the sketcher tool on left controller
            for (int x = 0; x < leftController.childCount; x++)
            {
                if (leftController.GetChild(x).name == "Controller_Sketcher")
                {
                    leftSketchStarter = leftController.GetChild(x);
                }
            }

            // Get list of helpers on left controller
            for (int x = 0; x < leftController.childCount; x++)
            {
                if (leftController.GetChild(x).name == "Controller_Helper")
                {
                    mainHelperOnLeftController = leftController.GetChild(x);

                    for (int y = 0; y < mainHelperOnLeftController.GetChild(0).childCount; y++)
                    {
                        leftHelpers.Add(mainHelperOnLeftController.GetChild(0).GetChild(y));
                    }
                }
            }

            // Get the audio source on left controller
            for (int y = 0; y < leftSketchStarter.childCount; y++)
            {
                if (leftSketchStarter.GetChild(y).name.StartsWith("AudioSource"))
                {
                    leftAudioSource = leftSketchStarter.GetChild(y);
                }
            }

            // Get the brush & the eraser on left controller
            for (int y = 0; y < leftSketchStarter.childCount; y++)
            {
                if (leftSketchStarter.GetChild(y).name == "Brush")
                {
                    leftBrush = leftSketchStarter.GetChild(y);
                    leftBrush.localScale = new Vector3(sketchSize * 200, sketchSize * 200, sketchSize * 200);
                    leftBrush.gameObject.SetActive(true);

                    leftBrushEffect = leftBrush.GetComponentInChildren<ParticleSystem>();
                    leftBrushEffect.Stop();

                }
                else if (leftSketchStarter.GetChild(y).name == "Eraser")
                {
                    leftEraser = leftSketchStarter.GetChild(y);
                    leftEraser.localScale = new Vector3(sketchSize * 200, sketchSize * 200, sketchSize * 200);
                    leftEraser.gameObject.SetActive(false);

                    leftEraserEffect = leftEraser.GetComponentInChildren<ParticleSystem>();
                    leftEraserEffect.Stop();
                }
                else if (leftSketchStarter.GetChild(y).name == "GroupSelector")
                {
                    leftGroupSelector = leftSketchStarter.GetChild(y);
                    leftGroupSelector.gameObject.SetActive(false);
                }

                else if (leftSketchStarter.GetChild(y).name == "ObjectSelector")
                {
                    leftObjectSelector = leftSketchStarter.GetChild(y);
                    leftObjectSelector.gameObject.SetActive(false);
                }

            }
        }

        if (GameObject.FindGameObjectWithTag("RightController"))
        {
            // Get the right controller
            rightController = GameObject.FindGameObjectWithTag("RightController").transform;

            // Init the pointer on the right controller
            rightPointer = Instantiate(newPointer, new Vector3(0, 0, 0), Quaternion.identity);
            rightPointer.name = "Sketcher_RightPointer";
            rightPointer.transform.parent = rightController;
            rightPointer.transform.localPosition = new Vector3(0, 0, 0); // Only for HTC Vive
            rightPointer.SetActive(false);

            // Get the sketcher tool on right controller
            for (int x = 0; x < rightController.childCount; x++)
            {
                if (rightController.GetChild(x).name == "Controller_Sketcher")
                {
                    rightSketchStarter = rightController.GetChild(x);
                }
            }

            // Get list of helpers on right controller
            for (int x = 0; x < rightController.childCount; x++)
            {
                if (rightController.GetChild(x).name == "Controller_Helper")
                {
                    mainHelperOnRightController = rightController.GetChild(x);

                    for (int y = 0; y < mainHelperOnRightController.GetChild(0).childCount; y++)
                    {
                        rightHelpers.Add(mainHelperOnRightController.GetChild(0).GetChild(y));
                    }
                }
            }

            // Get the audio source on right controller
            for (int y = 0; y < rightSketchStarter.childCount; y++)
            {
                if (rightSketchStarter.GetChild(y).name.StartsWith("AudioSource"))
                {
                    rightAudioSource = rightSketchStarter.GetChild(y);
                }
            }

            // Get the brush & the eraser on right controller
            for (int y = 0; y < rightSketchStarter.childCount; y++)
            {
                if (rightSketchStarter.GetChild(y).name == "Brush")
                {
                    rightBrush = rightSketchStarter.GetChild(y);
                    rightBrush.localScale = new Vector3(sketchSize * 200, sketchSize * 200, sketchSize * 200);
                    rightBrush.gameObject.SetActive(true);

                    rightBrushEffect = rightBrush.GetComponentInChildren<ParticleSystem>();
                    rightBrushEffect.Stop();
                }
                else if (rightSketchStarter.GetChild(y).name == "Eraser")
                {
                    rightEraser = rightSketchStarter.GetChild(y);
                    rightEraser.localScale = new Vector3(sketchSize * 200, sketchSize * 200, sketchSize * 200);
                    rightEraser.gameObject.SetActive(false);

                    rightEraserEffect = rightEraser.GetComponentInChildren<ParticleSystem>();
                    rightEraserEffect.Stop();
                }
                else if (rightSketchStarter.GetChild(y).name == "GroupSelector")
                {
                    rightGroupSelector = rightSketchStarter.GetChild(y);
                    rightGroupSelector.gameObject.SetActive(false);
                }

                else if (rightSketchStarter.GetChild(y).name == "ObjectSelector")
                {
                    rightObjectSelector = rightSketchStarter.GetChild(y);
                    rightObjectSelector.gameObject.SetActive(false);
                }
            }
        }

        if (GameObject.FindGameObjectWithTag("MenuLookAt"))
        {
            headset = GameObject.FindGameObjectWithTag("MenuLookAt").transform;
        }

        //Debug.Log("<b>[SketchManager]</b> Initialized");

        DisableHelp();
    }

    /// <summary>
    /// Init the sketching menu
    /// </summary>
    private void InitSketchingMenu()
    {
        sketchingMenu = Instantiate(newSketchingMenu, new Vector3(0, -5, 0), Quaternion.identity);
        sketchingMenu.name = "Sketcher_Menu";
        sketchingMenu.transform.position = new Vector3(0, -5, 0);
        sketchingMenu.transform.localScale = new Vector3(0, 0, 0);
    }

    /// <summary>
    /// Init the symmetry mode
    /// </summary>
    private void InitSymmetryMode()
    {
        if (mySymmetryPlane == null)
        {
            symmetryMode = false;

            // Init the symmetry plane
            mySymmetryPlane = Instantiate(newSymmetryPlane, new Vector3(0, 0, 0), Quaternion.identity);
            mySymmetryPlane.name = "SymmetryPlane";
            mySymmetryPlane.transform.parent = workFolder;
            mySymmetryPlane.transform.position = new Vector3(0, 1, 0);
            mySymmetryPlane.transform.eulerAngles = new Vector3(90, 0, 0);
            mySymmetryPlane.transform.localScale = new Vector3(1, 1, 1);
            mySymmetryPlane.SetActive(false);

            // Init the symmetry left brush
            symmetryLeftBrush = new GameObject();
            symmetryLeftBrush.name = "SymmetryLeftBrush";
            symmetryLeftBrush.transform.position = new Vector3(0, 0, 0);
            symmetryLeftBrush.transform.eulerAngles = new Vector3(0, 0, 0);
            symmetryLeftBrush.transform.localScale = new Vector3(1, 1, 1);

            // Init the symmetry right brush
            symmetryRightBrush = new GameObject();
            symmetryRightBrush.name = "SymmetryRightBrush";
            symmetryRightBrush.transform.position = new Vector3(0, 0, 0);
            symmetryRightBrush.transform.eulerAngles = new Vector3(0, 0, 0);
            symmetryRightBrush.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    #endregion


    #region ENABLE or DISABLE the SKETCH MANAGER
    /// <summary>
    /// Enable the Sketching mode
    /// </summary>
    public void EnableManager()
    {
        // Enable menu
        /*sketchingMenu.transform.localScale = new Vector3(0, 0, 0);
        sketchingMenu.transform.position = new Vector3(0, -5, 0);*/

        leftBrush.gameObject.SetActive(true);
        leftBrushEffect.Stop();
        rightBrush.gameObject.SetActive(true);
        rightBrushEffect.Stop();

        leftEraser.gameObject.SetActive(false);
        leftEraserEffect.Stop();
        rightEraser.gameObject.SetActive(false);
        rightEraserEffect.Stop();

        leftGroupSelector.gameObject.SetActive(false);
        rightGroupSelector.gameObject.SetActive(false);

        leftObjectSelector.gameObject.SetActive(false);
        rightObjectSelector.gameObject.SetActive(false);

        // Open menu
        OpenSketchingMenu(_ActionsManager.GetComponent<ActionsManager>().WasRightHander());
    }

    /// <summary>
    /// Disable the Sketching mode
    /// </summary>
    public void DisableManager()
    {
        // Stop the sketching action
        UnpressTriggger();

        // Disable menu
        sketchingMenu.transform.localScale = new Vector3(0, 0, 0);
        sketchingMenu.transform.position = new Vector3(0, -5, 0);
        sketchingMenuIsOpen = false;

        // Disable pointer
        leftPointer.gameObject.SetActive(false);
        rightPointer.gameObject.SetActive(false);

        // Disable elements
        leftBrush.gameObject.SetActive(false);
        rightBrush.gameObject.SetActive(false);
        leftEraser.gameObject.SetActive(false);
        rightEraser.gameObject.SetActive(false);
        leftGroupSelector.gameObject.SetActive(false);
        rightGroupSelector.gameObject.SetActive(false);

        leftObjectSelector.gameObject.SetActive(false);
        rightObjectSelector.gameObject.SetActive(false);
        /*if (sketchingMenuIsOpen == true)
        {
            CloseSketchingMenu();
        }*/
    }
    #endregion


    #region INTERACTIONS with CONTROLLERS (Trigger) (Sketching, Erasing, Select group...)
    /// <summary>
    /// Start a new sketch
    /// </summary>
    public void PressTrigger(bool myPlayerHander)
    {
        // Si le menu de dessin n'est pas ouvert
        if (sketchingMenuIsOpen == false)
        {
            // Si le mode "SKETCH" est activé
            if (currentMode.StartsWith("Sketch"))
            {

                // Si le joueur n'est pas déjà en train de dessiner
                if (isSketching == false)
                {
                    // Get the player hander
                    rightHander = myPlayerHander;

                    // Init controllers
                    if (leftBrush == null || rightBrush == null)
                    {
                        InitControllers();
                    }

                    // Instantiate the sketch
                    mySketch = Instantiate(sketchPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    mySketch.name = "Sketch_" + sketchId;
                    sketchId++;

                    myLineRenderer = mySketch.GetComponent<LineRenderer>();

                    // Get the color & the size from the brush
                    if (rightHander == true)
                    {
                        myLineRenderer.material.color = rightBrush.GetComponent<MeshRenderer>().material.color;
                        sketchSize = rightBrush.transform.localScale.x / 200;
                        rightBrushEffect.Play();
                    }
                    else
                    {
                        myLineRenderer.material.color = leftBrush.GetComponent<MeshRenderer>().material.color;
                        sketchSize = leftBrush.transform.localScale.x / 200;
                        leftBrushEffect.Play();
                    }

                    // Set the color of the sketch
                    myLineRenderer.startColor = Color.white;
                    myLineRenderer.endColor = Color.white;

                    // Set the size of the sketch
                    myLineRenderer.startWidth = sketchSize;
                    myLineRenderer.endWidth = sketchSize;

                    // Display a smooth line (start & end width)
                    AnimationCurve curve = new AnimationCurve();
                    curve.AddKey(0.0f, 0.0f);
                    curve.AddKey(0.1f, sketchSize);
                    curve.AddKey(0.9f, sketchSize);
                    curve.AddKey(1.0f, 0.0f);
                    myLineRenderer.widthCurve = curve;

                    // Si le joueur travaille en symétrie
                    if (symmetryMode == true)
                    {
                        // Instantiate the sketch
                        mySymmetricSketch = Instantiate(sketchPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        mySymmetricSketch.name = "Sketch_" + sketchId;
                        sketchId++;
                        mySymmetricLineRenderer = mySymmetricSketch.GetComponent<LineRenderer>();

                        // Get the color & the size from the brush
                        if (rightHander == true)
                        {
                            mySymmetricLineRenderer.material.color = rightBrush.GetComponent<MeshRenderer>().material.color;
                            sketchSize = rightBrush.transform.localScale.x / 200;
                        }
                        else
                        {
                            mySymmetricLineRenderer.material.color = leftBrush.GetComponent<MeshRenderer>().material.color;
                            sketchSize = leftBrush.transform.localScale.x / 200;
                        }

                        // Set the color of the sketch
                        mySymmetricLineRenderer.startColor = Color.white;
                        mySymmetricLineRenderer.endColor = Color.white;

                        // Set the size of the sketch
                        mySymmetricLineRenderer.startWidth = sketchSize;
                        mySymmetricLineRenderer.endWidth = sketchSize;

                        // Display a smooth line (start & end width)
                        mySymmetricLineRenderer.widthCurve = curve;
                    }

                    // Init the number of points in the sketch
                    pointsOnMySketch = 0;

                    // Début de lecture du fichier audio
                    if (rightHander == true)
                    {
                        rightAudioSource.GetComponent<AudioSource>().Play();
                    }
                    else
                    {
                        leftAudioSource.GetComponent<AudioSource>().Play();
                    }

                    isSketching = true;
                }
            }

            // Si le mode "ERASER" est activé
            else if (currentMode.StartsWith("Eraser"))
            {
                // Si le joueur n'est pas déjà en train de dessiner
                if (isSketching == false)
                {
                    // Get the player hander
                    rightHander = myPlayerHander;

                    // Init controllers
                    if (leftBrush == null || rightBrush == null)
                    {
                        InitControllers();
                    }

                    // Send info to the erasing trigger
                    if (rightHander == true)
                    {
                        leftEraser.GetComponent<ErasingTrigger>().IsErasing(false);
                        rightEraser.GetComponent<ErasingTrigger>().IsErasing(true);
                        rightEraserEffect.Play();
                    }
                    else
                    {
                        rightEraser.GetComponent<ErasingTrigger>().IsErasing(false);
                        leftEraser.GetComponent<ErasingTrigger>().IsErasing(true);
                        leftEraserEffect.Play();
                    }

                    // Instantiate the sketch
                    mySketch = Instantiate(eraserPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    mySketch.name = "Eraser";
                    myLineRenderer = mySketch.GetComponent<LineRenderer>();

                    // Set the size of the sketch
                    myLineRenderer.startWidth = eraseSize;
                    myLineRenderer.endWidth = eraseSize;

                    // Display a smooth line (start & end width)
                    AnimationCurve curve = new AnimationCurve();
                    curve.AddKey(0.0f, 0.0f);
                    curve.AddKey(0.1f, eraseSize);
                    curve.AddKey(0.9f, eraseSize);
                    curve.AddKey(1.0f, 0.0f);
                    myLineRenderer.widthCurve = curve;

                    // Init the number of points in the sketch
                    pointsOnMySketch = 0;

                    // Début de lecture du fichier audio
                    if (rightHander == true)
                    {
                        rightAudioSource.GetComponent<AudioSource>().Play();
                    }
                    else
                    {
                        leftAudioSource.GetComponent<AudioSource>().Play();
                    }

                    isErasing = true;
                    isSketching = true;
                }
            }

            // Si le mode "SELECT GROUP" est activé
            else if (currentMode.StartsWith("SelectGroup"))
            {
                // Si le joueur n'est pas déjà en train de dessiner
                if (isSelectingGroup == true)
                {
                    // Get the player hander
                    rightHander = myPlayerHander;

                    //Debug.Log("Check >>> " + rightGroupSelector.name + " " + leftGroupSelector);

                    // Get the selected group
                    if (rightHander == true && rightGroupSelector.GetComponent<SelectGroupTrigger>().GetSelectedGroup() != null)
                    {
                        mySketchFolder = rightGroupSelector.GetComponent<SelectGroupTrigger>().GetSelectedGroup().gameObject;
                    }
                    else if (rightHander == false && leftGroupSelector.GetComponent<SelectGroupTrigger>().GetSelectedGroup() != null)
                    {
                        mySketchFolder = leftGroupSelector.GetComponent<SelectGroupTrigger>().GetSelectedGroup().gameObject;
                    }
                    else
                    {
                        OpenSketchingMenu(rightHander);
                    }

                    //Debug.Log("Select : " + mySketchFolder);

                    // Init all groups colors
                    DisplaySketchFoldersByColor(false);

                    // Stop the group selection tool
                    rightGroupSelector.GetComponent<SelectGroupTrigger>().IsSelectingGroup(false);
                    leftGroupSelector.GetComponent<SelectGroupTrigger>().IsSelectingGroup(false);
                    isSelectingGroup = false;

                    Debug.Log("<b>[SketchManager]</b> " + mySketchFolder.name + " selected");

                    UpdateMode("Sketch");
                }
            }

            // Si le mode 
            else if (currentMode.StartsWith("SelectObject"))
            {
                // Si le joueur n'est pas déjà en train de dessiner
                if (isSelectingObject == false && isSketching == false)
                {
                    // Get the player hander
                    rightHander = myPlayerHander;

                    rightObjectSelector.GetComponent<SelectObjectTrigger>().CptTrigger();

                    //Debug.Log("Check >>> " + rightGroupSelector.name + " " + leftGroupSelector);

                    // Get the selected group
                    if (rightHander == true && rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject() != null)
                    {

                        string i_folder = rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().parent.name;
                        string j_sketch = rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().name;
                        string num_i = null;
                        string num_j = null;

                        foreach (char item in i_folder)
                        {
                            if (item >= 48 && item <= 58)
                            {
                                num_i += item;
                            }
                        }

                        i = int.Parse(num_i);

                        foreach (char item in j_sketch)
                        {
                            if (item >= 48 && item <= 58)
                            {
                                num_j += item;
                            }
                        }

                        j = int.Parse(num_j);

                        Debug.Log("presstrigger  i j =======" + num_i + "   and   " + num_j);

                        TheLastSelectedObject = rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject();

                        if (currentSelectedObjects[i, j] == null)
                        {
                            currentSelectedObjects[i, j] = rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject();
                            rightObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(true);


                            string mySelectfoldername = "SelectSketchFolder_" + i;
                            if (selectFolder.Find(mySelectfoldername) == null)
                            {
                                CreateNewSelectFolder();
                            }
                            currentSelectedObjects[i, j].transform.parent = selectFolder.Find(mySelectfoldername).transform;

                        }
                        else
                        {
                            string foldername = "SketchFolder_" + i;
                            currentSelectedObjects[i, j].transform.parent = workFolder.Find(foldername).transform;

                            currentSelectedObjects[i, j] = null;
                            rightObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(true);

                        }
                        //mySelectObjectFolder = rightGroupSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().gameObject;

                        Debug.Log("presstrigger currentSelectedObjects       " + currentSelectedObjects[0, 1]);
                    }

                    else if (rightHander == false && leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject() != null)
                    {
                        string i_folder = leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().parent.name;
                        string j_sketch = leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().name;
                        string num_i = null;
                        string num_j = null;

                        foreach (char item in i_folder)
                        {
                            if (item >= 48 && item <= 58)
                            {
                                num_i += item;
                            }
                        }

                        i = int.Parse(num_i);

                        foreach (char item in j_sketch)
                        {
                            if (item >= 48 && item <= 58)
                            {
                                num_j += item;
                            }
                        }

                        j = int.Parse(num_j);

                        TheLastSelectedObject = leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject();

                        if (currentSelectedObjects[i, j] == null)
                        {
                            currentSelectedObjects[i, j] = leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject();
                            leftObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(true);

                            string mySelectfoldername = "SelectSketchFolder_" + i;
                            if (selectFolder.Find(mySelectfoldername) == null)
                            {
                                CreateNewSelectFolder();
                            }
                            currentSelectedObjects[i, j].transform.parent = selectFolder.Find(mySelectfoldername).transform;


                        }
                        else
                        {
                            string foldername = "SketchFolder_" + i;
                            currentSelectedObjects[i, j].transform.parent = workFolder.Find(foldername).transform;

                            leftObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(true);
                            currentSelectedObjects[i, j] = null;
                        }
                        //mySelectObjectFolder = leftGroupSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().gameObject;
                    }
                    else
                    {
                        leftObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(false);
                        rightObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(false);

                        rightObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(false);
                        leftObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(false);

                        OpenSketchingMenu(rightHander);
                    }

                    isSelectingObject = true;
                    isSketching = true;
                }
            }
        }

        // Si le menu de dessin est ouvert
        else if (sketchingMenuIsOpen == true)
        {
            // Update the sketching mode
            //UpdateSketchingTool();

            if (myPlayerHander == rightHander)
            {
                UpdateMode("");
                rightHander = myPlayerHander;
            }
            else
            {
                //CloseSketchingMenu();
            }
        }

        //rightHander = myPlayerHander;
    }

    /// <summary>
    /// User is sketching
    /// </summary>
    public void IsPressingTrigger()
    {
        if (isSketching == true && isSelectingObject == false)
        {
            // Ajout des points à la ligne
            myLineRenderer.positionCount = pointsOnMySketch + 1;

            if (rightHander == true)
            {
                myLineRenderer.SetPosition(pointsOnMySketch, rightBrush.transform.position);
            }
            else
            {
                myLineRenderer.SetPosition(pointsOnMySketch, leftBrush.transform.position);
            }

            // Si le joueur travaille en symétrie
            if (symmetryMode == true && isErasing == false)
            {
                // Ajout des points à la ligne
                mySymmetricLineRenderer.positionCount = pointsOnMySketch + 1;

                if (rightHander == true)
                {
                    // Calcul de la symétrie de la brosse
                    Vector3 planeNormal_RightBrush = Vector3.Dot((mySymmetryPlane.transform.position - rightBrush.transform.position), mySymmetryPlane.transform.up) * mySymmetryPlane.transform.up;
                    Vector3 rightProjection = rightBrush.transform.position + planeNormal_RightBrush * 2;
                    symmetryRightBrush.transform.position = rightProjection;

                    // Mise à jour de la position du point symétrique
                    mySymmetricLineRenderer.SetPosition(pointsOnMySketch, symmetryRightBrush.transform.position);
                }
                else
                {
                    // Calcul de la symétrie de la brosse
                    Vector3 planeNormal_LeftBrush = Vector3.Dot((mySymmetryPlane.transform.position - leftBrush.transform.position), mySymmetryPlane.transform.up) * mySymmetryPlane.transform.up;
                    Vector3 leftProjection = leftBrush.transform.position + planeNormal_LeftBrush * 2;
                    symmetryLeftBrush.transform.position = leftProjection;

                    // Mise à jour de la position du point symétrique
                    mySymmetricLineRenderer.SetPosition(pointsOnMySketch, symmetryLeftBrush.transform.position);
                }
            }

            pointsOnMySketch++;

            // Stop the sketching process if the user is drawing the same sketch during 10 seconds
            if (sketchTimer < 10)
            {
                sketchTimer += Time.deltaTime;
            }
            else
            {
                sketchTimer = 0;
                UnpressTriggger();
            }
        }
        else if (isSketching == true && isSelectingObject == true)
        {
            // Get the player hander
            //rightHander = myPlayerHander;

            //Debug.Log("Check >>> " + rightGroupSelector.name + " " + leftGroupSelector);

            // Get the selected group
            if (rightHander == true && rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject() != null)
            {
                string i_folder = rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().parent.name;
                string j_sketch = rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().name;
                string num_i = null;
                string num_j = null;

                foreach (char item in i_folder)
                {
                    if (item >= 48 && item <= 58)
                    {
                        num_i += item;
                    }
                }

                i = int.Parse(num_i);

                foreach (char item in j_sketch)
                {
                    if (item >= 48 && item <= 58)
                    {
                        num_j += item;
                    }
                }

                j = int.Parse(num_j);

                Debug.Log("pressing  i j =======" + num_i + "   and   " + num_j);

                TheLastSelectedObject = rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject();

                if (currentSelectedObjects[i, j] == null)
                {
                    currentSelectedObjects[i, j] = rightObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject();
                    rightObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(true);
                    //currentSelectedObjects[i, j].GetComponent<LineRenderer>().material.color = Color.white;

                    string mySelectfoldername = "SelectSketchFolder_" + i;
                    if (selectFolder.Find(mySelectfoldername) == null)
                    {
                        CreateNewSelectFolder();
                    }
                    currentSelectedObjects[i, j].transform.parent = selectFolder.Find(mySelectfoldername).transform;

                }
                else
                {
                    string foldername = "SketchFolder_" + i;
                    currentSelectedObjects[i, j].transform.parent = workFolder.Find(foldername).transform;
                    //currentSelectedObjects[i, j].GetComponent<Sketch>().ResetColor();
                    currentSelectedObjects[i, j] = null;
                    rightObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(true);
                }
                Debug.Log("pressing currentSelectedObjects       " + currentSelectedObjects[0, 1]);

            }

            else if (rightHander == false && leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject() != null)
            {
                string i_folder = leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().parent.name;
                string j_sketch = leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().name;
                string num_i = null;
                string num_j = null;

                foreach (char item in i_folder)
                {
                    if (item >= 48 && item <= 58)
                    {
                        num_i += item;
                    }
                }

                i = int.Parse(num_i);

                foreach (char item in j_sketch)
                {
                    if (item >= 48 && item <= 58)
                    {
                        num_j += item;
                    }
                }

                j = int.Parse(num_j);

                TheLastSelectedObject = leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject();

                if (currentSelectedObjects[i, j] == null)
                {

                    currentSelectedObjects[i, j] = leftObjectSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject();
                    //currentSelectedObjects[i, j].GetComponent<LineRenderer>().material.color = Color.white;
                    leftObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(true);

                    string mySelectfoldername = "SelectSketchFolder_" + i;
                    if (selectFolder.Find(mySelectfoldername) == null)
                    {
                        CreateNewSelectFolder();
                    }
                    currentSelectedObjects[i, j].transform.parent = selectFolder.Find(mySelectfoldername).transform;

                }
                else
                {
                    string foldername = "SketchFolder_" + i;
                    currentSelectedObjects[i, j].transform.parent = workFolder.Find(foldername).transform;

                    currentSelectedObjects[i, j] = null;
                    leftObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(true);
                }
                //mySelectObjectFolder = leftGroupSelector.GetComponent<SelectObjectTrigger>().GetSelectedObject().gameObject;
            }



        }
    }

    /// <summary>
    /// Stop the current sketch
    /// </summary>
    public void UnpressTriggger()
    {
        // Si l'utilisateur est en train de dessiner
        if (isSketching == true)
        {

            // Fin de lecture du fichier audio
            leftAudioSource.GetComponent<AudioSource>().Stop();
            rightAudioSource.GetComponent<AudioSource>().Stop();

            // Si le mode "SKETCH" est activé
            if (currentMode.StartsWith("Sketch"))
            {
                // Arrêt de l'effet
                leftBrushEffect.Stop();
                rightBrushEffect.Stop();

                // Optimisation de la courbe
                int pointsWithoutOpti = myLineRenderer.positionCount;
                if (pointsWithoutOpti > 0)
                {
                    // 1 -> Réduction du nombre de points sur la ligne
                    List<Vector3> points = new List<Vector3>();
                    for (int x = 0; x < myLineRenderer.positionCount; x++)
                    {
                        points.Add(myLineRenderer.GetPosition(x));
                    }
                    float tolerance = 0.01f; //Valeur initiale = 0.0008f;
                    List<Vector3> simplifiedPoints = new List<Vector3>();
                    LineUtility.Simplify(points.ToList(), tolerance, simplifiedPoints);
                    myLineRenderer.positionCount = simplifiedPoints.Count;
                    myLineRenderer.SetPositions(simplifiedPoints.ToArray());
                    points.Clear();

                    // 2 -> Lissage de la courbe
                    Vector3[] linePositions = simplifiedPoints.ToArray();
                    for (int i = 0; i < simplifiedPoints.Count; i++)
                    {
                        linePositions[i] = simplifiedPoints[i];
                    }
                    simplifiedPoints.Clear();
                    Vector3[] smoothedPoints = Sketch_LineSmoother.SmoothLine(linePositions, 0.01f);//0.005f //0.010f ou 0.012f
                    linePositions = null;

                    // 3 -> Mise à jour de la courbe
                    myLineRenderer.positionCount = smoothedPoints.Length;
                    myLineRenderer.SetPositions(smoothedPoints);

                    // TEST TubeRenderer
                    /*GameObject myTube = Instantiate(tubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    myTube.GetComponent<TubeRenderer>().SetPoints(smoothedPoints, 0.01f, Color.cyan);*/

                    smoothedPoints = null;

                    // Open a color menu if user creates a sketch point
                    //if (myLineRenderer.positionCount <= 2)
                    if (myLineRenderer.positionCount <= 3 || (myLineRenderer.positionCount <= 5 && sketchTimer < 0.5f))
                    {
                        // Destroy the current sketch
                        Destroy(mySketch);

                        // Open the sketching menu
                        if (sketchingMenuIsOpen == false)
                        {
                            OpenSketchingMenu(rightHander);
                        }

                        //Debug.Log("<b>[SketchManager]</b> ----- Open sketching menu -----");
                    }
                    else
                    {
                        // Create the collider
                        if (mySketch.GetComponent<MeshCollider>() == null)
                        {
                            mySketch.GetComponent<Sketch_LineCollider>().CreateColliderFromLine(myLineRenderer);
                        }

                        // Assignation du parent
                        if (mySketchFolder == null)
                        {
                            CreateNewSketchFolder();
                        }

                        // Envoi de l'échelle lors de la création

                        mySketch.GetComponent<Sketch>().SetSketchSettings(workFolder.localScale.x, sketchSize, myLineRenderer.material.color);
                        //mySketch.GetComponent<Sketch>().SetSketchSettings_V2(workFolder.localScale.x, sketchSize, myLineRenderer.material.color, mySketch.transform.localScale.x);

                        mySketch.transform.parent = mySketchFolder.transform;

                        mySketch.GetComponent<Sketch>().SetSketchSettings_V2(mySketch.transform.localScale.x);

                        // Send action to history
                        _HistoryManager.GetComponent<HistoryManager>().AddAction_Z("Create a sketch", mySketch.transform);

                        // Try to create a tube
                        /*mySketch.transform.GetChild(0).gameObject.AddComponent<TubeRenderer>();
                        mySketch.transform.GetComponentInChildren<TubeRenderer>().Init();
                        mySketch.GetComponent<LineRenderer>().enabled = false;*/

                        // Try to create a pipe
                        /*PipeMeshGenerator pmg = mySketch.GetComponentInChildren<PipeMeshGenerator>();

                        for (int i = 0; i < mySketch.GetComponent<LineRenderer>().positionCount; i++)
                        {
                            Debug.Log(mySketch.GetComponent<LineRenderer>().GetPosition(i));
                            pmg.points.Add(mySketch.GetComponent<LineRenderer>().GetPosition(i));
                        }

                        pmg.RenderPipe();
                        mySketch.GetComponent<LineRenderer>().enabled = false;*/

                        Debug.Log("<b>[SketchManager]</b> New SKETCH created (" + mySketch.name + ") (" + myLineRenderer.positionCount + " points)");
                    }
                }

                // Si le joueur travaille en symétrie
                if (symmetryMode == true)
                {
                    if (pointsWithoutOpti > 0)
                    {
                        // 1 -> Réduction du nombre de points sur la ligne
                        List<Vector3> points = new List<Vector3>();
                        for (int x = 0; x < mySymmetricLineRenderer.positionCount; x++)
                        {
                            points.Add(mySymmetricLineRenderer.GetPosition(x));
                        }
                        float tolerance = 0.01f; //Valeur initiale = 0.0008f;
                        List<Vector3> simplifiedPoints = new List<Vector3>();
                        LineUtility.Simplify(points.ToList(), tolerance, simplifiedPoints);
                        mySymmetricLineRenderer.positionCount = simplifiedPoints.Count;
                        mySymmetricLineRenderer.SetPositions(simplifiedPoints.ToArray());
                        points.Clear();

                        // 2 -> Lissage de la courbe
                        Vector3[] linePositions = simplifiedPoints.ToArray();
                        for (int i = 0; i < simplifiedPoints.Count; i++)
                        {
                            linePositions[i] = simplifiedPoints[i];
                        }
                        simplifiedPoints.Clear();
                        Vector3[] smoothedPoints = Sketch_LineSmoother.SmoothLine(linePositions, 0.005f);//0.010f ou 0.012f
                        linePositions = null;

                        // 3 -> Mise à jour de la courbe
                        mySymmetricLineRenderer.positionCount = smoothedPoints.Length;
                        mySymmetricLineRenderer.SetPositions(smoothedPoints);

                        // TEST TubeRenderer
                        /*GameObject myTube = Instantiate(tubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        myTube.GetComponent<TubeRenderer>().SetPoints(smoothedPoints, 0.01f, Color.cyan);*/

                        smoothedPoints = null;

                        // Open a color menu if user creates a sketch point
                        //if (mySymmetricLineRenderer.positionCount <= 2)
                        if (myLineRenderer.positionCount <= 3 || (myLineRenderer.positionCount <= 5 && sketchTimer < 0.5f))
                        {
                            // Destroy the current sketch
                            Destroy(mySymmetricSketch);

                            // Open the sketching menu
                            /*if (sketchingMenuIsOpen == false)
                            {
                                OpenSketchingMenu(rightHander);
                            }*/

                            //Debug.Log("<b>[SketchManager]</b> Open sketching menu");
                        }
                        else
                        {
                            // Create the collider
                            if (mySymmetricSketch.GetComponent<MeshCollider>() == null)
                            {
                                mySymmetricSketch.GetComponent<Sketch_LineCollider>().CreateColliderFromLine(mySymmetricLineRenderer);
                            }

                            // Assignation du parent
                            if (mySketchFolder == null)
                            {
                                CreateNewSketchFolder();
                            }

                            // Envoi de l'échelle lors de la création
                            /*mySketch.GetComponent<Sketch>().SetInitialScale(workFolder.localScale.x);
                            mySketch.GetComponent<Sketch>().SetInitialColor(myLineRenderer.material.color);*/
                            mySymmetricSketch.GetComponent<Sketch>().SetSketchSettings(workFolder.localScale.x, sketchSize, mySymmetricLineRenderer.material.color);

                            mySymmetricSketch.transform.parent = mySketchFolder.transform;

                            // Send action to history
                            _HistoryManager.GetComponent<HistoryManager>().AddAction_Z("Create a sketch (symmetric)", mySymmetricSketch.transform);

                            Debug.Log("<b>[SketchManager]</b> New SYMMETRIC SKETCH created (" + mySymmetricSketch.name + ") (" + mySymmetricLineRenderer.positionCount + " points)");
                        }
                    }
                }

                pointsOnMySketch = 0;
                isSketching = false;
            }

            // Si le mode "ERASER" est activé
            else if (currentMode.StartsWith("Eraser"))
            {
                // Arrêt de l'effet
                leftEraserEffect.Stop();
                rightEraserEffect.Stop();

                // Send info to the erasing trigger
                rightEraser.GetComponent<ErasingTrigger>().IsErasing(false);
                leftEraser.GetComponent<ErasingTrigger>().IsErasing(false);

                // Optimisation de la courbe
                int pointsWithoutOpti = myLineRenderer.positionCount;
                if (pointsWithoutOpti > 0)
                {
                    // 1 -> Réduction du nombre de points sur la ligne
                    List<Vector3> points = new List<Vector3>();
                    for (int x = 0; x < myLineRenderer.positionCount; x++)
                    {
                        points.Add(myLineRenderer.GetPosition(x));
                    }
                    float tolerance = 0.01f; //Valeur initiale = 0.0008f;
                    List<Vector3> simplifiedPoints = new List<Vector3>();
                    LineUtility.Simplify(points.ToList(), tolerance, simplifiedPoints);
                    myLineRenderer.positionCount = simplifiedPoints.Count;
                    myLineRenderer.SetPositions(simplifiedPoints.ToArray());
                    points.Clear();

                    // 2 -> Lissage de la courbe
                    Vector3[] linePositions = simplifiedPoints.ToArray();
                    for (int i = 0; i < simplifiedPoints.Count; i++)
                    {
                        linePositions[i] = simplifiedPoints[i];
                    }
                    simplifiedPoints.Clear();
                    Vector3[] smoothedPoints = Sketch_LineSmoother.SmoothLine(linePositions, 0.008f);//0.010f ou 0.012f
                    linePositions = null;

                    // 3 -> Mise à jour de la courbe
                    myLineRenderer.positionCount = smoothedPoints.Length;
                    myLineRenderer.SetPositions(smoothedPoints);

                    smoothedPoints = null;

                    // Open a color menu if user creates a sketch point
                    //if (myLineRenderer.positionCount <= 2)
                    if (myLineRenderer.positionCount <= 3 || (myLineRenderer.positionCount <= 5 && sketchTimer < 0.5f))
                    {
                        // Destroy the current sketch
                        Destroy(mySketch);

                        // Open the sketching menu
                        if (sketchingMenuIsOpen == false)
                        {
                            OpenSketchingMenu(rightHander);
                        }

                        //Debug.Log("<b>[SketchManager]</b> Open sketching menu");
                    }
                    else
                    {
                        // Destroy the current sketch
                        mySketch.GetComponent<Sketch>().EraseSketch();

                        Debug.Log("<b>[SketchManager]</b> New ERASING SKETCH created (" + mySketch.name + ") (" + myLineRenderer.positionCount + " points)");
                    }
                }
                pointsOnMySketch = 0;
                isErasing = false;
                isSketching = false;
            }

            // Si le mode "SelectObject" est activé
            else if (currentMode.StartsWith("SelectObject"))
            {
                _GrabManager.GetComponent<GrabManager>().SetSelectedObjects(selectFolder);

                if (isSelectingObject == true)
                {
                    isSelectingObject = false;
                    isSketching = false;
                }

            }

            // Reset the sketching timer
            sketchTimer = 0;
        }

    }
    #endregion

    #region INTERACTIONS with SKETCHING MENU
    /// <summary>
    /// Open the sketching menu
    /// </summary>
    public void OpenSketchingMenu(bool myPlayerHander)
    {
        if (coroutineIsRunning == false)
        {
            // Disable the GrabManager
            _GrabManager.GetComponent<GrabManager>().DisableGrabbingAction(true);

            // Get the player hander (left or right)
            rightHander = myPlayerHander;

            // Get the current controller used to open the menu
            Transform currentController;
            if (rightHander == true)
            {
                currentController = rightController;
            }
            else
            {
                currentController = leftController;
            }

            // Set the menu in front of the controller
            sketchingMenu.transform.parent = currentController;
            sketchingMenu.transform.localPosition = new Vector3(0, 0, 0.15f); // Only for HTC Vive
            sketchingMenu.transform.parent = null;

            // Menu is looking at the headset
            sketchingMenu.transform.rotation = Quaternion.LookRotation(sketchingMenu.transform.position - headset.position);

            // Clean the area between the controller & the menu
            menuCleaner.GetComponent<MenuCleaner>().CleanMenuArea(currentController, headset);

            // Open the menu
            //sketchingMenu.SetActive(true);
            StartCoroutine(OnSketchingMenu());

            // Enable a pointer to interact with the menu (pointer is only available on the controller used to open the menu)
            if (rightHander == true)
            {
                rightPointer.SetActive(true);
            }
            else
            {
                leftPointer.SetActive(true);
            }

            // Set the current state of the menu
            sketchingMenuIsOpen = true;
        }
    }

    /// <summary>
    /// Update the current mode of the sketching menu
    /// </summary>
    public void UpdateMode(string toolToOpen)
    {
        // Unclean the area between the controller & the menu
        menuCleaner.GetComponent<MenuCleaner>().UncleanMenuArea();

        // Enable the GrabManager
        _GrabManager.GetComponent<GrabManager>().DisableGrabbingAction(false);

        // Set the menu button hit by the user
        Transform hitButton = null;

        // If the current mode is updated by the user with the menu pointer...
        if (toolToOpen == "")
        {
            // Get the hit button
            if (rightHander == true)
            {
                hitButton = rightPointer.GetComponent<SketchingMenu_Pointer>().GetHitButton();
            }
            else
            {
                hitButton = leftPointer.GetComponent<SketchingMenu_Pointer>().GetHitButton();
            }

            // If user clicked inside the menu
            if (hitButton != null)
            {
                currentMode = hitButton.name;
            }
            else
            {
                currentMode = "Sketch";
            }
        }

        // Else if the current tool is updated by script...
        else if (toolToOpen != "")
        {
            currentMode = toolToOpen;
        }

        Debug.Log("<b>[SketchManager]</b> ----- Select : " + currentMode + " -----");

        // Update the current mode :
        switch (currentMode)
        {
            // 1. Back to the Main Manager
            case "Back":
                //currentMode = "Sketch";
                DisableHelp();
                _AppManager.GetComponent<AppManager>().UpdateMode("MainManager");
                break;

            // 2. Load the Project Manager
            case "ProjectManager":
                //currentMode = "Sketch";
                DisableHelp();
                _AppManager.GetComponent<AppManager>().UpdateMode("ProjectManager");
                break;

            // 3. Load the Capture Manager
            case "CaptureManager":
                //currentMode = "Sketch";
                DisableHelp();
                _AppManager.GetComponent<AppManager>().UpdateMode("CaptureManager");
                break;

            // 4. Close the current group & create a new group
            case "Group":

                // Update the help on controller
                UpdateHelp();

                // Create new folder
                CreateNewSketchFolder();

                // Reset the sketching tool by default
                currentMode = "Sketch";
                leftEraser.gameObject.SetActive(false);
                rightEraser.gameObject.SetActive(false);
                leftBrush.gameObject.SetActive(true);
                rightBrush.gameObject.SetActive(true);

                // Close the sketching menu
                CloseSketchingMenu();
                break;

            // 5. Erase a sketch
            case "Eraser":

                // Update the help on controller
                UpdateHelp();

                // Disable the brush & Enable the eraser
                eraseSize = 0.01f;

                rightBrush.gameObject.SetActive(false);
                rightEraser.gameObject.SetActive(true);
                rightEraser.GetComponent<MeshRenderer>().material.color = Color.red;
                rightEraser.localScale = new Vector3(eraseSize * 200, eraseSize * 200, eraseSize * 200);
                rightEraserEffect.Stop();

                var rightPs = rightEraserEffect.main;
                rightPs.startColor = new Color(1, 0.5f, 0);

                leftBrush.gameObject.SetActive(false);
                leftEraser.gameObject.SetActive(true);
                leftEraser.GetComponent<MeshRenderer>().material.color = Color.red;
                leftEraser.localScale = new Vector3(eraseSize * 200, eraseSize * 200, eraseSize * 200);
                leftEraserEffect.Stop();

                var leftPs = leftEraserEffect.main;
                leftPs.startColor = new Color(1, 0.5f, 0);

                // Close the sketching menu
                CloseSketchingMenu();
                break;

            // 6. Enable or disable the symmetry mode
            case "Symmetry":

                // Update the help on controller
                UpdateHelp();

                // Switch the symmetry mode
                symmetryMode = !symmetryMode;

                // Enable or disable the symmetry plane
                if (symmetryMode == false)
                {
                    mySymmetryPlane.SetActive(false);
                }
                else
                {
                    mySymmetryPlane.SetActive(true);
                }

                // Reset the sketching tool by default
                currentMode = "Sketch";

                leftEraser.gameObject.SetActive(false);
                rightEraser.gameObject.SetActive(false);
                leftBrush.gameObject.SetActive(true);
                rightBrush.gameObject.SetActive(true);

                // Close the sketching menu
                CloseSketchingMenu();
                break;

            // 7. Select a group of sketches
            case "SelectGroup":

                // Update the help on controller
                UpdateHelp();

                // Display each sketch folder
                DisplaySketchFoldersByColor(true);

                // Enable the group selector on the controller
                if (rightHander == true)
                {
                    leftGroupSelector.gameObject.SetActive(false);
                    leftGroupSelector.GetComponent<SelectGroupTrigger>().IsSelectingGroup(false);

                    rightGroupSelector.gameObject.SetActive(true);
                    rightGroupSelector.GetComponent<SelectGroupTrigger>().IsSelectingGroup(true);
                }
                else
                {
                    rightGroupSelector.gameObject.SetActive(false);
                    rightGroupSelector.GetComponent<SelectGroupTrigger>().IsSelectingGroup(false);

                    leftGroupSelector.gameObject.SetActive(true);
                    leftGroupSelector.GetComponent<SelectGroupTrigger>().IsSelectingGroup(true);
                }

                isSelectingGroup = true;

                // Close the sketching menu
                CloseSketchingMenu();
                break;

            case "SelectObject":

                // Update the help on controller
                UpdateHelp();

                //CreateNewSelectFolder();

                // Display each sketch folder
                //DisplaySketchFoldersByColor(true);

                // Enable the group selector on the controller
                if (rightHander == true)
                {
                    leftObjectSelector.gameObject.SetActive(false);
                    leftObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(false);

                    rightObjectSelector.gameObject.SetActive(true);
                    rightObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(true);
                }
                else
                {
                    rightObjectSelector.gameObject.SetActive(false);
                    rightObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(false);

                    leftObjectSelector.gameObject.SetActive(true);
                    leftObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(true);
                }

                rightObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(false);
                leftObjectSelector.GetComponent<SelectObjectTrigger>().AffirmObject(false);

                //isSelectingObject = true;

                // Close the sketching menu
                CloseSketchingMenu();
                break;

            // 8. Close the sketching menu
            case "CloseMenu":
                CloseSketchingMenu();
                break;

            // 9. Close the sketching menu
            case "":
                CloseSketchingMenu();

                if (currentMode == "SelectObject")
                {
                    if (rightHander == true)
                    {
                        leftObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(false);
                        rightObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(true);
                    }
                    else
                    {
                        rightObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(false);
                        leftObjectSelector.GetComponent<SelectObjectTrigger>().IsSelectingObject(true);
                    }
                }

                break;
        }

        // Update the sketching tool :
        if (currentMode.StartsWith("Sketch"))
        {
            leftEraser.gameObject.SetActive(false);
            rightEraser.gameObject.SetActive(false);
            leftBrush.gameObject.SetActive(true);
            rightBrush.gameObject.SetActive(true);

            leftBrushEffect.Stop();
            rightBrushEffect.Stop();

            // Update the help on controller
            UpdateHelp();

            // Update sketch color
            if (currentMode.StartsWith("Sketch_Color_") && hitButton != null)
            {
                if (rightHander == true)
                {
                    rightBrush.GetComponent<MeshRenderer>().material.color = hitButton.GetChild(0).GetComponent<Image>().color;

                    var rightPs = rightBrushEffect.main;
                    rightPs.startColor = hitButton.GetChild(0).GetComponent<Image>().color;
                }
                else
                {
                    leftBrush.GetComponent<MeshRenderer>().material.color = hitButton.GetChild(0).GetComponent<Image>().color;

                    var leftPs = leftBrushEffect.main;
                    leftPs.startColor = hitButton.GetChild(0).GetComponent<Image>().color;
                }
                sketchingMenu.GetComponent<SketchingMenu>().UpdateColorOnSizeButttons(hitButton.GetChild(0).GetComponent<Image>().color);

                // Update the gradient
                if (!currentMode.Contains("Gradient"))
                {
                    sketchingMenu.GetComponent<SketchingMenu>().UpdateColorOnGradientButtons(hitButton.GetChild(0).GetComponent<Image>().color);
                }
            }

            // Update sketch size
            else if (currentMode.StartsWith("Sketch_Size_"))
            {
                // Get the selected size
                string myStringSize = currentMode.Replace("Sketch_Size_", "");
                Debug.Log(myStringSize);
                float mySize = float.Parse(myStringSize);
                sketchSize = mySize;
                mySize = mySize * 200;

                // Set the brush size
                if (rightHander == true)
                {
                    rightEraser.gameObject.SetActive(false);
                    rightBrush.gameObject.SetActive(true);
                    rightBrush.localScale = new Vector3(mySize, mySize, mySize);

                    ParticleSystem.ShapeModule _editableShape = rightBrushEffect.shape;
                    _editableShape.scale = new Vector3(mySize, mySize, mySize);
                }
                else
                {
                    leftEraser.gameObject.SetActive(false);
                    leftBrush.gameObject.SetActive(true);
                    leftBrush.localScale = new Vector3(mySize, mySize, mySize);

                    ParticleSystem.ShapeModule _editableShape = leftBrushEffect.shape;
                    _editableShape.scale = new Vector3(mySize, mySize, mySize);
                }
            }

            // Close the sketching menu
            CloseSketchingMenu();
        }
    }

    /// <summary>
    /// Close the sketching menu
    /// </summary>
    public void CloseSketchingMenu()
    {
        if (coroutineIsRunning == false)
        {
            // Fermeture du menu
            if (sketchingMenuIsOpen == true)
            {
                StartCoroutine(OffSketchingMenu());
                //sketchingMenu.gameObject.SetActive(false);
                leftPointer.gameObject.SetActive(false);
                rightPointer.gameObject.SetActive(false);
                sketchingMenuIsOpen = false;
            }
        }

        // Unclean the area between the controller & the menu
        menuCleaner.GetComponent<MenuCleaner>().UncleanMenuArea();
    }

    /// <summary>
    /// Animation to show the menu
    /// </summary>
    /// <returns></returns>
    IEnumerator OnSketchingMenu()
    {
        float elapsedTime = 0f;
        float finalTime = 0.025f; // 0.25f

        //Vector3 startScale = captureMenu.transform.localScale;
        Vector3 startScale = new Vector3(1, 1, 1);
        Vector3 finalScale = new Vector3(1, 1, 1);

        coroutineIsRunning = true;

        // Enable buttons
        for (int x = 0; x < sketchingMenu.transform.GetChild(1).childCount; x++)
        {
            if (sketchingMenu.transform.GetChild(1).GetChild(x).gameObject.activeSelf)
            {
                sketchingMenu.transform.GetChild(1).GetChild(x).GetComponent<SketchingMenu_Button>().EnableButton();
            }
        }

        // Enable menu
        while (elapsedTime < finalTime)
        {
            sketchingMenu.transform.localScale = Vector3.Lerp(startScale, finalScale, (elapsedTime / finalTime));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        coroutineIsRunning = false;

        yield return null;
    }

    /// <summary>
    /// Animation to close the menu
    /// </summary>
    /// <returns></returns>
    IEnumerator OffSketchingMenu()
    {
        float elapsedTime = 0f;
        float finalTime = 0.25f;

        Vector3 startScale = sketchingMenu.transform.localScale;
        Vector3 finalScale = sketchingMenu.transform.localScale;
        //Vector3 finalScale = new Vector3(0, 0, 0);

        coroutineIsRunning = true;

        // Disable buttons
        for (int x = 0; x < sketchingMenu.transform.GetChild(1).childCount; x++)
        {
            if (sketchingMenu.transform.GetChild(1).GetChild(x).gameObject.activeSelf)
            {
                sketchingMenu.transform.GetChild(1).GetChild(x).GetComponent<SketchingMenu_Button>().DisableButton();
            }
        }

        // Disable menu
        while (elapsedTime < finalTime)
        {
            sketchingMenu.transform.localScale = Vector3.Lerp(startScale, finalScale, (elapsedTime / finalTime));

            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // Set the menu in the waiting area
        sketchingMenu.transform.localScale = new Vector3(0, 0, 0);
        sketchingMenu.transform.position = new Vector3(0, -5, 0);

        coroutineIsRunning = false;

        yield return null;
    }
    #endregion


    #region GROUPS of SKETCHES
    /// <summary>
    /// Create a new folder (= a group of sketches)
    /// </summary>
    public void CreateNewSketchFolder()
    {
        // Suppression du dernier dossier créé si il est vide
        /*if (workFolder.childCount > 0)
        {
            if (workFolder.GetChild(workFolder.childCount - 1).name != "TrashFolder" && workFolder.GetChild(workFolder.childCount - 1).childCount == 0)
            {
                Destroy(workFolder.GetChild(workFolder.childCount - 1).gameObject);
            }
        }
        */
        // Création d'un nouveau dossier
        mySketchFolder = Instantiate(sketchFolderPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        mySketchFolder.transform.parent = workFolder;
        mySketchFolder.name = "SketchFolder_" + folderNumber;
        mySketchFolder.transform.localScale = new Vector3(1, 1, 1);
        folderNumber++;

        Debug.Log("<b>[SketchManager]</b> New FOLDER created (" + mySketchFolder.name + ")");
    }

    public void CreateNewSelectFolder()
    {
        // Suppression du dernier dossier créé si il est vide
        /*if (selectFolder.childCount > 0)
        {
            if (selectFolder.GetChild(selectFolder.childCount - 1).name != "TrashFolder" && selectFolder.GetChild(selectFolder.childCount - 1).childCount == 0)
            {
                Destroy(selectFolder.GetChild(selectFolder.childCount - 1).gameObject);
            }
        }*/

        // Création d'un nouveau dossier
        mySelectFolder = Instantiate(sketchFolderPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        mySelectFolder.transform.parent = selectFolder;
        mySelectFolder.name = "SelectSketchFolder_" + i;
        mySelectFolder.transform.localScale = new Vector3(1, 1, 1);

        Debug.Log("<b>[SketchManager]</b> New Select FOLDER created (" + mySelectFolder.name + ")");
    }

    /// <summary>
    /// Create a new folder for selection (= a group of sketches)
    /// </summary>

    /*
    public void CreateNewSelectFolder()
    {
        // Suppression du dernier dossier créé si il est vide
        if (selectFolder.childCount > 0)
        {
            if (workFolder.GetChild(workFolder.childCount - 1).name != "TrashFolder" && workFolder.GetChild(workFolder.childCount - 1).childCount == 0)
            {
                Destroy(workFolder.GetChild(workFolder.childCount - 1).gameObject);
            }
        }

        // Création d'un nouveau dossier
        mySelectObjectFolder = Instantiate(sketchFolderPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        mySelectObjectFolder.transform.parent = workFolder;
        mySelectObjectFolder.name = "SelectFolder_" + folderNumber;
        mySelectObjectFolder.transform.localScale = new Vector3(1, 1, 1);
        folderNumber++;

        Debug.Log("<b>[SketchManager]</b> New FOLDER created (" + mySketchFolder.name + ")");
    }
    */

    /// <summary>
    /// Duplicate a folder of sketches
    /// </summary>
    public void DuplicateSketchFolder(bool useRightController)
    {
        // Manipulation avec le controller droit
        if (_GrabManager.GetComponent<GrabManager>().GetGrabbedObject(useRightController) != null)
        {
            // Récupération de l'objet à dupliquer
            Transform folderToDuplicate = _GrabManager.GetComponent<GrabManager>().GetGrabbedObject(useRightController);

            // Suppression du dernier dossier créé si il est vide
            if (workFolder.GetChild(workFolder.childCount - 1).name != "TrashFolder" && workFolder.GetChild(workFolder.childCount - 1).childCount == 0)
            {
                Destroy(workFolder.GetChild(workFolder.childCount - 1).gameObject);
            }

            // Création d'un nouveau dossier
            GameObject duplicatedFolder = Instantiate(sketchFolderPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            duplicatedFolder.transform.parent = workFolder;
            duplicatedFolder.name = "SketchFolder_" + folderNumber;
            folderNumber++;

            // Positionnement du dossier dans l'espace
            duplicatedFolder.transform.localPosition = folderToDuplicate.transform.localPosition;
            duplicatedFolder.transform.localRotation = folderToDuplicate.transform.localRotation;
            duplicatedFolder.transform.localScale = folderToDuplicate.transform.localScale;

            // Création de chaque dessin présent dans le dossier
            for (int x = 0; x < folderToDuplicate.childCount; x++)
            {
                GameObject duplicatedSketch = Instantiate(sketchPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                duplicatedSketch.name = "Sketch_" + sketchId;
                sketchId++;

                // Version précédente
                /*duplicatedSketch.transform.parent = duplicatedFolder.transform;
                duplicatedSketch.transform.localPosition = new Vector3(0, 0, 0);
                duplicatedSketch.transform.localRotation = new Quaternion(0, 0, 0, 0);
                duplicatedSketch.transform.localScale = new Vector3(1, 1, 1);*/

                // Nouvelle version
                duplicatedSketch.transform.parent = duplicatedFolder.transform;
                duplicatedSketch.transform.localPosition = folderToDuplicate.GetChild(x).transform.localPosition;
                duplicatedSketch.transform.localRotation = folderToDuplicate.GetChild(x).transform.localRotation;
                duplicatedSketch.transform.localScale = folderToDuplicate.GetChild(x).transform.localScale;

                /*duplicatedSketch.GetComponent<Sketch>().SetInitialScale(folderToDuplicate.GetChild(x).GetComponent<Sketch>().GetInitialScale());
                duplicatedSketch.GetComponent<Sketch>().SetInitialColor(duplicatedSketch.GetComponent<LineRenderer>().material.color);*/
                duplicatedSketch.GetComponent<Sketch>().SetSketchSettings(folderToDuplicate.GetChild(x).GetComponent<Sketch>().GetInitialScale(), folderToDuplicate.GetChild(x).GetComponent<Sketch>().GetInitialSize(), folderToDuplicate.GetChild(x).GetComponent<Sketch>().GetInitialColor());

                LineRenderer duplicatedLr = duplicatedSketch.GetComponent<LineRenderer>();
                duplicatedLr.startColor = Color.white;
                duplicatedLr.endColor = Color.white;
                duplicatedLr.material.color = duplicatedSketch.GetComponent<Sketch>().GetInitialColor();

                // Test
                duplicatedLr.widthMultiplier = folderToDuplicate.GetChild(x).GetComponent<LineRenderer>().widthMultiplier;

                // Display a smooth line (start & end width)
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0.0f, 0.0f);
                curve.AddKey(0.1f, duplicatedSketch.GetComponent<Sketch>().GetInitialSize());
                curve.AddKey(0.9f, duplicatedSketch.GetComponent<Sketch>().GetInitialSize());
                curve.AddKey(1.0f, 0.0f);
                duplicatedLr.widthCurve = curve;

                /*duplicatedLr.startWidth = folderToDuplicate.GetChild(x).GetComponent<LineRenderer>().startWidth;
                duplicatedLr.endWidth = folderToDuplicate.GetChild(x).GetComponent<LineRenderer>().endWidth;*/

                // Copie des points de passage du dessin
                for (int y = 0; y < folderToDuplicate.GetChild(x).GetComponent<LineRenderer>().positionCount; y++)
                {
                    duplicatedLr.positionCount = y + 1;
                    duplicatedLr.SetPosition(y, folderToDuplicate.GetChild(x).GetComponent<LineRenderer>().GetPosition(y));
                }

                // Create the collider
                if (duplicatedSketch.GetComponent<MeshCollider>() == null)
                {
                    duplicatedSketch.GetComponent<Sketch_LineCollider>().CreateColliderFromLine(duplicatedLr);
                }
            }

            Debug.Log("<b>[SketchManager]</b> Duplicate FOLDER (" + folderToDuplicate.name + ")");

            // Création d'un nouveau dossier de dessins (on part du principe que l'utilisateur ne va pas modifier le dessin dupliqué)
            CreateNewSketchFolder();
        }


        // Manipulation avec le controller gauche
        // TO DO...

    }

    /// <summary>
    /// Display each sketch folder with a unique color
    /// </summary>
    public void DisplaySketchFoldersByColor(bool displayColor)
    {
        /*int folderNumber = 0;
        int currentFolder = 0;
        foreach (Transform sketchFolder in workFolder)
        {
            if (sketchFolder.name.StartsWith("SketchFolder_"))
            {
                folderNumber++;
            }   
        }*/

        foreach (Transform sketchFolder in workFolder)
        {
            if (sketchFolder.name.StartsWith("SketchFolder_"))
            {
                // Random color
                Color randomColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

                // Gradient color
                /*currentFolder++;
                float myGradient = 0.8f * currentFolder / folderNumber;
                Color shadedColor = new Color(myGradient, myGradient, myGradient);
                Debug.Log(myGradient);*/

                foreach (Transform sketch in sketchFolder)
                {
                    if (displayColor == true)
                    {
                        sketch.GetComponent<LineRenderer>().material.color = randomColor;
                    }
                    else if (displayColor == false)
                    {
                        sketch.GetComponent<Sketch>().ResetColor();
                    }
                }
            }
        }
    }
    #endregion



    #region HELPERS on Controllers
    public void EnableHelp()
    {
        // Switch helping state
        isHelping = !isHelping;

        // Show help on conrollers
        if (isHelping == true)
        {
            UpdateHelp();
        }

        // Hide help on conrollers
        else if (isHelping == false)
        {
            DisableHelp();
        }

        Debug.Log("<b>[SketchManager]</b> Display help (" + isHelping + ")");
    }

    public void UpdateHelp()
    {
        // Show help on conrollers
        if (isHelping == true)
        {
            string myCurrentMode = currentMode;

            // Rename with a shortest mode (to avoid each variant of "Sketch_Color...")
            if (myCurrentMode.StartsWith("Sketch_"))
            {
                myCurrentMode = "Sketch";
            }

            // Update helper
            switch (myCurrentMode)
            {
                default:
                    leftHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab a sketch"); // Grip : Press the grip to grab a sketch
                    leftHelpers[1].GetComponent<ControllerHelper>().EnableHelper(""); // Interactor
                    leftHelpers[2].GetComponent<ControllerHelper>().EnableHelper("Undo action"); // Touchpad_Left
                    leftHelpers[3].GetComponent<ControllerHelper>().EnableHelper("Redo action"); // Touchpad_Right
                    leftHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Press once the trigger to open the menu"); // Trigger

                    rightHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab a sketch"); // Grip : Press the grip to grab a sketch
                    rightHelpers[1].GetComponent<ControllerHelper>().EnableHelper(""); // Interactor
                    rightHelpers[2].GetComponent<ControllerHelper>().EnableHelper("Undo action"); // Touchpad_Left
                    rightHelpers[3].GetComponent<ControllerHelper>().EnableHelper("Redo action"); // Touchpad_Right
                    rightHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Press once the trigger to open the menu"); // Trigger
                    break;

                case "Sketch":

                    if (symmetryMode == true)
                    {
                        leftHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab the symmetry plane"); // Grip
                        rightHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab the symmetry plane"); // Grip
                    }
                    else
                    {
                        leftHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab a sketch"); // Grip
                        rightHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab a sketch"); // Grip
                    }

                    leftHelpers[1].GetComponent<ControllerHelper>().EnableHelper("Paintbrush"); // Interactor
                    leftHelpers[2].GetComponent<ControllerHelper>().EnableHelper("Undo action"); // Touchpad_Left
                    leftHelpers[3].GetComponent<ControllerHelper>().EnableHelper("Redo action"); // Touchpad_Right
                    leftHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Keep pressed the trigger to sketch"); // Trigger

                    rightHelpers[1].GetComponent<ControllerHelper>().EnableHelper("Paintbrush"); // Interactor
                    rightHelpers[2].GetComponent<ControllerHelper>().EnableHelper("Undo action"); // Touchpad_Left
                    rightHelpers[3].GetComponent<ControllerHelper>().EnableHelper("Redo action"); // Touchpad_Right
                    rightHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Keep pressed the trigger to sketch"); // Trigger
                    break;

                case "Eraser":
                    leftHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab a sketch"); // Grip
                    leftHelpers[1].GetComponent<ControllerHelper>().EnableHelper("Eraser"); // Interactor
                    leftHelpers[2].GetComponent<ControllerHelper>().EnableHelper("Undo action"); // Touchpad_Left
                    leftHelpers[3].GetComponent<ControllerHelper>().EnableHelper("Redo action"); // Touchpad_Right
                    leftHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Keep pressed the trigger to erase a sketch"); // Trigger

                    rightHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab a sketch"); // Grip
                    rightHelpers[1].GetComponent<ControllerHelper>().EnableHelper("Eraser"); // Interactor
                    rightHelpers[2].GetComponent<ControllerHelper>().EnableHelper("Undo action"); // Touchpad_Left
                    rightHelpers[3].GetComponent<ControllerHelper>().EnableHelper("Redo action"); // Touchpad_Right
                    rightHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Keep pressed the trigger to erase a sketch"); // Trigger
                    break;

                case "SelectGroup":
                    leftHelpers[0].GetComponent<ControllerHelper>().EnableHelper(""); // Grip
                    leftHelpers[1].GetComponent<ControllerHelper>().EnableHelper("Group picker"); // Interactor
                    leftHelpers[2].GetComponent<ControllerHelper>().EnableHelper(""); // Touchpad_Left
                    leftHelpers[3].GetComponent<ControllerHelper>().EnableHelper(""); // Touchpad_Right
                    leftHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Press the trigger to pick a group"); // Trigger

                    rightHelpers[0].GetComponent<ControllerHelper>().EnableHelper(""); // Grip
                    rightHelpers[1].GetComponent<ControllerHelper>().EnableHelper("Group picker"); // Interactor
                    rightHelpers[2].GetComponent<ControllerHelper>().EnableHelper(""); // Touchpad_Left
                    rightHelpers[3].GetComponent<ControllerHelper>().EnableHelper(""); // Touchpad_Right
                    rightHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Press the trigger to pick a group"); // Trigger
                    break;

                case "SelectObject":
                    leftHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab a sketch"); // Grip
                    leftHelpers[1].GetComponent<ControllerHelper>().EnableHelper("Object picker"); // Interactor
                    leftHelpers[2].GetComponent<ControllerHelper>().EnableHelper("Undo action"); // Touchpad_Left
                    leftHelpers[3].GetComponent<ControllerHelper>().EnableHelper("Redo action"); // Touchpad_Right
                    leftHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Press the trigger to pick a object"); // Trigger

                    rightHelpers[0].GetComponent<ControllerHelper>().EnableHelper("Press the grip to grab a sketch"); // Grip
                    rightHelpers[1].GetComponent<ControllerHelper>().EnableHelper("Object picker"); // Interactor
                    rightHelpers[2].GetComponent<ControllerHelper>().EnableHelper("Undo action"); // Touchpad_Left
                    rightHelpers[3].GetComponent<ControllerHelper>().EnableHelper("Redo action"); // Touchpad_Right
                    rightHelpers[4].GetComponent<ControllerHelper>().EnableHelper("Press the trigger to pick a object"); // Trigger
                    break;
            }
        }
    }

    public void DisableHelp()
    {
        leftHelpers[0].GetComponent<ControllerHelper>().EnableHelper(""); // Grip
        leftHelpers[1].GetComponent<ControllerHelper>().EnableHelper(""); // Interactor
        leftHelpers[2].GetComponent<ControllerHelper>().EnableHelper(""); // Touchpad_Left
        leftHelpers[3].GetComponent<ControllerHelper>().EnableHelper(""); // Touchpad_Right
        leftHelpers[4].GetComponent<ControllerHelper>().EnableHelper(""); // Trigger

        rightHelpers[0].GetComponent<ControllerHelper>().EnableHelper(""); // Grip
        rightHelpers[1].GetComponent<ControllerHelper>().EnableHelper(""); // Interactor
        rightHelpers[2].GetComponent<ControllerHelper>().EnableHelper(""); // Touchpad_Left
        rightHelpers[3].GetComponent<ControllerHelper>().EnableHelper(""); // Touchpad_Right
        rightHelpers[4].GetComponent<ControllerHelper>().EnableHelper(""); // Trigger

        isHelping = false;
    }
    #endregion

}