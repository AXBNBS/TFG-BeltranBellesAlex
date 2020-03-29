﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AreaBichosPegajosos : MonoBehaviour
{
    private BichoPegajoso[] bichos;
    private int vivos;
    private List<Transform> dentro;
    private float enemigosY;
    private MovimientoHistoria2[] jugadoresMov;


    // Inicialización de variables.
    private void Awake ()
    {
        bichos = this.GetComponentsInChildren<BichoPegajoso> ();
        vivos = bichos.Length;
        dentro = new List<Transform> ();
        /*enemigos = this.GetComponentsInChildren<Enemigo>();
        perseguidores0 = 0;
        perseguidores1 = 0;
        vivos = enemigos.Length;
        tomadosPnt = new bool[8];
        dentro = new List<Transform>();
        violetaTrf = GameObject.Find("Violeta").transform;
        apartandose = true;
        apartandoseUltFrm = true;
        violeta1 = false;
        dentroVio = false;
        enemigosY = enemigos[0].transform.GetChild(3).position.y;
        jugadoresMov = GameObject.FindObjectsOfType<MovimientoHistoria2>();
        for (int i = 0; i < enemigos.Length; i += 1)
        {
            enemigos[i].prioridad = i;
        }*/
    }


    // Si los enemigos estaban persiguiendo a Violeta pero han pasado a apartarse (por estar ella en el aire), reiniciamos la parte que le corresponde a ella en el array de booleanos, ya que ahora los enemigos no irán a por esos puntos. Si en cambio
    //los enemigos estaban apartándose y tienen que volver a perseguirla, nos aseguramos de que aquellos enemigos que la tengan como blanco lo hagan.
    private void Update ()
    {
        /*if (vivos == 0)
        {
            jugadoresMov[0].CombateTerminado();
            jugadoresMov[1].CombateTerminado();
            this.gameObject.SetActive(false);
        }

        apartandose = violetaTrf.position.y > enemigosY;

        if (dentroVio == true)
        {
            if (apartandose == true && apartandoseUltFrm == false)
            {
                ReiniciarArrayBooleanos(false);

                foreach (Enemigo e in enemigos)
                {
                    if (e.avatarTrf == violetaTrf)
                    {
                        e.acercarse = false;

                        e.SinBlanco();
                    }
                }
            }

            if (apartandose == false && apartandoseUltFrm == true)
            {
                APor(violetaTrf, violeta1);
            }
        }

        apartandoseUltFrm = apartandose;*/
    }


    // Cada vez que uno de los avatares entre dentro del área enemiga, hacemos que los enemigos empiecen a atacar, además, se dividirán para atacar en el caso de que haya 2 personajes en la zona, se activará el booleano que indica que Violeta está
    //en la zona en caso necesario.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true)
        {
            dentro.Add (other.transform);
            DividirEnemigos ();
        }
    }


    // Si un avatar ha salido del área enemiga, haremos que los enemigos se centren en el que queda o, en caso de que no quede ninguno, simplemente paren de atacar. Si no hay nadie desactivamos el booleano que indica que Violeta está en el área, y
    //si lo hay, miraremos si el avatar que ha salido corresponde al de Violeta o no para hacerlo.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true)
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


    // Reiniciamos la array de booleanos, poniéndolos todos a falso o sólo la mitad que corresponda a Violeta.
    /*public void ReiniciarArrayBooleanos(bool todos)
    {
        if (todos == false)
        {
            int inicio = violeta1 == false ? 0 : tomadosPnt.Length / 2;
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
    }*/


    // Tras eliminar a un enemigo, hacemos que sus compañeros que compartían objetivo se reorganicen para atacar al mismo.
    /*public void APor(Transform objetivo, bool uno)
    {
        if (uno == false)
        {
            perseguidores0 = 0;
        }
        else
        {
            perseguidores1 = 0;
        }
        foreach (Enemigo e in enemigos)
        {
            if (e.Vencido() == false && e.avatarTrf == objetivo)
            {
                if (uno == false)
                {
                    perseguidores0 += 1;
                }
                else
                {
                    perseguidores1 += 1;
                }

                //print (this.name + ": a por " + objetivo.name);
                e.AtacarA(objetivo, uno);
            }
        }
    }*/


    // Todas las ranuras alrededor del avatar pasan a estar desocupadas.
    /*public void LiberarRanuras(Transform avatar)
    {
        foreach (Enemigo e in enemigos)
        {
            if (e.avatarTrf == avatar)
            {
                e.SinBlanco();
            }
        }
    }*/


    // .
    /*public void Alerta(Transform enemigo, Transform avatar)
    {
        if (corrutinaActVio == false && violetaTrf == avatar)
        {
            this.StartCoroutine("EsquivarVioleta", enemigo);
        }
        if (corrutinaActAbd == false && violetaTrf != avatar)
        {
            this.StartCoroutine("EsquivarAbedul", enemigo);
        }
    }*/


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


    // .
    /*private IEnumerator EsquivarVioleta(Transform enemigo)
    {
        corrutinaActVio = true;

        yield return new WaitForSeconds(0.3f);

        foreach (Enemigo e in enemigos)
        {
            if (e.danyado == false && e.Vencido() == false && e.transform != enemigo && e.avatarTrf == violetaTrf && Vector3.Distance(e.transform.position, enemigo.position) < alertaRng)
            {
                e.Saltar();
            }
        }

        corrutinaActVio = false;
    }*/


    // .
    /*private IEnumerator EsquivarAbedul(Transform enemigo)
    {
        corrutinaActAbd = true;

        yield return new WaitForSeconds(0.3f);

        foreach (Enemigo e in enemigos)
        {
            if (e.danyado == false && e.Vencido() == false && e.transform != enemigo && e.avatarTrf != violetaTrf && Vector3.Distance(e.transform.position, enemigo.position) < alertaRng)
            {
                e.Saltar();
            }
        }

        corrutinaActAbd = false;
    }*/
}