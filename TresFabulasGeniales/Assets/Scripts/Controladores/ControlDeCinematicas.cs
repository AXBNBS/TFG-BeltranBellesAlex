
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;



public class ControlDeCinematicas : MonoBehaviour
{
    [SerializeField] private int[] cuadros;
    [SerializeField] private string[] texto;
    [SerializeField] private float[] cuadrosSeg, textoSeg;
    [SerializeField] private Animator[] actores;
    private bool seguir;
    private int cuadrosPas, cuadrosInd, textoInd, letrasEsc;
    private string personaje, frase;
    private PlayableDirector director;
    private GameObject textoPan;
    private Text[] mostradoTxt;
    private Salud[] saludScr;


    // Inicializamos variables, desactivamos los scripts de salud (y con ellos la barra) y detenemos el input del jugador.
    private void Start ()
    {
        GameObject[] avataresObj = GameObject.FindGameObjectsWithTag ("Jugador");
        
        director = this.GetComponent<PlayableDirector> ();
        textoPan = Fundido.instancia.textoPan;
        mostradoTxt = textoPan.GetComponentsInChildren<Text> ();
        saludScr = new Salud[2] { avataresObj[0].GetComponent<Salud> (), avataresObj[1].GetComponent<Salud> () };
        foreach (Salud s in saludScr) 
        {
            s.enabled = false;
        }

        CambioDePersonajesYAgrupacion.instancia.PararInput ();
    }


    // Si el panel no esta activo y se ha llegado a un punto de la cinemática en el que hay que mostrar texto, lo activamos y empezamos a hacerlo. Si por el contrario no está activo y pulsamos el botón para pasar texto, mostraremos completamente
    //la línea actual o pasaremos a la siguiente según la situación. Además, llamamos a la función que se asegura de que la cinemática no prosiga si el jugador no ha leído las suficientes cajas de texto. La parte final sirve para omitir la
    //secuencia, para agilizar el testeo si hace falta.
    private void Update ()
    {
        if (textoPan.activeSelf == false && textoInd < texto.Length && textoSeg[textoInd] < director.time && this.IsInvoking () == false) 
        {
            textoPan.SetActive (true);
            SepararTexto ();
            this.InvokeRepeating ("EscribirTexto", 0, 0.04f);
        }
        if (textoPan.activeSelf == true && (frase.Length - 5) > letrasEsc && Input.GetButtonDown ("Interacción") == true) 
        {
            if (seguir == false) 
            {
                ParrafoCompleto ();
            }
            else 
            {
                cuadrosPas += 1;
                seguir = false;

                textoPan.SetActive (false);
            }
        }
        ControlarFlujo ();

        if (Input.GetKeyDown (KeyCode.L) == true) 
        {
            director.time = director.duration - 0.1f;
            textoInd = texto.Length;
            cuadrosPas = cuadros[cuadros.Length - 1];

            textoPan.SetActive (false);
            director.Resume ();
        }
    }


    // Si la cinemática no esta pausada, decidimos si hacerlo o no en función de si el jugador ha leído suficientes cuadros de texto para el tiempo de cinemática que lleva. En el caso contrario, miramos si se cumple el número de cuadros pasados
    //necesarios para seguir con la secuencia.
    private void ControlarFlujo () 
    {
        if (PlayState.Playing == director.state) 
        {
            if (cuadros[cuadrosInd] > cuadrosPas) 
            {
                if (cuadrosSeg[cuadrosInd] < director.time) 
                {
                    director.Pause ();
                }
            }
            else 
            {
                if ((cuadros.Length - 1) > cuadrosInd)
                {
                    cuadrosInd += 1;
                }
            }
        }
        else 
        {
            if (cuadros[cuadrosInd] <= cuadrosPas) 
            {
                director.Resume ();

                if ((cuadros.Length - 1) > cuadrosInd) 
                {
                    cuadrosInd += 1;
                }
            }
        }
    }


    // El texto se separa de manera que se asigna la primera parte del mismo al nombre del interlocutor mostrado en el panel y el resto a la frase que este diga.
    private void SepararTexto () 
    {
        bool comenzando = true;

        mostradoTxt[0].text = "";
        mostradoTxt[1].text = "";
        personaje = "";
        frase = "";
        foreach (char c in texto[textoInd])
        {
            if (comenzando == false)
            {
                if (c != '*')
                {
                    frase += c;
                }
                else
                {
                    frase += '\n';
                }
            }
            else
            {
                if (c != '=')
                {
                    personaje += c;
                }
                else
                {
                    comenzando = false;
                }
            }
        }
        mostradoTxt[0].text = personaje;
    }


    // Cuando se muestra la totalidad de la línea actual, cambiaremos un booleano para que lo refleje, restableceremos las letras escritas a 0, nos aseguraremos de que se muestra todo (sólo sería necesario si hemos pulsado el botón antes de que se
    //muestre toda la línea), actualizamos el índice para acceder a la siguiente posición de la array del texto y cancelamos el invoke que actualizaba el texto mostrado cada pocas centésimas de segundo.
    private void ParrafoCompleto () 
    {
        seguir = true;
        letrasEsc = 0;
        mostradoTxt[1].text = frase;
        textoInd += 1;

        this.CancelInvoke ();
    }


    // Un invoke que se llama con una frecuencia determinada cuando empieza a mostrarse una línea de texto: en cada nueva llamada muestra una nueva letra y, si llega al final del "párrafo", este se marca como completo.
    private void EscribirTexto () 
    {
        mostradoTxt[1].text += frase[letrasEsc];
        letrasEsc += 1;
        if (letrasEsc == frase.Length) 
        {
            ParrafoCompleto ();
        }
    }


    // Se llama mediante un evento al finalizar la cinemática, se encarga de reactivar los scripts de salud (y con ello también la barra) y permitir el input de los personajes de nuevo. Al final, desactivamos el objeto actual.
    public void FinDeCinematica ()
    {
        foreach (Salud s in saludScr)
        {
            s.enabled = true;
        }

        CambioDePersonajesYAgrupacion.instancia.PermitirInput ();
        this.gameObject.SetActive (false);
    }
}