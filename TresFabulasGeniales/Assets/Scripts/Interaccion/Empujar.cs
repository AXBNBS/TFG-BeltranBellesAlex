
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Empujar : MonoBehaviour
{
    public bool input;

    private bool cercano, agarrado;
    private LayerMask movilCap;
    private CharacterController characterCtr;
    private float longitudRay, offsetY;
    private Quaternion nuevaRot;
    private MovimientoHistoria2 movimientoScr;
    private ObjetoMovil empujado;

    
    // Inicialización de variables.
    private void Start ()
    {
        cercano = false;
        agarrado = false;
        movilCap = LayerMask.GetMask ("Movil");
        characterCtr = this.GetComponent<CharacterController> ();
        longitudRay = characterCtr.radius * 3;
        offsetY = characterCtr.height / 2;
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
    }


    // Si no hay un objeto agarrado, comprobamos si se cumplen las condiciones que lo permiten; en caso contrario, miramos si hemos soltado el botón de la interacción o el objeto empieza a caer para soltarlo.
    private void Update ()
    {
        if (agarrado == false)
        {
            EmpujePermitido ();
        }
        else 
        {
            if (empujado.caer == true || Input.GetButtonUp ("Interacción") == true) 
            {
                movimientoScr.PararEmpuje ();

                agarrado = false;
            }
        }
    }


    // Nos aseguramos de sólo permitir que se empuje al estar cerca de un objeto con su respectivo trigger que lo active.
    private void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Empujable") 
        {
            cercano = true;
        }
    }


    // Al salir del trigger desactivamos el booleano que permite que cojamos el objeto.
    private void OnTriggerExit (Collider other)
    {
        if (other.tag == "Empujable")
        {
            cercano = false;
        }
    }


    // Pal debug.
    /*private void OnDrawGizmos ()
    {
        Gizmos.DrawLine (puntoIni, puntoIni + this.transform.forward * longitudRay);
    }*/


    // Si el imput está permitido, estamos cerca del objeto, nuestro personaje está en el suelo, se está pulsando el botón de interacción y el objeto está delante del personaje a poca distancia, rotamos al personaje para que esté perfectamente alineado
    //con él y a una distancia correcta para evitar que el personaje atraviese el objeto o se quede demasiado lejos del mismo, también ofrecemos los datos necesarios sobre el objeto al personaje para realizar el empuje.
    private void EmpujePermitido () 
    {
        Vector3 puntoIni = new Vector3 (this.transform.position.x, this.transform.position.y + offsetY, this.transform.position.z);

        RaycastHit rayoDat;
        if (input == true && cercano == true && characterCtr.isGrounded == true && Input.GetButton ("Interacción") == true && Physics.Raycast (puntoIni, this.transform.forward, out rayoDat, longitudRay, movilCap, QueryTriggerInteraction.Ignore) == true)
        {
            Vector3 diferencia;
            float angulo;

            empujado = rayoDat.transform.gameObject.GetComponent<ObjetoMovil> ();
            diferencia = rayoDat.point - puntoIni;
            diferencia.y = 0;
            angulo = Vector3.Angle (this.transform.forward, diferencia);
            nuevaRot = Quaternion.Euler (0, angulo, 0);
            agarrado = true;
            this.transform.rotation = nuevaRot;

            bool ejeX = empujado.EjeDeTrigger (rayoDat.collider);

            if (ejeX == true)
            {
                this.transform.position = this.transform.position.x > rayoDat.point.x ? new Vector3 (this.transform.position.x + characterCtr.radius, this.transform.position.y, this.transform.position.z) :
                    new Vector3 (this.transform.position.x - characterCtr.radius, this.transform.position.y, this.transform.position.z);
            }
            else 
            {
                this.transform.position = this.transform.position.z > rayoDat.point.z ? new Vector3 (this.transform.position.x, this.transform.position.y, this.transform.position.z + characterCtr.radius) :
                    new Vector3 (this.transform.position.x, this.transform.position.y, this.transform.position.z - characterCtr.radius);
            }

            CambioDePersonajesYAgrupacion.instancia.Separar ();
            movimientoScr.ComenzarEmpuje (ejeX, empujado);
        }
    }
}