
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


    // Inicializamos el padre del twii y la longitud de la cuerda.
    public void Inicializar () 
    {
        twiiTrf.parent = enganche.engancheTrf;
        cuerda.longitud = Vector3.Distance (twiiTrf.localPosition, enganche.posicion);
    }


    // .
    public Vector3 Mover (Vector3 posicion, Vector3 posicionPrv, float tiempo) 
    {
        twii.velocidad += ObtenerPosicionNueva (posicion, posicionPrv, tiempo);

        twii.AplicarGravedad ();
        twii.AplicarResistencia ();
        twii.LimitarVelocidadMaxima ();

        posicion += twii.velocidad * tiempo;
        if (Vector3.Distance (posicion, enganche.posicion) < cuerda.longitud) 
        {
            posicion = Vector3.Normalize (posicion - enganche.posicion) * cuerda.longitud;
            cuerda.longitud = Vector3.Distance (posicion, enganche.posicion);
        }

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


    // .
    public void CambiarEnganche (Vector3 nuevaPos) 
    {
        twiiTrf.parent = null;
        enganche.engancheTrf.position = nuevaPos;
        twiiTrf.parent = enganche.engancheTrf;
        enganche.posicion = enganche.engancheTrf.InverseTransformPoint (nuevaPos);
        cuerda.longitud = Vector3.Distance (twiiTrf.localPosition, enganche.posicion);
    }


    // .
    public Vector3 Caida (Vector3 posicion, float tiempo) 
    {
        twii.AplicarGravedad ();
        twii.AplicarResistencia ();
        twii.LimitarVelocidadMaxima ();

        posicion += twii.velocidad * tiempo;

        return posicion;
    }
}