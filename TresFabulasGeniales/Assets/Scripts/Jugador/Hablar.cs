
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
    private Transform npc;
    private SeguimientoCamara camara;
    private MovimientoHistoria1 personajeMov1;
    private MovimientoHistoria2 personajeMov2;
    private MovimientoHistoria3 personajeMov3;
    private Quaternion rotacionObj;

    
    // .
    private void Start ()
    {
        texto = null;
        panelTxt = GameObject.FindGameObjectWithTag ("PanelTexto");
        mostradoTxt = panelTxt.GetComponentsInChildren<Text> ();
        tiempoPas = 0;
        parrafoAct = 0;
        letrasEsc = 0;
        camara = GameObject.FindObjectOfType<SeguimientoCamara> ();
        personajeMov1 = this.GetComponent<MovimientoHistoria1> ();
        personajeMov2 = this.GetComponent<MovimientoHistoria2> ();
        personajeMov3 = this.GetComponent<MovimientoHistoria3> ();

        panelTxt.SetActive (false);
    }


    // .
    private void Update ()
    {
        tiempoPas += Time.deltaTime;
        //this.transform.rotation = Quaternion.Euler (0, 180, 0);

        if (input == true && texto != null && Input.GetButtonDown ("Interacción") == true) 
        {
            if (panelTxt.activeSelf == false)
            {
                panelTxt.SetActive (true);
                SepararTexto ();
                ControlarInput (false);

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
            ControlarInput (true);
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
        string escenaNmb = SceneManager.GetActiveScene().name;

        if (escenaNmb.Contains ("Eva") == true)
        {
            personajeMov1.input = activar;
        }
        else 
        {
            if (escenaNmb.Contains ("Violeta") == true) 
            {
                personajeMov2.input = activar;
            }
            else 
            {
                personajeMov3.input = activar;
            }
        }
        camara.input = activar;
    }
}