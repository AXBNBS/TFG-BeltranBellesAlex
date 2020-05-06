
using System.Collections.Generic;
using UnityEngine;



[RequireComponent (typeof (SphereCollider))]
public class AreaBichosPegajosos : MonoBehaviour
{
    private BichoPegajoso[] bichos;
    private int vivos;
    private List<Transform> dentro;


    // Inicialización de variables.
    private void Awake ()
    {
        bichos = this.GetComponentsInChildren<BichoPegajoso> ();
        vivos = bichos.Length;
        dentro = new List<Transform> ();
        this.GetComponent<Collider>().isTrigger = true;
    }


    // Cada vez que uno de los avatares entre dentro del área enemiga, hacemos que los enemigos empiecen a atacar, además, se dividirán para atacar en el caso de que haya 2 personajes en la zona.
    private void OnTriggerEnter (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true)
        {
            dentro.Add (other.transform);
            DividirEnemigos ();
        }
    }


    // Si un avatar ha salido del área enemiga, haremos que los enemigos se centren en el que queda o, en caso de que no quede ninguno, simplemente paren de atacar.
    private void OnTriggerExit (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true)
        {
            dentro.Remove (other.transform);

            if (dentro.Count == 0)
            {
                foreach (BichoPegajoso b in bichos)
                {
                    if (b.isActiveAndEnabled == true)
                    {
                        b.Parar ();
                    }
                }
            }
            else
            {
                foreach (BichoPegajoso b in bichos)
                {
                    if (b.isActiveAndEnabled == true)
                    {
                        b.AtacarA (dentro[0]);
                    }
                }
            }
        }
    }


    // Hacemos que la mitad de los enemigos se dirigan hacia uno de los jugadores en caso de que ambos estén en la zona, sino todos irán hacia el único que haya.
    private void DividirEnemigos ()
    {
        int indice = 0;

        foreach (BichoPegajoso b in bichos)
        {
            if (b.isActiveAndEnabled == true)
            {
                if (dentro.Count == 1 || indice < vivos / 2)
                {
                    b.AtacarA (dentro[0]);
                }
                else
                {
                    b.AtacarA (dentro[1]);
                }
                indice += 1;
            }
        }
    }
}