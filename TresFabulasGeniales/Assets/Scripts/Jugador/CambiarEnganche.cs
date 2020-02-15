
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CambiarEnganche : MonoBehaviour
{
    public Transform engancheNue;
    public Colgarse colgarse;

    
    // .
    private void Start ()
    {
        
    }


    // .
    private void Update ()
    {
        if (Mathf.RoundToInt (Input.GetAxisRaw ("Engancharse")) != 0) 
        {
            colgarse.balanceo.CambiarEnganche (engancheNue.position);
        }
    }
}