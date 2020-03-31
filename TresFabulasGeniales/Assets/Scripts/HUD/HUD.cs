
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class HUD : MonoBehaviour
{
    // .
    private void Start ()
    {
        GameObject.FindGameObjectWithTag("Transicion").transform.parent = this.transform.GetChild (0);
    }
}