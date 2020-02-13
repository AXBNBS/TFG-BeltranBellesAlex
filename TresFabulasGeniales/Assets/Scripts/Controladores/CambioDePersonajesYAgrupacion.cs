
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CambioDePersonajesYAgrupacion : MonoBehaviour
{
    public static CambioDePersonajesYAgrupacion instancia;
    public bool input, juntos;

    [SerializeField] private LayerMask avataresCap, capasSinAvt;
    private MovimientoHistoria2[] personajesMov;
    private Transform[] personajesTrf, detrases, puntosSeg;
    private SeguimientoCamara camara;
    private ColisionesCamara camaraHij;
    private float agrupacionRad;
    private int personajeAct;


    // Inicialización de elementos.
    private void Start ()
    {
        instancia = this;
        juntos = false;
        personajesMov = GameObject.FindObjectsOfType<MovimientoHistoria2> ();
        personajesTrf = new Transform[2];
        detrases = new Transform[2];
        puntosSeg = new Transform[2];
        camara = GameObject.FindObjectOfType<SeguimientoCamara> ();
        camaraHij = camara.GetComponentInChildren<ColisionesCamara> ();
        agrupacionRad = personajesMov[0].offsetXZ * 3;
        personajesTrf[0] = personajesMov[0].transform.GetChild (2);
        personajesTrf[1] = personajesMov[1].transform.GetChild (2);
        detrases[0] = personajesTrf[0].parent.GetChild (0);
        detrases[1] = personajesTrf[1].parent.GetChild (0);
        puntosSeg[0] = personajesTrf[0].parent.GetChild (1);
        puntosSeg[1] = personajesTrf[1].parent.GetChild (1);
        if (personajesMov[0].input == true)
        {
            camara.objetivo = personajesTrf[0];
            camara.detras = detrases[0];
            personajeAct = 0;
        }
        else
        {
            camara.objetivo = personajesTrf[1];
            camara.detras = detrases[1];
            personajeAct = 1;
        }
        camara.transform.position = camara.objetivo.position;
    }


    // Si se permite el input, y se pulsa el botón/tecla de cambio de personaje, cambiamos del personaje actualmente controlado al otro.
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


    // Los personajes dejan de estar juntos.
    public void Separar ()
    {
        personajesMov[0].GestionarSeguimiento (false);
        personajesMov[1].GestionarSeguimiento (false);

        juntos = false;
    }


    // .
    public void PararInput () 
    {
        input = false;
        camara.input = false;
        personajesMov[personajeAct == 0 ? 0 : 1].input = false;
        camaraHij.enabled = false;
    }


    // .
    public void PermitirInput () 
    {
        input = true;
        camara.input = true;
        personajesMov[personajeAct].input = true;
        camaraHij.enabled = true;
    }


    // La cámara pasa a seguir al nuevo personaje y se desactiva el movimiento del otro, en caso de que el botón de cambio se haya pulsado cuando el personaje desde el que se cambia no está en el aire.
    private void CambiarA (int nuevo, int anterior)
    {
        if (personajesMov[anterior].EstaEnElAire () == false)
        {
            PararInput ();

            camara.objetivo = personajesTrf[nuevo];
            camara.detras = detrases[nuevo];
            personajeAct = nuevo;
            if (Vector3.Distance (personajesTrf[nuevo].position, personajesTrf[anterior].position) > 500 || Physics.Linecast (personajesTrf[anterior].position, personajesTrf[nuevo].position, capasSinAvt, QueryTriggerInteraction.Ignore) == true)
            {
                camara.transicionando = true;

                Fundido.instancia.FundidoAPosicion (personajesTrf[nuevo].position);
            }

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
}