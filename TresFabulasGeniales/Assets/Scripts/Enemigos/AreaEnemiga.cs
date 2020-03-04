
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AreaEnemiga : MonoBehaviour
{
    public Enemigo[] enemigos;

    private List<Transform> dentro;


    // Inicialización de variables.
    private void Start ()
    {
        enemigos = this.GetComponentsInChildren<Enemigo> ();
        dentro = new List<Transform> ();
    }


    // .
    private void Update ()
    {
        
    }


    // Cada vez que uno de los avatares entre dentro del área enemiga, hacemos que los enemigos empiecen a atacar, además, se dividirán para atacar en el caso de que haya 2 personajes en la zona.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            dentro.Add (other.transform);
            for (int e = 0; e < enemigos.Length; e += 1)
            {
                if (dentro.Count > 1 && e >= enemigos.Length / 2)
                {
                    enemigos[e].AtacarA (dentro[1]);
                }
                else
                {
                    enemigos[e].AtacarA (dentro[0]);
                }
            }
        }
    }


    // Si un avatar ha salido del área enemiga, haremos que los enemigos se centren en el que queda o, en caso de que no quede ninguno, simplemente paren de atacar.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            dentro.Remove (other.transform);
            if (dentro.Count == 0)
            {
                foreach (Enemigo e in enemigos)
                {
                    e.Parar ();
                }
            }
            else
            {
                foreach (Enemigo e in enemigos)
                {
                    e.AtacarA (dentro[0]);
                }
            }
        }
    }
}