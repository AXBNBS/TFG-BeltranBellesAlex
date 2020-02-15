
using UnityEngine;



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
}