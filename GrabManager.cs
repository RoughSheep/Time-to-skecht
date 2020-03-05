using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrabManager : MonoBehaviour
{
    // Managers
    private Transform _ActionsManager;
    private Transform _HistoryManager;
    private Transform _GrabManager;

    // Main controllers
    private Transform leftController;
    private Transform rightController;

    // Controller_Grabber
    private Transform leftGrabController;
    private Transform rightGrabController;
    private Transform leftGrabbingHandler;
    private Transform rightGrabbingHandler;
    private Transform leftScaler;
    private Transform rightScaler;

    // Player
    private bool rightHander = true;

    // Grabber
    private bool isGrabbingOnLeft = false;
    private bool isGrabbingOnRight = false;

    // Grabbed object
    private Transform triggeredObject;
    private Transform rightGrabbedObject;
    private Transform previousRightGrabbedObject;
    private Transform leftGrabbedObject;
    private Transform previousLeftGrabbedObject;
    private Vector3 positionBeforeMove;
    private Quaternion rotationBeforeMove;

    // Scaler
    private bool isRescalingWorld = false;
    public GameObject newScalerViewer;
    private GameObject scalerViewer;
    private Text scalerViewer_ScaleValue_A;
    private Text scalerViewer_ScaleValue_B;
    private GameObject scalingHandler;
    Vector3 initialPos;
    float initialScale = 1;
    float currentScale = 1;
    float scaleToDisplay = 1;
    float initialDistanceBetweenControllers;
    float currentDistanceBetweenControllers;
    Transform realScalingHandler;

    // Work folder
    private Transform workFolderToRescale;

    // Other
    private bool disableGrab = false;

    // Selection
    private Transform currentSelectedObjects;

    // Start is called before the first frame update
    void Start()
    {
        // Get other managers
        _ActionsManager = GameObject.Find("_ActionsManager").transform;
        _HistoryManager = GameObject.Find("_HistoryManager").transform;
        _GrabManager = transform;

        // Init controllers
        if (leftGrabController == null || rightGrabController == null)
        {
            InitControllers();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrabbingOnRight == true || isGrabbingOnLeft == true)
        {
            OnGrabbing();
        }

        if (isRescalingWorld == true)
        {
            OnRescalingWorld();
        }

    }


    #region Grabbing
    /// <summary>
    /// Start to grab an object
    /// </summary>
    /// <param name="useRightController"></param>
    public void StartGrabbing(bool useRightController)
    {
        // Block the grabbing action if a menu is open
        if (disableGrab == false)
        {
            // Manipulation avec le controller droit
            if (useRightController == true && rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject() != null)
            {
                isGrabbingOnRight = true;

                // Add haptic on controller
                _ActionsManager.GetComponent<ActionsManager>().RightPulse(0.2f, 0.2f, 0.2f);

                // Grab a sketch
                /*if (rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().name.StartsWith("Sketch"))
                {
                    rightGrabbedObject = rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().parent;
                    rightGrabbedObject.GetComponentInParent<Grabbable>().SetGrabState(true);
                }*/

                //Grab selections
                if (currentSelectedObjects != null)
                {
                    rightGrabbedObject = currentSelectedObjects;
                    rightGrabbedObject.GetComponentInParent<Grabbable>().SetGrabState(true);
                }


                //Grab an object
                else if (rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().name.StartsWith("Sketch"))
                {
                    rightGrabbedObject = rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject();
                    rightGrabbedObject.GetComponentInParent<Grabbable>().SetGrabState(true);
                }

                /*
                                //Grab a group
                                else if (rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().name.StartsWith("Sketch") && rightObjectSelector.GetComponent<SketchManager>().currentMode)
                                {
                                    rightGrabbedObject = rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().parent;
                                    rightGrabbedObject.GetComponentInParent<Grabbable>().SetGrabState(true);
                                }*/
                // Grab the capture camera
                else if (rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().name == "CaptureCamera")
                {
                    rightGrabbedObject = rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject();
                    rightGrabbedObject.GetComponent<Grabbable>().SetGrabState(true);
                }
                // Grab filters
                else if (rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().name.StartsWith("MH_Filter_"))
                {
                    rightGrabbedObject = rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject();
                    rightGrabbedObject.GetComponent<Grabbable>().SetGrabState(true);
                }
                // Grab others
                else
                {
                    rightGrabbedObject = rightGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().parent;
                    rightGrabbedObject.GetComponentInParent<Grabbable>().SetGrabState(true);
                }

                // Save the initial position & rotation before moving object
                positionBeforeMove = rightGrabbedObject.transform.localPosition;
                rotationBeforeMove = rightGrabbedObject.transform.localRotation;

                rightGrabbingHandler.transform.position = rightGrabbedObject.transform.position;
                rightGrabbingHandler.transform.rotation = rightGrabbedObject.transform.rotation;

                //Debug.Log("Grab " + rightGrabbedObject + " on right controller");
            }

            // Manipulation avec le controller gauche
            if (useRightController == false && leftGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject() != null)
            {
                isGrabbingOnLeft = true;

                // Add haptic on controller
                _ActionsManager.GetComponent<ActionsManager>().LeftPulse(0.2f, 0.2f, 0.2f);

                // Grab a sketch
                if (leftGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().name.StartsWith("Sketch"))
                {
                    leftGrabbedObject = leftGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().parent;
                    leftGrabbedObject.GetComponentInParent<Grabbable>().SetGrabState(true);
                }
                // Grab the capture camera
                else if (leftGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().name == "CaptureCamera")
                {
                    leftGrabbedObject = leftGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject();
                    leftGrabbedObject.GetComponent<Grabbable>().SetGrabState(true);
                }
                // Grab filters
                else if (leftGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().name.StartsWith("MH_Filter_"))
                {
                    leftGrabbedObject = leftGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject();
                    leftGrabbedObject.GetComponent<Grabbable>().SetGrabState(true);
                }
                // Grab others
                else
                {
                    leftGrabbedObject = leftGrabController.GetComponentInChildren<GrabbingTrigger>().GetTriggeredObject().parent;
                    leftGrabbedObject.GetComponentInParent<Grabbable>().SetGrabState(true);
                }

                leftGrabbingHandler.transform.position = leftGrabbedObject.transform.position;
                leftGrabbingHandler.transform.rotation = leftGrabbedObject.transform.rotation;

                //Debug.Log("Grab " + leftGrabbedObject + " on left controller");
            }
        }

    }



    /// <summary>
    /// Is grabbing an object
    /// </summary>
    public void OnGrabbing()
    {
        if (rightGrabbedObject != null)
        {
            rightGrabbedObject.transform.position = rightGrabbingHandler.transform.position;
            rightGrabbedObject.transform.rotation = rightGrabbingHandler.transform.rotation;
        }

        if (leftGrabbedObject != null)
        {
            leftGrabbedObject.transform.position = leftGrabbingHandler.transform.position;
            leftGrabbedObject.transform.rotation = leftGrabbingHandler.transform.rotation;
        }
    }



    /// <summary>
    /// Stop to grab an oject
    /// </summary>
    /// <param name="useRightController"></param>
    public void StopGrabbing(bool useRightController)
    {
        if (useRightController == true && rightGrabbedObject != null)
        {
            Debug.Log("<b>[GrabManager]</b> " + rightGrabbedObject.name + " moved by the right controller");

            // Add haptic on controller
            _ActionsManager.GetComponent<ActionsManager>().RightPulse(0.1f, 0.2f, 0.2f);

            // Send action to history
            _HistoryManager.GetComponent<HistoryManager>().AddAction_Z("Move object"
                + "_" + ProjectUtilities.VectorToString(positionBeforeMove)
                + "_" + ProjectUtilities.QuaternionToString(rotationBeforeMove)
                + "_" + ProjectUtilities.VectorToString(rightGrabbedObject.localPosition)
                + "_" + ProjectUtilities.QuaternionToString(rightGrabbedObject.localRotation),
                rightGrabbedObject);

            rightGrabbedObject.GetComponentInParent<Grabbable>().SetGrabState(false);
            rightGrabbedObject = null;
            rightGrabbingHandler.transform.position = new Vector3(0, 0, 0);
            rightGrabbingHandler.transform.rotation = new Quaternion(0, 0, 0, 0);

            isGrabbingOnRight = false;
        }

        if (useRightController == false && leftGrabbedObject != null)
        {
            Debug.Log("<b>[GrabManager]</b> " + leftGrabbedObject.name + " moved by the left controller");

            // Add haptic on controller
            _ActionsManager.GetComponent<ActionsManager>().LeftPulse(0.1f, 0.2f, 0.2f);

            // Send action to history
            _HistoryManager.GetComponent<HistoryManager>().AddAction_Z("Move object"
                + "_" + ProjectUtilities.VectorToString(positionBeforeMove)
                + "_" + ProjectUtilities.QuaternionToString(rotationBeforeMove)
                + "_" + ProjectUtilities.VectorToString(leftGrabbedObject.localPosition)
                + "_" + ProjectUtilities.QuaternionToString(leftGrabbedObject.localRotation),
                rightGrabbedObject);

            leftGrabbedObject.GetComponentInParent<Grabbable>().SetGrabState(false);
            leftGrabbedObject = null;
            leftGrabbingHandler.transform.position = new Vector3(0, 0, 0);
            leftGrabbingHandler.transform.rotation = new Quaternion(0, 0, 0, 0);

            isGrabbingOnLeft = false;
        }
    }
    #endregion

    public void SetSelectedObjects(Transform myObjects)
    {
        currentSelectedObjects = myObjects;
    }


    #region Scaling
    /// <summary>
    /// Start to rescale the world by grabbing with 2 controllers
    /// </summary>
    public void StartRescalingWorld()
    {
        if (isRescalingWorld == false)
        {
            // Manipulation avec les 2 controllers
            if (isGrabbingOnRight == false && isGrabbingOnLeft == false)
            {
                // Add haptic on controller
                _ActionsManager.GetComponent<ActionsManager>().LeftPulse(0.2f, 0.2f, 0.2f);
                _ActionsManager.GetComponent<ActionsManager>().RightPulse(0.2f, 0.2f, 0.2f);

                isRescalingWorld = true;

                // Init the scaler viwer
                scalerViewer = Instantiate(newScalerViewer);
                scalerViewer.name = "ScalerViewer";

                for (int a = 0; a < scalerViewer.transform.GetChild(0).childCount; a++)
                {
                    if (scalerViewer.transform.GetChild(0).GetChild(a).name == "ScaleValue_A")
                    {
                        scalerViewer_ScaleValue_A = scalerViewer.transform.GetChild(0).GetChild(a).GetComponent<Text>();
                    }
                    if (scalerViewer.transform.GetChild(0).GetChild(a).name == "ScaleValue_B")
                    {
                        scalerViewer_ScaleValue_B = scalerViewer.transform.GetChild(0).GetChild(a).GetComponent<Text>();
                    }
                }

                scalerViewer.GetComponent<LineRenderer>().SetPosition(0, leftScaler.transform.position);
                scalerViewer.GetComponent<LineRenderer>().SetPosition(1, Vector3.Lerp(leftScaler.transform.position, rightScaler.transform.position, 0.5f));
                scalerViewer.GetComponent<LineRenderer>().SetPosition(2, rightScaler.transform.position);

                scalingHandler = new GameObject("ScalerHandler"); //GameObject.CreatePrimitive(PrimitiveType.Cube);
                scalingHandler.transform.parent = scalerViewer.transform;
                scalingHandler.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

                scalerViewer.transform.position = Vector3.Lerp(leftScaler.transform.position, rightScaler.transform.position, 0.5f);
                scalerViewer.transform.LookAt(leftScaler);
                //scalerViewer.transform.localScale = new Vector3(0.005f, 0.005f, Vector3.Distance(leftScaler.transform.position, rightScaler.transform.position));

                // Si modification de pos et rot lors du grab
                /*scalingHandler.transform.position = workFolderToRescale.transform.position;
                scalingHandler.transform.rotation = workFolderToRescale.transform.rotation;*/


                #region Test1
                // Début du Test 1
                /*Vector3 currentPos = workFolderToRescale.transform.position;
                for (int x = 0; x < workFolderToRescale.childCount; x++)
                {
                    if (workFolderToRescale.GetChild(x).GetComponent<SketchFolder>())
                    {
                        workFolderToRescale.GetChild(x).transform.position = scalingHandler.transform.position;
                        //workFolderToRescale.GetChild(x).transform.position = scalingHandler.transform.transform.TransformPoint(workFolderToRescale.GetChild(x).transform.position);
                        //Debug.Log(workFolderToRescale.GetChild(x).transform.position + " / " + scalingHandler.transform.transform.TransformPoint(workFolderToRescale.GetChild(x).transform.position));
                    }
                    else if (workFolderToRescale.GetChild(x).name.StartsWith("DEBUG"))
                    {
                        workFolderToRescale.GetChild(x).transform.position = workFolderToRescale.transform.position - scalingHandler.transform.position;
                        Debug.Log("aaaa");
                        //workFolderToRescale.GetChild(x).transform.position = scalingHandler.transform.transform.TransformPoint(workFolderToRescale.GetChild(x).transform.position);
                        //Debug.Log(workFolderToRescale.GetChild(x).transform.position + " / " + scalingHandler.transform.transform.TransformPoint(workFolderToRescale.GetChild(x).transform.position));
                    }
                }
                workFolderToRescale.transform.position = scalingHandler.transform.position;*/
                // Fin du Test 1
                #endregion

                #region Test2
                // Début du Test 2
                /* workFolderToRescale.transform.position = scalingHandler.transform.position;
                 Vector3 currentPos = workFolderToRescale.transform.position;
                 workFolderToRescale.transform.position = scalingHandler.transform.position;

                 for (int x = 0; x < workFolderToRescale.childCount; x++)
                 {
                     if (workFolderToRescale.GetChild(x).GetComponent<SketchFolder>())
                     {
                         workFolderToRescale.GetChild(x).transform.position = scalingHandler.transform.position;
                         //workFolderToRescale.GetChild(x).transform.position = scalingHandler.transform.transform.TransformPoint(workFolderToRescale.GetChild(x).transform.position);
                         //Debug.Log(workFolderToRescale.GetChild(x).transform.position + " / " + scalingHandler.transform.transform.TransformPoint(workFolderToRescale.GetChild(x).transform.position));
                     }
                     else if (workFolderToRescale.GetChild(x).name.StartsWith("DEBUG"))
                     {
                         Debug.Log(workFolderToRescale.GetChild(x).transform.position + " /// " + workFolderToRescale.GetChild(x).transform.localPosition);
                         workFolderToRescale.GetChild(x).transform.position = (currentPos - scalingHandler.transform.position);
                         Debug.Log(workFolderToRescale.GetChild(x).transform.position + " /// " + workFolderToRescale.GetChild(x).transform.localPosition);
                     }
                 }*/
                // Fin du Test 2
                #endregion

                #region Test 3
                // Début du Test 3
                /*workFolderToRescale.transform.position = scalingHandler.transform.position;
                Vector3 currentPos = workFolderToRescale.transform.position;
                //workFolderToRescale.transform.position = scalingHandler.transform.position;

                for (int x = 0; x < workFolderToRescale.childCount; x++)
                {
                    if (workFolderToRescale.GetChild(x).GetComponent<SketchFolder>())
                    {
                        workFolderToRescale.GetChild(x).transform.position = scalingHandler.transform.position;
                        //workFolderToRescale.GetChild(x).transform.position = scalingHandler.transform.transform.TransformPoint(workFolderToRescale.GetChild(x).transform.position);
                        //Debug.Log(workFolderToRescale.GetChild(x).transform.position + " / " + scalingHandler.transform.transform.TransformPoint(workFolderToRescale.GetChild(x).transform.position));
                    }
                    else if (workFolderToRescale.GetChild(x).name.StartsWith("DEBUG"))
                    {
                        Vector3 decalage = currentPos - initialPos;
                        Debug.Log(" Handler POS = " + scalingHandler.transform.position);
                        Debug.Log(" Décalage = " + decalage);
                        Debug.Log(" CurrentScale = " + currentScale + " InitialScale = " + initialScale + "        " + currentScale / initialScale);
                        //Debug.Log(workFolderToRescale.GetChild(x).transform.position + " /// " + workFolderToRescale.GetChild(x).transform.localPosition);
                        //workFolderToRescale.GetChild(x).transform.localPosition -=  decalage;
                        workFolderToRescale.GetChild(x).transform.localPosition = workFolderToRescale.GetChild(x).transform.localPosition - (decalage * currentScale / initialScale);
                        //Debug.Log(workFolderToRescale.GetChild(x).transform.position + " /// " + workFolderToRescale.GetChild(x).transform.localPosition);
                        Debug.Log(" Debug POS = " + workFolderToRescale.GetChild(x).transform.position);
                        Debug.Log(" Debug LOCAL POS = " + workFolderToRescale.GetChild(x).transform.localPosition);
                    }
                }*/
                // Fin du Test 3
                #endregion

                initialPos = workFolderToRescale.transform.position;
                initialScale = workFolderToRescale.transform.localScale.x;
                initialDistanceBetweenControllers = Vector3.Distance(leftScaler.transform.position, rightScaler.transform.position);

                // V1 : Rotation sur les 3 axes
                /*realScalingHandler = new GameObject("RealScalingHandler").transform;
                realScalingHandler.transform.position = Vector3.Lerp(leftScaler.transform.position, rightScaler.transform.position, 0.5f);
                realScalingHandler.transform.LookAt(leftScaler);//a
                realScalingHandler.transform.localScale = new Vector3(initialScale, initialScale, initialScale);
                workFolderToRescale.transform.parent = realScalingHandler.transform;*/

                // V2 : Rotation sur l'axe y (perpendiculaire au sol)
                realScalingHandler = new GameObject("RealScalingHandler").transform;
                realScalingHandler.transform.position = Vector3.Lerp(leftScaler.transform.position, rightScaler.transform.position, 0.5f);
                realScalingHandler.transform.localEulerAngles = new Vector3(0, scalerViewer.transform.localEulerAngles.y, 0);
                realScalingHandler.transform.localScale = new Vector3(initialScale, initialScale, initialScale);
                workFolderToRescale.transform.parent = realScalingHandler.transform;
            }
        }
    }



    /// <summary>
    /// Is rescaling the world by grabbing with 2 controllers
    /// </summary>
    public void OnRescalingWorld()
    {
        // Manipulation avec les 2 controllers
        if (isRescalingWorld == true)
        {
            // Visualisation
            currentDistanceBetweenControllers = Vector3.Distance(leftScaler.transform.position, rightScaler.transform.position);
            scalerViewer.transform.position = Vector3.Lerp(leftScaler.transform.position, rightScaler.transform.position, 0.5f);
            scalerViewer.transform.LookAt(leftScaler);

            scalerViewer.GetComponent<LineRenderer>().SetPosition(0, leftScaler.transform.position);
            scalerViewer.GetComponent<LineRenderer>().SetPosition(1, Vector3.Lerp(leftScaler.transform.position, rightScaler.transform.position, 0.5f));
            scalerViewer.GetComponent<LineRenderer>().SetPosition(2, rightScaler.transform.position);

            // Calcul des changements de position, de rotation et d'échelle

            // V1 : Rotation sur les 3 axes
            /*realScalingHandler.transform.position = Vector3.Lerp(leftScaler.transform.position, rightScaler.transform.position, 0.5f);
            realScalingHandler.transform.LookAt(leftScaler);
            realScalingHandler.transform.localEulerAngles = new Vector3(0, scalerViewer.transform.localEulerAngles.y, 0);
            currentScale = initialScale * currentDistanceBetweenControllers / initialDistanceBetweenControllers;
            realScalingHandler.transform.localScale = new Vector3(currentScale, currentScale, currentScale);*/

            // V2 : Rotation sur l'axe y (perpendiculaire au sol)
            realScalingHandler.transform.position = Vector3.Lerp(leftScaler.transform.position, rightScaler.transform.position, 0.5f);
            realScalingHandler.transform.localEulerAngles = new Vector3(0, scalerViewer.transform.localEulerAngles.y, 0);
            currentScale = initialScale * currentDistanceBetweenControllers / initialDistanceBetweenControllers;
            realScalingHandler.transform.localScale = new Vector3(currentScale, currentScale, currentScale);

            // Display the real scale value
            //scaleToDisplay = Mathf.Round(currentScale * 100.0f) * 0.01f;
            scalerViewer_ScaleValue_A.text = currentScale.ToString("f2");
            scalerViewer_ScaleValue_B.text = currentScale.ToString("f2");
        }
    }


    /// <summary>
    /// Stop to rescale the world by ungrabbing a controller
    /// </summary>
    public void StopRescalingWorld()
    {
        // Manipulation avec les 2 controllers
        if (isRescalingWorld == true)
        {
            // Add haptic on controller
            _ActionsManager.GetComponent<ActionsManager>().LeftPulse(0.1f, 0.2f, 0.2f);
            _ActionsManager.GetComponent<ActionsManager>().RightPulse(0.1f, 0.2f, 0.2f);

            isRescalingWorld = false;

            workFolderToRescale.transform.parent = null;

            if (scalerViewer != null)
            {
                Destroy(scalerViewer);
                Destroy(scalingHandler);
                Destroy(realScalingHandler.gameObject);
            }

            // Mise à jour de la taille des sketches (line renderer)
            for (int x = 0; x < workFolderToRescale.childCount; x++)
            {
                // Update the sketch size
                if (workFolderToRescale.GetChild(x).name.StartsWith("SketchFolder_"))//if (workFolderToRescale.GetChild(x).name != "World" && workFolderToRescale.GetChild(x).name != "SymmetryPlane")
                {
                    for (int y = 0; y < workFolderToRescale.GetChild(x).childCount; y++)
                    {
                        if (workFolderToRescale.GetChild(x).GetChild(y).GetComponent<Sketch>())
                        {
                            LineRenderer lr = workFolderToRescale.GetChild(x).GetChild(y).GetComponent<LineRenderer>();

                            //Debug.Log("Previous: " + lr.widthMultiplier);

                            //lr.widthMultiplier = currentScale / workFolderToRescale.GetChild(x).GetChild(y).GetComponent<Sketch>().GetInitialScale();
                            lr.widthMultiplier = currentScale * workFolderToRescale.GetChild(x).GetChild(y).GetComponent<Sketch>().GetInitialSketchScale();

                            //Debug.Log("WidthMultiplier: " + lr.widthMultiplier + " = " + currentScale + " * " + workFolderToRescale.GetChild(x).GetChild(y).GetComponent<Sketch>().GetInitialSketchScale());
                        }
                    }
                }

                // Update the size of the symmetry plane to keep it at scale 1
                else if (workFolderToRescale.GetChild(x).name == "SymmetryPlane")
                {
                    workFolderToRescale.GetChild(x).localScale = new Vector3(1 / currentScale, 1 / currentScale, 1 / currentScale);
                }
            }

            Debug.Log("<b>[GrabManager]</b> Rescale the world (from scale " + initialScale + " to " + currentScale + " )");
        }
    }
    #endregion


    #region Others
    public Transform GetGrabbedObject(bool useRightController)
    {
        if (useRightController == true && rightGrabbedObject != null)
        {
            return rightGrabbedObject;
        }
        if (useRightController == false && leftGrabbedObject != null)
        {
            return leftGrabbedObject;
        }
        return null;
    }

    /// <summary>
    /// Disable the grabbing manager & the grabbing trigger (user can't grab a sketch if a menu is open)
    /// </summary>
    /// <param name="wantToDisable"></param>
    public void DisableGrabbingAction(bool wantToDisable)
    {
        if (_GrabManager != null)
        {
            _GrabManager.gameObject.SetActive(!wantToDisable);
            rightGrabController.GetComponentInChildren<GrabbingTrigger>().DisableGrabbingTrigger(wantToDisable);
        }
    }
    #endregion


    /// <summary>
    /// Init controllers
    /// </summary>
    private void InitControllers()
    {
        if (GameObject.FindGameObjectWithTag("LeftController"))
        {
            leftController = GameObject.FindGameObjectWithTag("LeftController").transform;

            for (int x = 0; x < leftController.childCount; x++)
            {
                if (leftController.GetChild(x).name == "Controller_Grabber")
                {
                    leftGrabController = leftController.GetChild(x);

                    for (int y = 0; y < leftGrabController.childCount; y++)
                    {
                        if (leftGrabController.GetChild(y).name == "GrabbingHandler")
                        {
                            leftGrabbingHandler = leftGrabController.GetChild(y);
                        }
                        else if (leftGrabController.GetChild(y).name == "ScalingGrabber")
                        {
                            leftScaler = leftGrabController.GetChild(y);
                        }
                        /*else if (leftGrabController.GetChild(y).name == "ScalingHandler")
                        {
                            scalingHandler = leftGrabController.GetChild(y);
                        }*/
                    }
                }
            }
        }

        if (GameObject.FindGameObjectWithTag("RightController"))
        {
            rightController = GameObject.FindGameObjectWithTag("RightController").transform;

            for (int x = 0; x < rightController.childCount; x++)
            {
                if (rightController.GetChild(x).name == "Controller_Grabber")
                {
                    rightGrabController = rightController.GetChild(x);

                    for (int y = 0; y < rightGrabController.childCount; y++)
                    {
                        if (rightGrabController.GetChild(y).name == "GrabbingHandler")
                        {
                            rightGrabbingHandler = rightGrabController.GetChild(y);
                        }
                        else if (rightGrabController.GetChild(y).name == "ScalingGrabber")
                        {
                            rightScaler = rightGrabController.GetChild(y);
                        }
                        /*else if (rightGrabController.GetChild(y).name == "ScalingHandler")
                        {
                            scalingHandler = rightGrabController.GetChild(y);
                        }*/
                    }
                }
            }
        }

        if (GameObject.Find("WorkFolder"))
        {
            workFolderToRescale = GameObject.Find("WorkFolder").transform;
        }
    }
}
