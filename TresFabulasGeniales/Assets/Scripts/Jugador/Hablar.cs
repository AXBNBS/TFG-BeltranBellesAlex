
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class Hablar : MonoBehaviour
{
    public bool input;
    public string[] texto;

    [SerializeField] private int rotacionVel;
    private GameObject panelTxt;
    private Text[] mostradoTxt;
    private float tiempoPas;
    private int parrafoAct, letrasEsc;
    private string personaje, frase;
    private bool escena0;
    private Transform npc;
    private MovimientoHistoria1 personajeMov1Scr;
    private MovimientoHistoria2 personajeMov2Scr;
    private Ataque ataqueScr;
    private Empujar empujarScr;
    private CambioDePersonajesYAgrupacion cambioScr;
    private MovimientoHistoria3 personajeMov3Scr;
    private SeguimientoCamara camaraScr;
    private Quaternion rotacionObj;

    
    // .
    private void Start ()
    {
        panelTxt = GameObject.FindGameObjectWithTag("Interfaz").transform.GetChild(0).GetChild(0).gameObject;
        mostradoTxt = panelTxt.GetComponentsInChildren<Text> ();
        tiempoPas = 0;
        parrafoAct = 0;
        letrasEsc = 0;
        escena0 = SceneManager.GetActiveScene().name == "CasaBeltranuki";
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


    // .
    private void Update ()
    {
        tiempoPas += Time.deltaTime;

        if (input == true && texto != null && Input.GetButtonDown ("Interacción") == true) 
        {
            if (panelTxt.activeSelf == false)
            {
                IniciarDialogo ();
                ControlarInput (false);
                camaraScr.PuntoMedioDialogo (true, this.transform.position, npc.GetComponent<CapsuleCollider>().bounds.center);

                rotacionObj = Quaternion.Euler (0, Quaternion.LookRotation(npc.position - this.transform.position).eulerAngles.y, 0);
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
        if (panelTxt.activeSelf == true) 
        {
            if (tiempoPas > 0.03f && letrasEsc < frase.Length) 
            {
                EscribirTexto ();

                tiempoPas = 0;
            }
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionObj, Time.deltaTime * rotacionVel);
            if (camaraScr != null && Quaternion.Angle (this.transform.rotation, rotacionObj) < 1) 
            {
                camaraScr.CalcularGiro ();
            }
        }
    }


    // .
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Hablable") == true) 
        {
            npc = other.transform;
        }
    }


    // .
    public void IniciarDialogo ()
    {
        panelTxt.SetActive (true);
        SepararTexto ();
    }


    // .
    private void ParrafoCompleto () 
    {
        mostradoTxt[1].text = frase;
        letrasEsc = frase.Length;
    }


    // .
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
            }
            else 
            {
                Fundido.instancia.FundidoAEscena (AlmacenDatos.instancia.regresarA);
            }
        }
    }


    // .
    private void EscribirTexto ()
    {
        mostradoTxt[1].text += frase[letrasEsc];
        letrasEsc += 1;
    }


    // .
    private void SepararTexto () 
    {
        bool comenzando = true;

        mostradoTxt[0].text = "";
        mostradoTxt[1].text = "";
        personaje = "";
        frase = "";
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


    // .
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
}