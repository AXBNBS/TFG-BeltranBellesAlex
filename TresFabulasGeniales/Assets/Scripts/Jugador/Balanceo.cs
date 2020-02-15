
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class Balanceo
{
    public Transform twiiTrf;
    public Enganche enganche;
    public Cuerda cuerda;
    public Twii twii;

    private Vector3 posicionPrv;


    // .
    public void Inicializar () 
    {
        twiiTrf.parent = enganche.engancheTrf;
        cuerda.longitud = Vector3.Distance (twiiTrf.localPosition, enganche.posicion);
    }


    // .
    public Vector3 Mover (Vector3 posicion, float tiempo) 
    {
        twii.velocidad += ObtenerPosicionNueva (posicion, posicionPrv, tiempo);
        twii.AplicarGravedad ();

        posicion += twii.velocidad * tiempo;
        posicionPrv = posicion;

        return posicion;
    }


    // . 
    public Vector3 ObtenerPosicionNueva (Vector3 actual, Vector3 previa, float tiempo) 
    {
        float engancheDst;
        Vector3 limitadaPos, predecidaPos;

        engancheDst = Vector3.Distance (actual, enganche.posicion);
        if (engancheDst > cuerda.longitud) 
        {
            limitadaPos = Vector3.Normalize (actual - enganche.posicion) * cuerda.longitud;
            predecidaPos = (limitadaPos - previa) / tiempo;

            return predecidaPos;
        }

        return Vector3.zero;
    }
}