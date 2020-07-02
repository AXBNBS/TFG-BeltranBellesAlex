
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;



public class Morir : MonoBehaviour
{
    [SerializeField] private int muerteVelYMin;
    private CharacterController personajeCtr;


    // Asignación de una referencia al controlador del personaje.
    private void Start ()
    {
        personajeCtr = this.GetComponent<CharacterController> ();
    }


    // Si el personaje se está moviendo a una velocidad muy alta en sentido negativo del eje Y, morirá.
    private void Update ()
    {
        //print (personajeCtr.velocity.y);
        if (personajeCtr.velocity.y < muerteVelYMin) 
        {
            this.StartCoroutine (Muerte ("Caida"));
        }
    }


    // Si el avatar controlado acaba de ser derrotado, esperamos brevemente, paramos el tiempo y lo mandamos a la escena de muerte. Almacenaremos también datos como la escena a la que se regresará, el nombre del avatar y la causa de su muerte.
    private IEnumerator Muerte (string causa)
    {
        yield return new WaitForSeconds (0.2f);

        Time.timeScale = 0;
        AlmacenDatos.instancia.muerte = true;
        AlmacenDatos.instancia.regresarA = SceneManager.GetActiveScene().buildIndex;
        AlmacenDatos.instancia.avatarMue = this.name;
        AlmacenDatos.instancia.causaMue = causa;

        Fundido.instancia.FundidoAEscena (0);
    }
}