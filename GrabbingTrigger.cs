using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbingTrigger : MonoBehaviour
{
    // Grabbing
    private bool isGrabbing = false;
    private Transform triggeredObject;

    // Highlighted controller
    private Transform currentController;
    private GameObject controllerMeshes;
    private bool isHighlighted = false;

    // Managers
    private Transform _ActionsManager;

    // Start is called before the first frame update
    void Start()
    {
        // Get the actions manager to add haptic on controllers
        _ActionsManager = GameObject.Find("_ActionsManager").transform;

        // Get the highlighted controller
        currentController = transform.parent.parent; // = Controller (right)
        for (int x = 0; x < currentController.childCount; x++)
        {
            if (currentController.GetChild(x).name == "Controller_Meshes")
            {
                controllerMeshes = currentController.GetChild(x).gameObject;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Without SketchFolder
    /*public Transform GetTriggeredObject()
    {
        if (triggeredObject != null && triggeredObject.GetComponent<Grabbable>().GetGrabState() == false)
        {
            return triggeredObject;
        }
        else
        {
            return null;
        }
    }*/

    // With SketchFolder
    public Transform GetTriggeredObject()
    {
        if (triggeredObject != null && triggeredObject.GetComponentInParent<Grabbable>().GetGrabState() == false)
        {
            return triggeredObject;
        }
        else
        {
            return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Objets 3D
        if (isGrabbing == false && other.tag == "Grabbable")
        {
            // Reset the color of the prvious triggered object
            /*if (triggeredObject != null)
            {
                for (int x = 0; x < triggeredObject.parent.childCount; x++)
                {
                    if (triggeredObject.parent.GetChild(x).GetComponent<Sketch>())
                    {
                        triggeredObject.parent.GetChild(x).GetComponent<Sketch>().ResetColor();
                    }
                    else
                    {
                        triggeredObject.parent.GetChild(x).GetComponent<Renderer>().material.color = Color.white;
                    }

                }
                triggeredObject = null;

                GetComponent<Renderer>().material.color = Color.white;

                //Debug.Log("Untriggered");
                // Affichage de la possibilité d'attraper l'objet
                //highlightedCircle.GetComponent<Renderer>().material.color = Color.white;
            }*/

            // Add haptic on controller
            if (currentController.name.Contains("eft"))
            {
                _ActionsManager.GetComponent<ActionsManager>().LeftPulse(0.1f, 0.2f, 0.2f);
            }
            else if (currentController.name.Contains("ight"))
            {
                _ActionsManager.GetComponent<ActionsManager>().RightPulse(0.1f, 0.2f, 0.2f);
            }



            triggeredObject = other.transform;

            /*for (int x = 0; x < triggeredObject.parent.childCount; x++)
            {
                triggeredObject.parent.GetChild(x).GetComponent<Renderer>().material.color = Color.yellow;
            }*/

            for (int a = 0; a < controllerMeshes.transform.childCount; a++)
            {
                if (controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>() && (controllerMeshes.transform.GetChild(a).name.EndsWith("Body") || controllerMeshes.transform.GetChild(a).name.EndsWith("Head")))
                {
                    if (other.GetComponent<Sketch>())
                    {
                        //controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>().material.color = other.GetComponent<Sketch>().GetInitialColor();
                        controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>().material.color = other.GetComponent<LineRenderer>().material.color;
                    }
                    else
                    {
                        controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>().material.color = new Color(1, 0.9f, 0.5f);
                    }
                }
            }
            isHighlighted = true;
            //GetComponent<Renderer>().material.color = Color.yellow;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Objets 3D
        if (isGrabbing == false && other.tag == "Grabbable")
        {
            triggeredObject = other.transform;

            if (isHighlighted == false)
            {
                for (int a = 0; a < controllerMeshes.transform.childCount; a++)
                {
                    if (controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>() && (controllerMeshes.transform.GetChild(a).name.EndsWith("Body") || controllerMeshes.transform.GetChild(a).name.EndsWith("Head")))
                    {
                        if (other.GetComponent<Sketch>())
                        {
                            //controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>().material.color = other.GetComponent<Sketch>().GetInitialColor();
                            controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>().material.color = other.GetComponent<LineRenderer>().material.color;
                        }
                        else
                        {
                            controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>().material.color = new Color(1, 0.9f, 0.5f);
                        }
                    }
                }
            }


            //GetComponent<Renderer>().material.color = Color.yellow;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (isGrabbing == false && other.tag == "Grabbable" && triggeredObject != null)
        {
            /*for (int x = 0; x < triggeredObject.parent.childCount; x++)
            {
                if (triggeredObject.parent.GetChild(x).GetComponent<Sketch>())
                {
                    triggeredObject.parent.GetChild(x).GetComponent<Sketch>().ResetColor();
                }
                else
                {
                    triggeredObject.parent.GetChild(x).GetComponent<Renderer>().material.color = Color.white;
                }
                
            }*/
            triggeredObject = null;

            for (int a = 0; a < controllerMeshes.transform.childCount; a++)
            {
                if (controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>() && (controllerMeshes.transform.GetChild(a).name.EndsWith("Body") || controllerMeshes.transform.GetChild(a).name.EndsWith("Head")))
                {
                    controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>().material.color = Color.white;
                }
            }
            isHighlighted = false;
            //GetComponent<Renderer>().material.color = Color.white;

            //Debug.Log("Untriggered");
            // Affichage de la possibilité d'attraper l'objet
            //highlightedCircle.GetComponent<Renderer>().material.color = Color.white;
        }

    }

    /// <summary>
    /// Disable the grabbing trigger (user can't grab a sketch if a menu is open)
    /// </summary>
    /// <param name="wantToDisable"></param>
    public void DisableGrabbingTrigger(bool wantToDisable)
    {
        if (wantToDisable == true)
        {
            // Reset the color of the highlighted controller mesh
            for (int a = 0; a < controllerMeshes.transform.childCount; a++)
            {
                if (controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>() && (controllerMeshes.transform.GetChild(a).name.EndsWith("Body") || controllerMeshes.transform.GetChild(a).name.EndsWith("Head")))
                {
                    controllerMeshes.transform.GetChild(a).GetComponent<MeshRenderer>().material.color = Color.white;
                }
            }

            isHighlighted = false;
        }
    }
}