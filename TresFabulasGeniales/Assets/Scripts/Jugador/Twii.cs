
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class Twii
{
    [HideInInspector] public Vector3 velocidad;

    [SerializeField] private int gravedad;
    [SerializeField] private Vector3 gravedadDir;
    [SerializeField] private float resistencia, velocidadMax;
    private Vector3 resistenciaDir;


    // .
    public void AplicarGravedad () 
    {
        velocidad += gravedadDir * gravedad * Time.deltaTime;
    }


    // .
    public void AplicarResistencia () 
    {
        resistenciaDir = -velocidad;
        resistenciaDir *= resistencia;
        velocidad += resistenciaDir;
    }


    // .
    public void LimitarVelocidadMaxima () 
    {
        velocidad = Vector3.ClampMagnitude (velocidad, velocidadMax);
    }
}