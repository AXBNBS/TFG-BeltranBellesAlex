
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AreaEnemiga : MonoBehaviour
{
    public Enemigo[] enemigos;
    public int perseguidores0, perseguidores1;
    public bool[] tomadosPnt;

    private List<Transform> dentro;
    private Transform violetaTrf;
    private bool apartandose, apartandoseUltFrm, violeta1;
    private float enemigosY;


    // Inicialización de variables.
    private void Start ()
    {
        enemigos = this.GetComponentsInChildren<Enemigo> ();
        perseguidores0 = 0;
        perseguidores1 = 0;
        tomadosPnt = new bool[8];
        dentro = new List<Transform> ();
        violetaTrf = GameObject.Find("Violeta").transform;
        enemigosY = enemigos[0].transform.position.y;
        for (int i = 0; i < enemigos.Length; i += 1) 
        {
            enemigos[i].indice = i;
        }
    }


    // .
    private void Update ()
    {
        apartandose = dentro.Contains (violetaTrf) == true && violetaTrf.position.y < enemigosY;

        if (apartandose == true && apartandoseUltFrm == false)
        {
            ReiniciarArrayBooleanos (false);
        }

        if (apartandose == false && apartandoseUltFrm == true)
        {
            foreach (Enemigo e in enemigos) 
            {
                e.AtacarA (violetaTrf, violeta1);
            }
        }

        apartandoseUltFrm = apartandose;
    }


    // Cada vez que uno de los avatares entre dentro del área enemiga, hacemos que los enemigos empiecen a atacar, además, se dividirán para atacar en el caso de que haya 2 personajes en la zona.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            perseguidores0 = 0;
            perseguidores1 = 0;

            dentro.Add (other.transform);
            for (int e = 0; e < enemigos.Length; e += 1)
            {
                if (dentro.Count > 1 && e >= enemigos.Length / 2)
                {
                    enemigos[e].AtacarA (dentro[1], true);

                    perseguidores1 += 1;
                    violeta1 = dentro[1] == violetaTrf ? true : false;
                }
                else
                {
                    enemigos[e].AtacarA (dentro[0], false);

                    perseguidores0 += 1;
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

                perseguidores0 = 0;
                perseguidores1 = 0;
            }
            else
            {
                foreach (Enemigo e in enemigos)
                {
                    e.AtacarA (dentro[0], false);
                }

                perseguidores0 = enemigos.Length;
                perseguidores1 = 0;
            }
        }
    }


    // .
    public void ReiniciarArrayBooleanos (bool todos) 
    {
        if (todos == false)
        {
            int inicio = violeta1 == false ? 0 : 4;
            int fin = inicio + tomadosPnt.Length / 2;

            for (int t = inicio; t < fin; t += 1) 
            {
                tomadosPnt[t] = false;
            }
        }
        else
        {
            for (int t = 0; t < tomadosPnt.Length; t += 1)
            {
                tomadosPnt[t] = false;
            }
        }
    }
}