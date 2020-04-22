
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CambioDePersonajesYAgrupacion : MonoBehaviour
{
    public static CambioDePersonajesYAgrupacion instancia;
    public bool input, juntos, violetaAct;

    [SerializeField] private LayerMask capasSinAvt;
    private Hablar[] personajesHbl;
    private MovimientoHistoria2[] personajesMov;
    private Ataque[] personajesAtq;
    private Salud[] personajesSld;
    private Empujar abedulEmp;
    private Transform[] personajesTrf, detrases, puntosSeg;
    private SeguimientoCamara camara;
    private ColisionesCamara camaraHij;
    private int personajeAct;
    private CharacterController[] personajesCtr;


    // Inicialización de elementos.
    private void Start ()
    {
        instancia = this;
        juntos = false;
        violetaAct = true;
        personajesHbl = GameObject.FindObjectsOfType<Hablar> ();
        personajesMov = new MovimientoHistoria2[2];
        personajesMov[0] = personajesHbl[0].GetComponent<MovimientoHistoria2> ();
        personajesMov[1] = personajesHbl[1].GetComponent<MovimientoHistoria2> ();
        personajesAtq = new Ataque[2];
        personajesAtq[0] = personajesMov[0].GetComponent<Ataque> ();
        personajesAtq[1] = personajesMov[1].GetComponent<Ataque> ();
        personajesSld = new Salud[2];
        personajesSld[0] = personajesMov[0].GetComponent<Salud> ();
        personajesSld[1] = personajesMov[1].GetComponent<Salud> ();
        if (personajesMov[0].GetComponent<Empujar> () != null)
        {
            abedulEmp = personajesMov[0].GetComponent<Empujar> ();
        }
        else 
        {
            abedulEmp = personajesMov[1].GetComponent<Empujar> ();
        }
        personajesTrf = new Transform[2];
        detrases = new Transform[2];
        puntosSeg = new Transform[2];
        camara = GameObject.FindObjectOfType<SeguimientoCamara> ();
        camaraHij = camara.GetComponentInChildren<ColisionesCamara> ();
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
        personajesCtr = new CharacterController[2];
        personajesCtr[0] = personajesMov[0].GetComponent<CharacterController> ();
        personajesCtr[1] = personajesMov[1].GetComponent<CharacterController> ();
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

                violetaAct = !violetaAct;
            }
        }
    }


    // Para ver el radio de agrupación de los gatos.
    /*private void OnDrawGizmos ()
    {
        Gizmos.color = Color.red;
        
        Gizmos.DrawWireSphere (personajesCtr[personajeAct].bounds.center, agrupacionRad);
    }*/


    // Los personajes dejan de estar juntos.
    public void Separar ()
    {
        personajesMov[0].GestionarSeguimiento (false);
        personajesMov[1].GestionarSeguimiento (false);
    }


    // Recibe un script de salud de uno de los personajes y devuelve true si este se corresponde con el del personaje actualmente controlado.
    public bool ActivarInputAutorizado (Salud saludScr) 
    {
        int indiceScr = saludScr == personajesSld[0] ? 0 : 1;

        return (indiceScr == personajeAct);
    }


    // Si la cámara ha llegado al personaje que ha de seguir, se activará la IA de combate del no controlado (esto sólo acabará sucediendo si hay enemigos cerca).
    public void ActivarIACombate () 
    {
        personajesMov[personajeAct == 0 ? 1 : 0].ComenzarAtaque ();
    }


    // Una vez la cámara ha alcanzado el nuevo personaje controlado, permitimos de nuevo el input en este script, el movimiento de la cámara, el ataque y movimiento de este nuevo personaje (si es Abedul, también el empuje) y que la cámara se ajuste
    //en base a las colisiones con el entorno.
    public void PermitirInput ()
    {
        input = true;
        camara.input = true;
        personajesHbl[personajeAct].input = true;
        personajesMov[personajeAct].input = true;
        personajesAtq[personajeAct].input = true;
        if (violetaAct == false)
        {
            abedulEmp.input = true;
        }
        camaraHij.enabled = true;

        personajesMov[personajeAct].ComprobacionNaifes ();
    }


    // Al cambiar de personaje, dejamos de permitir el input en este script, impedimos el movimiento de la cámara, que el personaje controlado hasta ahora pueda moverse y atacar (también empujar si este era Abedul) y que la cámara se ajuste en base
    //a las colisiones con el entorno.
    public void PararInput () 
    {
        input = false;
        camara.input = false;
        personajesHbl[personajeAct].input = false;
        personajesMov[personajeAct].input = false;
        personajesAtq[personajeAct].input = false;
        if (violetaAct == false) 
        {
            abedulEmp.input = false;
        }
        camaraHij.enabled = false;
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
            camara.cambioCmp = false;
            if (Vector3.Distance (personajesTrf[nuevo].position, personajesTrf[anterior].position) > 500 || Physics.Linecast (personajesTrf[anterior].position, personajesTrf[nuevo].position, capasSinAvt, QueryTriggerInteraction.Ignore) == true)
            {
                camara.transicionando = true;

                Fundido.instancia.FundidoAPosicion (personajesTrf[nuevo].position);
            }
            else 
            {
                camara.desplazandose = true;
            }

            personajesMov[nuevo].GestionarCambio ();
        }
    }


    // Si en un cierto radio se encuantra el otro avatar, haremos que este comienze a seguir a aquel que esté siendo controlado por el jugador.
    private void Juntar ()
    {
        if (personajesMov[personajeAct].companyeroCer == true)
        {
            personajesMov[personajeAct == 0 ? 1 : 0].GestionarSeguimiento (true);
        }
    }
}