
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AreaEnemiga : MonoBehaviour
{
    private Enemigo[] enemigos;
    private List<Transform> dentro;


    // .
    private void Start ()
    {
        enemigos = this.GetComponentsInChildren<Enemigo> ();
        dentro = new List<Transform> ();
    }


    // .
    private void Update ()
    {
        
    }


    // .
    private void OnTriggerEnter (Collider other)
    {
        dentro.Add (other.transform);
        for (int e = 0; e < enemigos.Length; e += 1) 
        {
            if (dentro.Count > 1 && e >= enemigos.Length / 2) 
            {
                enemigos[e].AtacarA (dentro[1]);
            }
            else 
            {
                enemigos[e].AtacarA (dentro[0]);
            }
        }

    }


    // .
    private void OnTriggerExit (Collider other)
    {
        dentro.Remove (other.transform);
        if (dentro.Count == 0) 
        {
            foreach (Enemigo e in enemigos)
            {
                e.Parar ();
            }
        }
        else 
        {
            foreach (Enemigo e in enemigos) 
            {
                e.AtacarA (dentro[0]);
            }
        }
    }
}