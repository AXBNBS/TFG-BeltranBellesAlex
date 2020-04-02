
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AlmacenDatos : MonoBehaviour
{
    public static AlmacenDatos instancia;
    public int regresarA;


    // Creación del singleton e inicialización de variables.
    private void Awake ()
    {
        if (GameObject.FindObjectsOfType<AlmacenDatos>().Length > 1)
        {
            GameObject.Destroy (this.gameObject);
        }
        else
        {
            GameObject.DontDestroyOnLoad (this.gameObject);
        }

        instancia = this;
        regresarA = 1;
        // CUIDAO CON ESTO AL FINAL
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
}