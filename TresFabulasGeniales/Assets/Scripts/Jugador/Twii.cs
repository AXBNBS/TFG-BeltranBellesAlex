
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class Twii
{
    public Vector3 velocidad;
    public float gravedad = 20;
    public Vector3 gravedadDir = new Vector3 (0, -1, 0);


    public void AplicarGravedad () 
    {
        velocidad += gravedadDir * gravedad * Time.deltaTime;
    }
}