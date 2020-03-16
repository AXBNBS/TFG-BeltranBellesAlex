
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AreaEnemiga : MonoBehaviour
{
    public Enemigo[] enemigos;
    public int perseguidores0, perseguidores1, vivos;
    public bool[] tomadosPnt;

    private List<Transform> dentro;
    private Transform violetaTrf;
    private bool apartandose, apartandoseUltFrm, violeta1, dentroVio;
    private float enemigosY;
    private MovimientoHistoria2[] jugadoresMov;


    // Inicialización de variables.
    private void Start ()
    {
        enemigos = this.GetComponentsInChildren<Enemigo> ();
        perseguidores0 = 0;
        perseguidores1 = 0;
        vivos = enemigos.Length;
        tomadosPnt = new bool[8];
        dentro = new List<Transform> ();
        violetaTrf = GameObject.Find("Violeta").transform;
        apartandose = true;
        apartandoseUltFrm = true;
        violeta1 = false;
        dentroVio = false;
        enemigosY = enemigos[0].transform.position.y;
        jugadoresMov = GameObject.FindObjectsOfType<MovimientoHistoria2> ();
        for (int i = 0; i < enemigos.Length; i += 1) 
        {
            enemigos[i].indice = i;
        }
    }


    // Si los enemigos estaban persiguiendo a Violeta pero han pasado a apartarse (por estar ella en el aire), reiniciamos la parte que le corresponde a ella en el array de booleanos, ya que ahora los enemigos no irán a por esos puntos. Si en cambio
    //los enemigos estaban apartándose y tienen que volver a perseguirla, nos aseguramos de que aquellos enemigos que la tengan como blanco lo hagan.
    private void Update ()
    {
        if (vivos == 0) 
        {
            this.gameObject.SetActive (false);
            jugadoresMov[0].CombateTerminado ();
            jugadoresMov[1].CombateTerminado ();
        }

        apartandose = violetaTrf.position.y > enemigosY;

        if (dentroVio == true)
        { 
            if (apartandose == true && apartandoseUltFrm == false)
            {
                ReiniciarArrayBooleanos (false);
            }

            if (apartandose == false && apartandoseUltFrm == true)
            {
                APor (violetaTrf, violeta1);
            }
        }

        apartandoseUltFrm = apartandose;
    }


    // Cada vez que uno de los avatares entre dentro del área enemiga, hacemos que los enemigos empiecen a atacar, además, se dividirán para atacar en el caso de que haya 2 personajes en la zona, se activará el booleano que indica que Violeta está
    //en la zona en caso necesario.
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
                    if (dentro[1].name == "Abedul" || apartandose == false) 
                    {
                        enemigos[e].AtacarA (dentro[1], true);
                    }

                    perseguidores1 += 1;
                    violeta1 = dentro[1] == violetaTrf ? true : false;
                }
                else
                {
                    if (dentro[0].name == "Abedul" || apartandose == false)
                    {
                        enemigos[e].AtacarA (dentro[0], false);
                    }

                    perseguidores0 += 1;
                }
            }
            if (dentro[dentro.Count - 1] == violetaTrf) 
            {
                dentroVio = true;
            }
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
                foreach (Enemigo e in enemigos)
                {
                    e.Parar ();
                }
                ReiniciarArrayBooleanos (true);

                perseguidores0 = 0;
                perseguidores1 = 0;
                dentroVio = false;
            }
            else
            {
                foreach (Enemigo e in enemigos)
                {
                    e.AtacarA (dentro[0], false);
                }

                perseguidores0 = enemigos.Length;
                perseguidores1 = 0;
                if (dentro[0] != violetaTrf) 
                {
                    dentroVio = false;
                }
            }
        }
    }


    // Reiniciamos la array de booleanos, poniéndolos todos a falso o sólo la mitad que corresponda a violeta.
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


    // Tras eliminar a un enemigo, hacemos que sus compañeros que compartían objetivo se reorganicen para atacar al mismo.
    public void APor (Transform objetivo, bool uno) 
    {
        foreach (Enemigo e in enemigos) 
        {
            if (e.isActiveAndEnabled == true && e.avatarTrf == objetivo) 
            {
                e.AtacarA (objetivo, uno);
            }
        }
    }
}