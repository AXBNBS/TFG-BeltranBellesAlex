
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Empujar : MonoBehaviour
{
    public bool input;

    private bool cercano, agarrado, ejeX;
    private LayerMask movilCap;
    private CharacterController characterCtr;
    private float longitudRay;
    private MovimientoHistoria2 movimientoScr;
    private ObjetoMovil empujado;
    private Animator animador;
    private Collider objetoMovTrg;
    private Vector3 rayoDir;


    // Inicialización de variables.
    private void Start ()
    {
        cercano = false;
        agarrado = false;
        movilCap = LayerMask.GetMask ("Movil");
        characterCtr = this.GetComponent<CharacterController> ();
        longitudRay = characterCtr.bounds.size.x;
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        animador = this.transform.GetChild(6).GetComponent<Animator> ();
    }


    // Si no hay un objeto agarrado, comprobamos si se cumplen las condiciones que lo permiten; en caso contrario, miramos si hemos soltado el botón de la interacción o el objeto empieza a caer para soltarlo. Animamos también al personaje en función 
    //de si está cogiendo un objeto o no.
    private void Update ()
    {
        if (agarrado == false)
        {
            EmpujePermitido ();
        }
        else
        {
            if (input == false || empujado.caer == true || Input.GetButtonUp ("Interacción") == true)
            {
                agarrado = false;

                movimientoScr.PararEmpuje ();
            }
        }
        Animar ();
    }


    // Nos aseguramos de sólo permitir que se empuje al estar cerca de un objeto con su respectivo trigger que lo active.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Empujable") == true)
        {
            cercano = true;
            objetoMovTrg = other;
            empujado = other.GetComponent<ObjetoMovil> ();
            ejeX = empujado.EjeDeTrigger (objetoMovTrg);
            print ("He entrado.");
        }
    }

    // Nos aseguramos de refrescar constantemente la dirección en la que se lanzará el rayo del empuje.
    private void OnTriggerStay (Collider other)
    {
        if (empujado != null && other.CompareTag ("Empujable") == true)
        {
            if (ejeX == false)
            {
                rayoDir = empujado.transform.position.z > this.transform.position.z ? Vector3.forward : Vector3.back;
            }
            else
            {
                rayoDir = empujado.transform.position.x > this.transform.position.x ? Vector3.right : Vector3.left;
            }
            print ("Aquí sigo.");
        }
    }


    // Al salir del trigger desactivamos el booleano que permite que cojamos el objeto.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Empujable") == true)
        {
            cercano = false;
            empujado = null;
            print ("He salido.");
        }
    }


    // Pal debug.
    private void OnDrawGizmos ()
    {
        if (characterCtr != null) 
        {
            Gizmos.color = Color.red;

            Gizmos.DrawRay (characterCtr.bounds.center, rayoDir * longitudRay);
        }
    }


    // Si el imput está permitido, estamos cerca del objeto, nuestro personaje está en el suelo, se está pulsando el botón de interacción y el objeto está delante del personaje a poca distancia, rotamos al personaje para que esté perfectamente 
    //alineado con él y a una distancia correcta para evitar que el personaje atraviese el objeto o se quede demasiado lejos del mismo, también ofrecemos los datos necesarios sobre el objeto al personaje para realizar el empuje.
    private void EmpujePermitido ()
    {
        /*print ("Sueleado: " + movimientoScr.sueleado);
        print ("Bloqueado: " + empujado.bloqueado);
        print ("Botón: " + Input.GetButton ("Interacción"));
        print ("Linecast: " + Physics.Linecast (characterCtr.bounds.center, characterCtr.bounds.center + rayoDir * longitudRay, movilCap, QueryTriggerInteraction.Ignore));*/
        if (input == true && cercano == true && movimientoScr.sueleado == true && empujado.caer == false && Input.GetButton ("Interacción") == true && Vector3.Angle (-this.transform.right, rayoDir) < 45 && 
            Physics.Linecast (characterCtr.bounds.center, characterCtr.bounds.center + rayoDir * longitudRay, out RaycastHit datosRay, movilCap, QueryTriggerInteraction.Ignore) == true)
        {
            //print ("Jugador: " + puntoIni + ". Punto del rayo: " + rayoDat.point + ". Diferencia: " + diferencia + ".");
            this.transform.rotation = Quaternion.Euler (0, Quaternion.LookRotation(datosRay.point - this.transform.position).eulerAngles.y + 90, 0);
            agarrado = true;

            if (CambioDePersonajesYAgrupacion.instancia.juntos == true) 
            {
                CambioDePersonajesYAgrupacion.instancia.Separar ();
            }
            movimientoScr.ComenzarEmpuje (ejeX, empujado);
        }
    }


    // Se anima al personaje en función de si está empujando un objeto o no.
    private void Animar ()
    {
        animador.SetBool ("cogiendo", agarrado);
    }
}