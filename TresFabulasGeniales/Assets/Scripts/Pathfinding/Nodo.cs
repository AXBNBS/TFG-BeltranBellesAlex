
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Nodo
{
    public bool caminable;
    public Vector3 posicion;
    public int costeG, costeH, redX, redY;
    public Nodo (bool disponible, Vector3 pos, int x, int y) {
        caminable = disponible;
        posicion = pos;
        redX = x;
        redY = y; }
    public int costeF {
        get 
        {
            return costeG + costeH; 
        }}
}