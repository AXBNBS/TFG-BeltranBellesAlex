
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CambioDePersonajesYAgrupacion : MonoBehaviour
{
    public static CambioDePersonajesYAgrupacion instancia;
    public bool input, juntos;

    [SerializeField] private LayerMask avataresCap;
    private MovimientoHistoria2[] personajesMov;
    private Transform[] personajesTrf, detrases, puntosSeg;
    private SeguimientoCamara camara;
    private float agrupacionRad;


    // Inicialización de elementos y desactivación del movimiento de uno de los personajes.
    private void Start ()
    {
        instancia = this;
        juntos = false;
        personajesMov = GameObject.FindObjectsOfType<MovimientoHistoria2> ();
        personajesTrf = new Transform[2];
        detrases = new Transform[2];
        puntosSeg = new Transform[2];
        camara = GameObject.FindObjectOfType<SeguimientoCamara> ();
        agrupacionRad = personajesMov[0].offsetXZ * 3;
        personajesTrf[0] = personajesMov[0].transform;
        personajesTrf[1] = personajesMov[1].transform;
        detrases[0] = personajesTrf[0].GetChild (0);
        detrases[1] = personajesTrf[1].GetChild (0);
        puntosSeg[0] = personajesTrf[0].GetChild (1);
        puntosSeg[1] = personajesTrf[1].GetChild (1);
        if (personajesMov[0].input == true)
        {
            camara.objetivo = personajesTrf[0];
            camara.detras = detrases[0];
        }
        else
        {
            camara.objetivo = personajesTrf[1];
            camara.detras = detrases[1];
        }
        camara.transform.position = camara.objetivo.position;
    }


    // Si se permite el input, y se pulsa el botón/tecla de cambio de personaje, y cambiamos del personaje actual al otro.
    private void Update ()
    {
        if (input == true)
        {
            if (Input.GetButtonDown ("Juntar personajes") == true)
            {
                if (juntos == false)
                {
                    Juntar ();
                }
                else
                {
                    Separar ();
                }
            }
            if (Input.GetButtonDown ("Cambio personaje") == true)
            {
                if (personajesMov[0].input == true)
                {
                    CambiarA (1, 0);
                }
                else
                {
                    CambiarA (0, 1);
                }
                Separar ();
            }
        }
    }


    // Para ver el radio de agrupación de los gatos.
    /*private void OnDrawGizmos ()
    {
        Gizmos.DrawWireSphere (camara.objetivo.position, agrupacionRad);
    }*/


    // La cámara pasa a seguir al nuevo personaje y se desactiva el movimiento del otro.
    private void CambiarA (int nuevo, int anterior)
    {
        if (personajesMov[anterior].EstaEnElAire () == false)
        {
            camara.objetivo = personajesTrf[nuevo];
            camara.detras = detrases[nuevo];
            personajesMov[anterior].input = false;
            personajesMov[nuevo].input = true;

            personajesMov[nuevo].GestionarCambio ();
        }
    }


    // Si en un cierto radio se encuantra el otro avatar, haremos que este comienze a seguir a aquel que esté siendo controlado por el jugador.
    private void Juntar ()
    {
        Collider[] encontrado = Physics.OverlapSphere (camara.objetivo.position, agrupacionRad, avataresCap, QueryTriggerInteraction.Ignore);

        if (encontrado.Length > 1)
        {
            if (personajesMov[0].input == true)
            {
                personajesMov[1].GestionarSeguimiento (true);
            }
            else
            {
                personajesMov[0].GestionarSeguimiento (true);
            }
            
            juntos = true;
        }
    }


    // Los personajes dejan de estar juntos.
    private void Separar ()
    {
        personajesMov[0].GestionarSeguimiento (false);
        personajesMov[1].GestionarSeguimiento (false);

        juntos = false;
    }
}