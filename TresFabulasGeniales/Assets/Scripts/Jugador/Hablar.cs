﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class Hablar : MonoBehaviour
{
    public bool input;
    public string[] texto;
    public List<Texto> hablables;

    [SerializeField] private int rotacionVel;
    private float tiempoPas;
    private int parrafoAct, letrasEsc;
    private string personaje, frase;
    private bool escena0;
    private Quaternion rotacionObj;
    private GameObject panelTxt;
    private Text[] mostradoTxt;
    private CapsuleCollider capsulaNPC;
    private MovimientoHistoria1 personajeMov1Scr;
    private MovimientoHistoria2 personajeMov2Scr;
    private MovimientoHistoria3 personajeMov3Scr;
    private Ataque ataqueScr;
    private Empujar empujarScr;
    private CambioDePersonajesYAgrupacion cambioScr;
    private SeguimientoCamara camaraScr;
    private Texto textoScr;


    // Inicialización de variables y desactivación del panel. Además, obtendremos unos componentes u otros dependiendo de la historia en la que nos encontremos.
    private void Start ()
    {
        hablables = new List<Texto> ();
        tiempoPas = 0;
        parrafoAct = 0;
        letrasEsc = 0;
        escena0 = SceneManager.GetActiveScene().name == "CasaBeltranuki";
        panelTxt = GameObject.FindGameObjectWithTag("Interfaz").transform.GetChild(0).GetChild(0).gameObject;
        mostradoTxt = panelTxt.GetComponentsInChildren<Text> ();
        if (escena0 == false)
        {
            texto = null;
            personajeMov1Scr = this.GetComponent<MovimientoHistoria1> ();
            if (personajeMov1Scr == null)
            {
                personajeMov2Scr = this.GetComponent<MovimientoHistoria2> ();
                if (personajeMov2Scr != null)
                {
                    ataqueScr = this.GetComponent<Ataque> ();
                    empujarScr = this.GetComponent<Empujar> ();
                    cambioScr = GameObject.FindGameObjectWithTag("Controlador").GetComponent<CambioDePersonajesYAgrupacion> ();
                }
                else
                {
                    personajeMov3Scr = this.GetComponent<MovimientoHistoria3> ();
                }
            }
            camaraScr = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform.parent.GetComponent<SeguimientoCamara> ();
        }

        panelTxt.SetActive (false);
    }


    // Si pulsamos el botón de interacción con el input permitido y el texto disponible, activaremos la conversación con el NPC correspondiente (si aún no estábamos hablando), mostraremos el texto completo de la frase actual (si ya estamos 
    //hablando y todavía no se muestra entera) o pasaremos a la frase siguiente (si estamos hablando y el texto ya se muestra entero). Además, en el caso de que el panel esté activo y la frase aún no se muestre entera, el texto se mostrará 
    //progresivamente poco a poco y se rotará al personaje para que mire a su interlocutor.
    private void Update ()
    {
        tiempoPas += Time.deltaTime;
        if (textoScr == null || textoScr.hablando == false) 
        {
            textoScr = EncontrarConversacion ();
        }

        if (input == true && textoScr != null && Input.GetButtonDown ("Interacción") == true)
        {
            if (panelTxt.activeSelf == false)
            {
                if (this.IsInvoking ("ConversacionNPC") == false) 
                {
                    texto = textoScr.DevolverTexto ();
                    textoScr.hablando = true;

                    textoScr.ActivarIcono (false);
                    ControlarInput (false);
                    camaraScr.PuntoMedioDialogo (true, this.transform.position, capsulaNPC.bounds.center);
                    this.Invoke ("ConversacionNPC", 0.1f);
                }
            }
            else
            {
                if (letrasEsc < frase.Length)
                {
                    ParrafoCompleto ();
                }
                else
                {
                    ParrafoSiguiente ();
                }
            }
        }
        if (input == true && panelTxt.activeSelf == true)
        {
            if (tiempoPas > 0.03f && letrasEsc < frase.Length)
            {
                EscribirTexto ();

                tiempoPas = 0;
            }
            if (camaraScr != null && Quaternion.Angle (this.transform.rotation, rotacionObj) > 1) 
            {
                camaraScr.CalcularGiro (capsulaNPC.bounds.center);

                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionObj, Time.deltaTime * rotacionVel);
            }
        }
    }


    // El personaje añade a su lista el script de texto del NPC que tenga cerca.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Hablable") == true)
        {
            hablables.Add (other.GetComponent<Texto> ());
            //npc = other.GetComponent<CapsuleCollider> ();
        }
    }


    // El personaje elimina de su lista el script de texto del NPC del que se haya alejado.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Hablable") == true)
        {
            hablables.Remove (other.GetComponent<Texto> ());
            //npc = other.GetComponent<CapsuleCollider> ();
        }
    }


    // Función que activa el panel y separa el texto dado de manera correcta.
    public void IniciarDialogo ()
    {
        panelTxt.SetActive (true);
        SepararTexto ();
    }


    // Se actualiza el texto mostrado para que se vea completamente el párrafo actual.
    private void ParrafoCompleto ()
    {
        mostradoTxt[1].text = frase;
        letrasEsc = frase.Length;
    }


    // Si no se ha llegado aún al final de la conversación, se separa el texto del párrafo siguiente. En caso contrario, desactivamos el panel y el booleano que indica que el NPC está hablando. En el caso de que nos encontremos en la escena de los
    //narradores cuando esto pase, cargaremos la escena correspondiente; de lo contrario devolveremos el input al jugador y la cámapra pasara a seguir al mismo de nuevo.
    private void ParrafoSiguiente ()
    {
        parrafoAct += 1;
        letrasEsc = 0;

        if (parrafoAct != texto.Length)
        {
            SepararTexto ();
        }
        else
        {
            parrafoAct = 0;

            panelTxt.SetActive (false);
            if (escena0 == false)
            {
                ControlarInput (true);
                camaraScr.PuntoMedioDialogo (false, Vector3.zero, Vector3.zero);

                capsulaNPC.GetComponent<Texto>().FinalizarConversacion ();
                texto = null;
            }
            else
            {
                input = false;
                AlmacenDatos.instancia.muerte = false;
                Fundido.instancia.FundidoAEscena (AlmacenDatos.instancia.regresarA);
            }
        }
    }


    // Añadimos una nueva letra al texto mostrado.
    private void EscribirTexto ()
    {
        mostradoTxt[1].text += frase[letrasEsc];
        letrasEsc += 1;
    }


    // El texto se separa de manera que se asigna la primera parte del mismo al nombre del interlocutor mostrado en el panel y el resto a la frase que este diga.
    private void SepararTexto ()
    {
        bool comenzando = true;

        mostradoTxt[0].text = "";
        mostradoTxt[1].text = "";
        personaje = "";
        frase = "";
        //print (texto == null);
        foreach (char c in texto[parrafoAct])
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


    // Activamos o desactivamos el input en distintos scripts de acuerdo a la escena actual.
    private void ControlarInput (bool activar)
    {
        if (personajeMov2Scr != null)
        {
            personajeMov2Scr.input = activar;
            ataqueScr.input = activar;
            if (empujarScr != null)
            {
                empujarScr.input = activar;
            }
            cambioScr.input = activar;
        }
        else
        {
            if (personajeMov1Scr != null)
            {
                personajeMov1Scr.input = activar;
            }
            else
            {
                personajeMov3Scr.input = activar;
            }
        }
        camaraScr.input = activar;
    }


    // El jugador inicia una conversación con un NPC.
    private void ConversacionNPC ()
    {
        IniciarDialogo ();

        rotacionObj = Quaternion.Euler (0, Quaternion.LookRotation(capsulaNPC.bounds.center - this.transform.position).eulerAngles.y + 90, 0);
    }


    // De entre todos los NPCs cercanos, encuentra aquel cuyo vector diferencia con el avatar forme un menor ángulo con el vector hacia adelante del mismo. Este será con el que hablemos. 
    private Texto EncontrarConversacion () 
    {
        if (input == true && hablables.Count > 0) 
        {
            int mejorInd = 0;

            if (hablables.Count == 1) 
            {
                hablables[0].ActivarIcono (true);
            }
            else 
            {
                float anguloChc;

                float mejorAng = Vector3.Angle (new Vector3 (hablables[0].transform.position.x - this.transform.position.x, 0, hablables[0].transform.position.z - this.transform.position.z), -this.transform.right);

                hablables[0].ActivarIcono (false);

                for (int t = 1; t < hablables.Count; t += 1)
                {
                    anguloChc = Vector3.Angle (new Vector3 (hablables[t].transform.position.x - this.transform.position.x, 0, hablables[t].transform.position.z - this.transform.position.z), -this.transform.right);
                    if (anguloChc < mejorAng)
                    {
                        mejorInd = t;
                        mejorAng = anguloChc;
                    }

                    hablables[t].ActivarIcono (false);
                }
            }

            capsulaNPC = hablables[mejorInd].GetComponent<CapsuleCollider> ();

            hablables[mejorInd].ActivarIcono (true);

            return hablables[mejorInd];
        }

        for (int t = 0; t < hablables.Count; t += 1) 
        {
            hablables[0].ActivarIcono (false);
        }

        return null;
    }
}