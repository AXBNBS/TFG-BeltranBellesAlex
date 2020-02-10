
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Parpadeo : MonoBehaviour
{
    [SerializeField] private int[] indicesMat;
    [SerializeField] private Renderer renderer;
    [SerializeField] private Material materialAlt;
    [SerializeField] private Material[] materialesIni, materialesAlt, materialesPrb;


    // .
    private void Start ()
    {
        //renderer.materials = materialesPrb;
        /*int indice = 0;

        materialesIni = new Material[renderer.materials.Length];
        materialesAlt = new Material[materialesIni.Length];
        for (int m = 0; m < materialesIni.Length; m += 1)
        {
            materialesIni[m] = renderer.materials[m];
            if (m != indicesMat[indice])
            {
                materialesAlt[m] = renderer.materials[m];
            }
            else
            {
                if (indice < indicesMat.Length - 1) 
                {
                    indice += 1;
                }
                materialesAlt[m] = materialAlt;
            }
        }*/
    }


    // .
    private void Update ()
    {
        /*if (this.IsInvoking () == false && Input.GetKeyDown (KeyCode.F) == true)
        {
            this.InvokeRepeating ("Parpadear", 0, 0.25f);
        }*/
    }


    // .
    private void Parpadear ()
    {
        renderer.materials = renderer.materials == materialesIni ? materialesAlt : materialesIni;
    }
}