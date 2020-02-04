
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Red : MonoBehaviour
{
    public Vector2 dimensiones;
    public float nodoRad;
    public LayerMask obstaculosMsc;

    private Nodo[,] red;
    private float nodoDia;
    private int tamanyoRedX, tamanyoRedY;


    // Inicialización de variables y creación de la red.
    private void Start()
    {
        nodoDia = nodoRad * 2;
        tamanyoRedX = Mathf.RoundToInt(dimensiones.x / nodoDia);
        tamanyoRedY = Mathf.RoundToInt(dimensiones.y / nodoDia);

        CrearRed();
    }


    // Dibuja el área que abarca la red y cada uno de sus nodos (aquellos que estén en zonas no accesibles se pintan en rojo).
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(this.transform.position, new Vector3(dimensiones.x, 1, dimensiones.y));

        if (red != null)
        {
            foreach (Nodo n in red)
            {
                Gizmos.color = n.caminable ? Color.white : Color.red;

                Gizmos.DrawCube(n.posicion, Vector3.one * (nodoDia - 0.4f));
            }
        }
    }


    // Al recibir un punto del mapa, devuelve el nodo asociado al mismo.
    public Nodo DevolverNodoDePosicion(Vector3 pos)
    {
        float porcentajeX = Mathf.Clamp01((pos.x + dimensiones.x / 2) / dimensiones.x);
        float porcentajeY = Mathf.Clamp01((pos.z + dimensiones.y / 2) / dimensiones.y);
        int x = Mathf.RoundToInt((tamanyoRedX - 1) * porcentajeX);
        int y = Mathf.RoundToInt((tamanyoRedY - 1) * porcentajeY);

        return red[x, y];
    }


    // .
    /*public List<Nodo> ObtenerVecinos (Nodo nodo) 
    {
        List<Nodo> vecinos = new List<Nodo> ();

        for (int x = -1; x < +2; x += 1) 
        {
            for (int y = -1; y < +2; y += 1) 
            {
                if (x != 0 || y != 0)
                {
                    int checadaX = nodo.redX + x;
                    int checadaY = nodo.redY + y;
                }
            }
        }
    }*/


    // Obtenemos la posición de cada uno de los nodos de la red y si se puede caminar por él o no y los almacenamos en la red.
    private void CrearRed ()
    {
        Vector3 esquinaAbjIzq = this.transform.position - Vector3.right * dimensiones.x / 2 - Vector3.forward * dimensiones.y / 2;

        red = new Nodo[tamanyoRedX, tamanyoRedY];

        for (int x = 0; x < tamanyoRedX; x += 1) 
        {
            for (int y = 0; y < tamanyoRedY; y += 1) 
            {
                Vector3 punto = esquinaAbjIzq + Vector3.right * (x * nodoDia + nodoRad) + Vector3.forward * (y * nodoDia + nodoRad);
                bool caminable = !Physics.CheckSphere (punto, nodoRad, obstaculosMsc, QueryTriggerInteraction.Ignore);

                red[x, y] = new Nodo (caminable, punto, x, y);
            }
        }
    }
}