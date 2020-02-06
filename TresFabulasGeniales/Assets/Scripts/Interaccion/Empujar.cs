
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Empujar : MonoBehaviour
{
    public bool input;

    private bool cercano, agarrado;
    private LayerMask movilCap;
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
        longitudRay = this.GetComponent<CharacterController>().radius * 3;
        offsetY = this.GetComponent<CharacterController>().height / 2;
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
    }


    // Si permitimos input, el objeto está cerca, se está pulsando el botón de interacción y detectamos el objeto lanzando un rayo un poco más adelante, pasamos a empujarlo.
    private void Update ()
    {
        if (agarrado == false)
        {
            EmpujePermitido ();
        }
        else 
        {
            if (Input.GetButtonUp ("Interacción") == true || empujado.caer == true) 
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
    private void OnDrawGizmos ()
    {
        //Gizmos.DrawLine (puntoIni, puntoIni + this.transform.forward * longitudRay);
    }


    // .
    private void EmpujePermitido () 
    {
        Vector3 diferencia;
        float angulo;
        RaycastHit rayoDat;

        Vector3 puntoIni = new Vector3 (this.transform.position.x, this.transform.position.y + offsetY, this.transform.position.z);

        if (input == true && cercano == true && Input.GetButton ("Interacción") == true && Physics.Raycast (puntoIni, this.transform.forward, out rayoDat, longitudRay, movilCap, QueryTriggerInteraction.Ignore) == true)
        {
            empujado = rayoDat.transform.gameObject.GetComponent<ObjetoMovil> ();

            diferencia = rayoDat.point - puntoIni;
            diferencia.y = 0;
            angulo = Vector3.Angle (this.transform.forward, diferencia);
            nuevaRot = Quaternion.Euler (0, angulo, 0);
            agarrado = true;
            this.transform.rotation = nuevaRot;

            movimientoScr.ComenzarEmpuje (empujado.EjeDeTrigger (rayoDat.collider), empujado);
        }
    }
}