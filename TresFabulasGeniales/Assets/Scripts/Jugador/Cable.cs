
using UnityEngine;



[RequireComponent (typeof (LineRenderer))]
public class Cable : MonoBehaviour
{
    public bool padre;

    [SerializeField] private LayerMask capas;
    private Transform twii, nuevoObj, actualObj;
    private LineRenderer lineRnd;
    //private bool isTarget1;
    private Camera camara;


    // .
    private void Start ()
    {
        twii = GameObject.FindGameObjectWithTag("Jugador").transform;
        nuevoObj = this.transform;
        lineRnd = GetComponent<LineRenderer> ();
        //isTarget1 = true;
        camara = GameObject.FindGameObjectWithTag("CamaraPrincipal").GetComponent<Camera> ();

        if (padre == true)
        {
            lineRnd.SetPosition (1, this.transform.InverseTransformPoint (twii.position));
        }
        else
        {
            lineRnd.SetPosition (1, twii.position);
        }

        actualObj = nuevoObj;
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
                lineRnd.enabled = true;
            }
        }
        else if (Input.GetButtonDown ("Soltarse") == true)
        {
            lineRnd.enabled = false;
        }
   
        lineRnd = GetComponent<LineRenderer> ();
        if (padre == true)
        {
            lineRnd.SetPosition (1, this.transform.InverseTransformPoint (twii.position));
        }
        else
        {
            lineRnd.SetPosition (1, twii.position);
        }

        if (padre == true)
        {
            lineRnd.SetPosition (0, this.transform.InverseTransformPoint (actualObj.position));
        }
        else
        {
            lineRnd.SetPosition (0, actualObj.position);
        }
    }
}