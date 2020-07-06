
using UnityEngine;
using UnityEngine.SceneManagement;



public class Fundido : MonoBehaviour
{
    public static Fundido instancia;
    [HideInInspector] public bool animando;
    [HideInInspector] public Scene escena;
    [HideInInspector] public Animator animador;
    public GameObject saludPan, textoPan;

    private int escenaInd;
    private bool escena3;
    private enum TipoFundido { posicion, escena, pausa };
    private TipoFundido tipo;
    private Vector3 posicion;
    private Camera juegoCam, pausaCam;
    private SeguimientoCamara juegoCamScr;
    private MovimientoHistoria3 movimiento3Scr;


    // Inicialización de variables y gestiones pertinentes, según si hay ya una instancia del fundido o no.
    private void Awake ()
    {
        if (instancia != null)
        {
            instancia.escena = SceneManager.GetActiveScene ();

            instancia.saludPan.SetActive (false);
            instancia.EncontrarReferencias ();
            GameObject.Destroy (this.transform.parent.parent.gameObject);
        }
        else
        {
            instancia = this;
            animando = true;
            escena = SceneManager.GetActiveScene ();
            animador = this.GetComponent<Animator> ();

            EncontrarReferencias ();
            GameObject.DontDestroyOnLoad (this.transform.parent.parent.gameObject);
            saludPan.SetActive (false);
        }
    }


    // Cuando el nivel se haya cargado completamente, nos aseguramos de que el animador vuelve a su velocidad normal.
    private void OnLevelWasLoaded (int level) 
    {
        instancia.animador.speed = 1;
    }


    // Para moverse rápido en las distintas escenas, podemos pulsar P o O para ir a la siguiente o la anterior respectivamente. También es posible pausar pulsando el botón necesario y si no estamos en la escena del menú.
    private void Update ()
    {
        if (animando == false && textoPan.activeSelf == false) 
        {
            if (Input.GetKeyDown (KeyCode.P) == true && (SceneManager.sceneCountInBuildSettings - 1) != escena.buildIndex)
            {
                FundidoAEscena (escena.buildIndex + 1);
            }
            if (Input.GetKeyDown (KeyCode.O) == true && escena.buildIndex > 1)
            {
                FundidoAEscena (escena.buildIndex - 1);
            }
            if (Input.GetButtonDown ("Pausar") == true && AlmacenDatos.instancia.menuEsc != escena.name)
            {
                FundidoPausa ();
            }
        }
    }


    // Activamos un fundido que permite mover la cámara a una posición distinta una vez la imagen esté del todo oscurecida.
    public void FundidoAPosicion (Vector3 pos) 
    {
        animador.SetTrigger ("fundido");

        animando = true;
        tipo = TipoFundido.posicion;
        posicion = pos;
    }


    // Activamos el fundido para cargar una escena nueva cuando la pantalla esté completamente en negro.
    public void FundidoAEscena (int escena) 
    {
        animador.SetTrigger ("fundido");

        animando = true;
        tipo = TipoFundido.escena;
        escenaInd = escena;
    }


    // Activamos el fundido para ir a la pantalla de pausa una vez la imagen se oscurezca del todo. También desactivamos el input.
    public void FundidoPausa () 
    {
        animador.SetTrigger ("fundido");
        ControlarInput (false);

        animando = true;
        tipo = TipoFundido.pausa;
        Time.timeScale = 0;
    }


    // Una vez comienza una nueva escena, y si esta no es la del menú, encontramos las referencias a las cámaras del juego y del menú, desactivamos la segunda y obtenemos una última referencia al script de seguimiento de la del jugador. También 
    //obtenemos una referencia al script de movimiento de la historia 3 (si estamos en esa escena) para desactivar su input luego si procede.
    public void EncontrarReferencias ()
    {
        if (AlmacenDatos.instancia.menuEsc != escena.name)
        {
            GameObject[] avataresObj = GameObject.FindGameObjectsWithTag ("Jugador");

            juegoCam = GameObject.FindGameObjectWithTag("CamaraPrincipal").GetComponent<Camera> ();
            juegoCam.enabled = true;
            pausaCam = GameObject.FindGameObjectWithTag("CamaraPausa").GetComponent<Camera> ();
            pausaCam.enabled = false;
            juegoCamScr = juegoCam.transform.parent.GetComponent<SeguimientoCamara> ();
            escena3 = avataresObj.Length == 1;
            if (escena3 == true)
            {
                movimiento3Scr = avataresObj[0].GetComponent<MovimientoHistoria3> ();
            }
        }
    }


    // Si no estamos en la escena 3, paramos o devolvemos el input de los avatares en base a la función ya definida en el script que gestiona sus cambios. Si sí estamos en la 3, simplemente actualizamos el valor del booleano que indica si hay input
    //o no según corresponda.
    private void ControlarInput (bool activar)
    {
        if (escena3 == false)
        {
            if (activar == false)
            {
                CambioDePersonajesYAgrupacion.instancia.PararInput ();
            }
            else
            {
                CambioDePersonajesYAgrupacion.instancia.PermitirInput ();
            }
        }
        else
        {
            movimiento3Scr.input = activar;
        }
    }


    // Función que se llama al completar el fade out: se encarga de cambiar la cámara de posición, cambiar o volver de la pantalla de pausa o cargar la siguiente escena, según convenga.
    private void EscenaOscura () 
    {
        switch (tipo) 
        {
            case TipoFundido.posicion:
                juegoCamScr.transform.position = posicion;
                juegoCamScr.transicionando = false;

                CambioDePersonajesYAgrupacion.instancia.ActualizarMedidorSalud ();

                break;
            case TipoFundido.pausa:
                bool pausa = !Menu.instancia.isActiveAndEnabled;

                juegoCam.enabled = !pausa;
                pausaCam.enabled = pausa;

                Menu.instancia.MenuVisible (pausa);
                if (escena3 == false) 
                {
                    saludPan.SetActive (!saludPan.activeSelf);
                }

                if (pausa == false) 
                {
                    Time.timeScale = 1;

                    ControlarInput (true);
                }

                break;
            default:
                Time.timeScale = 1;
                animador.speed = 0;

                SceneManager.LoadScene (escenaInd);

                break;
        }
    }


    // Función que se llama al completar el fade in, se encarga de iniciar el diálogo entre los narradores si hemos vuelto a la escena inicial y de actualizar el valor de la variable que indica que la animación de fade in/out está sucediendo.
    private void EscenaVisible () 
    {
        animando = false;

        if (AlmacenDatos.instancia.muerte == true)
        {
            Hablar narradorScr = GameObject.FindObjectOfType<Hablar> ();

            narradorScr.input = true;
            
            narradorScr.IniciarDialogo ();
        }
    }
}