using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SelectObjectTrigger : MonoBehaviour
{
    private bool isSelectingObject = false;

    private bool ObjectAffirmed = false;

    private bool DontChangeMyColor = false;

    private int cpt = 0;
    private int cptCollider = 0;


    private Transform _ActionsManager;

    private Transform currentController;

    private Transform currentSelectedObject;


    // Start is called before the first frame update
    void Start()
    {
        // Get the current controller
        currentController = transform.parent.parent;

        // Get other managers
        _ActionsManager = GameObject.Find("_ActionsManager").transform;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        //Collider. = false;

        // Objets 3D
        if (isSelectingObject == true && other.GetComponent<Sketch>() && cptCollider == 0)
        {

            cptCollider++;

            // Add haptic on controller
            if (currentController.name.Contains("eft"))
            {
                _ActionsManager.GetComponent<ActionsManager>().LeftPulse(0.15f, 0.5f, 0.5f);
            }
            else if (currentController.name.Contains("ight"))
            {
                _ActionsManager.GetComponent<ActionsManager>().RightPulse(0.15f, 0.5f, 0.5f);
            }

            // Get the selected sketch

            if (other.transform.GetComponent<LineRenderer>().material.color == Color.white)
            {
                DontChangeMyColor = true;
            }
            else
            {
                other.transform.GetComponent<LineRenderer>().material.color = Color.white;
                DontChangeMyColor = false;

            }

            //other.transform.GetComponent<LineRenderer>().generateLightingData=true;


            /*Color a = other.transform.GetComponent<LineRenderer>().material.color;
            a.a = 0.0f;
            other.transform.GetComponent<LineRenderer>().material.color = a;*/

            currentSelectedObject = other.transform;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isSelectingObject == true && other.GetComponent<Sketch>() && currentSelectedObject != null && other.transform == currentSelectedObject)
        {
            Debug.Log("                  ObjectAffirmed                    " + ObjectAffirmed);
            Debug.Log("                  DontChangeMyColor                    " + DontChangeMyColor);
            Debug.Log("                  cpt                    " + cpt);

            //you didnt select an unselected object (pass through an unselected obj ==> reset color)
            if (ObjectAffirmed == false && DontChangeMyColor == false)
            {

                other.GetComponent<Sketch>().ResetColor();

            }
            //you didnt select a selected object (pass through a selected obj ==> dont reset color)
            else if (ObjectAffirmed == false && DontChangeMyColor == true)
            {
                //donothing
            }
            //you select a selected object (you deselect an obj ==> reset color)
            else if (ObjectAffirmed == true && DontChangeMyColor == true)
            {


                other.GetComponent<Sketch>().ResetColor();

            }
            //cpt = 1 3 5 you select a unselected object (you select an object ==> dont reset)    
            //cpt = 2 4 6 you select and select an other time an unselected object (you select then deselect an object ==> reset)
            else if (ObjectAffirmed == true && DontChangeMyColor == false)
            {
                //donothing
                if (cpt % 2 == 1 || cpt == 0)
                {
                    //donothing
                }
                else
                {
                    other.GetComponent<Sketch>().ResetColor();
                }
            }

            cpt = 0;
            cptCollider = 0;

            DontChangeMyColor = false;

            ObjectAffirmed = false;

            currentSelectedObject = null;

            gameObject.GetComponent<BoxCollider>().isTrigger = true;


            /*Color a = transform.GetComponent<SpriteRenderer>().color;
            a.a = 1.0f;
            other.transform.GetComponent<SpriteRenderer>().color = a;

            currentSelectedObject = other.transform;
            */
            //Debug.Log("a=" + other.transform.GetComponent<LineRenderer>().material.color.a);



        }

    }

    public void IsSelectingObject(bool selectObjectMode)
    {
        isSelectingObject = selectObjectMode;

        if (isSelectingObject == true)
        {
            currentSelectedObject = null;
        }
    }

    public void AffirmObject(bool ObjectMode)
    {
        ObjectAffirmed = ObjectMode;
    }

    public void CptTrigger()
    {
        cpt++;
    }

    public Transform GetSelectedObject()
    {
        if (currentSelectedObject != null && currentSelectedObject.GetComponentInParent<Grabbable>())
        {
            return currentSelectedObject;
        }
        else
        {
            return null;
        }
    }
}