
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class Fundido : MonoBehaviour
{
    public static Fundido instancia;

    private SeguimientoCamara camara;
    private Animator animador;
    private Vector3 posicion;
    private bool cambiarPos;
    private int escenaInd;


    // Inicialización de variables.
    private void Awake ()
    {
        if (instancia != null)
        {
            GameObject.Destroy (this.transform.parent.parent.gameObject);
        }
        else
        {
            instancia = this;

            GameObject.DontDestroyOnLoad (this.transform.parent.parent.gameObject);
        }
        animador = this.GetComponent<Animator> ();
    }


    // No entiendo como funciona pero de alguna manera se llama a "AlCargarEscena" cada vez que se carga una escena gracias a esto.
    private void OnEnable () 
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }


    // Hay que hacer esto también porque lo pone en la documentación de Unity y no sé, así va.
    private void OnDisable ()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }


    // Sirve para buscar el script de seguimiento de la cámara a cada nueva escena cargada.
    private void AlCargarEscena (Scene escena, LoadSceneMode modo)
    {
        camara = GameObject.FindObjectOfType<SeguimientoCamara> ();
    }


    // Activamos un fundido que permite mover la cámara a una posición distinta.
    public void FundidoAPosicion (Vector3 pos) 
    {
        animador.SetTrigger ("fundido");

        cambiarPos = true;
        posicion = pos;
    }


    // Activamos el fundido para cargar una escena nueva cuando la pantalla esté completamente en negro.
    public void FundidoAEscena (int escena) 
    {
        animador.SetTrigger ("fundido");

        cambiarPos = false;
        escenaInd = escena;
        AlmacenDatos.instancia.regresarA = SceneManager.GetActiveScene().buildIndex;
    }


    // Función que se llama al completar el fade out, se encarga de cambiar la cámara de posición o, si queremos cambiar de escena, cargar la nueva.
    private void EscenaOscura () 
    {
        if (cambiarPos == true)
        {
            camara.transform.position = posicion;
            camara.transicionando = false;
        }
        else 
        {
            Time.timeScale = 1;

            SceneManager.LoadScene (escenaInd);
        }
    }


    // Función que se llama al completar el fade in, se encarga de iniciar el diálgo entre los narradores si hemos vuelto a la escena inicial.
    private void EscenaVisible () 
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) 
        {
            GameObject.FindObjectOfType<Hablar>().IniciarDialogo ();
        }
    }
}