
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EncontrarCamino : MonoBehaviour
{
    private Red red;


    // .
    private void Awake ()
    {
        red = this.GetComponent<Red> ();
    }


    // .
    private void BuscarCamino (Vector3 inicio, Vector3 objetivo) 
    {
        Nodo inicioNod = red.DevolverNodoDePosicion (inicio);
        Nodo objetivoNod = red.DevolverNodoDePosicion (objetivo);
        List<Nodo> abiertos = new List<Nodo> ();
        HashSet<Nodo> cerrados = new HashSet<Nodo> ();

        abiertos.Add (inicioNod);

        while (abiertos.Count > 0) 
        {
            Nodo actual = abiertos[0];

            for (int i = 1; i < abiertos.Count; i += 1) 
            {
                if (abiertos[i].costeF < actual.costeF || (abiertos[i].costeF == actual.costeF && abiertos[i].costeH < actual.costeH)) 
                {
                    actual = abiertos[i];
                }
            }

            abiertos.Remove (actual);
            cerrados.Add (actual);

            if (actual == objetivoNod) 
            {
                return;
            }


        }
    }
}