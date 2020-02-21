
using UnityEngine;



[RequireComponent (typeof (LineRenderer))]
public class Cable : MonoBehaviour
{
    [SerializeField] private LayerMask capas;
    private Transform twii, nuevoObj, actualObj;
    private LineRenderer lineRnd;
    //private bool isTarget1;
    private Camera camara;
    private MovimientoHistoria3 twiiMov;


    // .
    private void Start ()
    {
        twii = GameObject.FindGameObjectWithTag("Jugador").transform;
        nuevoObj = this.transform;
        lineRnd = this.GetComponent<LineRenderer> ();
        camara = GameObject.FindGameObjectWithTag("CamaraPrincipal").GetComponent<Camera> ();
        twiiMov = GameObject.FindObjectOfType<MovimientoHistoria3> ();

        lineRnd.SetPosition (1, twii.position);

        actualObj = nuevoObj;
    }


    // .
    /*private void Update ()
    {
        if (Input.GetButtonDown ("Engancharse") == true)
        {
            //Ray ray = camara.ScreenPointToRay (Input.mousePosition);

            if (Physics.Raycast (twii.position, twiiMov.enganchePnt - twii.position, 15, capas, QueryTriggerInteraction.Ignore) == true)
            {
                lineRnd.enabled = true;
            }
        }

        if (Input.GetButtonDown ("Soltarse") == true)
        {
            lineRnd.enabled = false;
        }

        lineRnd.SetPosition (1, twii.position);
        lineRnd.SetPosition (0, actualObj.position);
    }*/
}
/*using UnityEngine;



[RequireComponent (typeof (LineRenderer))]
public class Rope : MonoBehaviour
{
    public Transform bob;
    public Transform target1;
    public bool hasParent;

    [SerializeField] private LayerMask capas;
    private Transform curTarget;
    private LineRenderer lr;
    private bool isTarget1;
    private Camera camara;


    // .
    private void Start ()
    {
        lr = GetComponent<LineRenderer> ();
        isTarget1 = true;
        camara = GameObject.FindObjectOfType<Camera> ();

        if (hasParent == true)
        {
            lr.SetPosition (1, transform.InverseTransformPoint (bob.position));
        }
        else
        {
            lr.SetPosition (1, bob.position);
        }

        curTarget = target1;
    }


    // .
    private void Update ()
    {
        if (Mathf.RoundToInt (Input.GetAxisRaw ("Engancharse")) != 0)
        {
            RaycastHit hit;
            Ray ray = camara.ScreenPointToRay (Input.mousePosition);

            if (Physics.Raycast (ray, out hit, capas) == true)
            {
                //print(hit.transform.name);
                lr.enabled = true;
            }
        }
        else if (Input.GetButtonDown ("Fire2"))
        {
            lr.enabled = false;
        }

        lr = GetComponent<LineRenderer> ();
        if (hasParent == true)
        {
            lr.SetPosition (1, this.transform.InverseTransformPoint (bob.position));
        }
        else
        {
            lr.SetPosition (1, bob.position);
        }

        if (hasParent == true)
        {
            lr.SetPosition (0, this.transform.InverseTransformPoint (curTarget.position));
        }
        else
        {
            lr.SetPosition (0, curTarget.position);
        }
    }
}*/
