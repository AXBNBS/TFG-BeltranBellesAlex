
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Agrupacion : MonoBehaviour
{
    private SphereCollider trigger;
    private CharacterController personajeCtrCpn;
    private MovimientoHistoria2 movimientoPad;


    // Inicialización de variables.
    private void Start ()
    {
        GameObject[] jugadoresObj;
        CharacterController personajeCtrPad;

        trigger = this.GetComponent<SphereCollider> ();
        jugadoresObj = GameObject.FindGameObjectsWithTag ("Jugador");
        personajeCtrCpn = jugadoresObj[this.transform.parent == jugadoresObj[0].transform ? 1 : 0].GetComponent<CharacterController> ();
        movimientoPad = this.transform.parent.GetComponent<MovimientoHistoria2> ();
        personajeCtrPad = this.transform.parent.GetComponent<CharacterController> ();
        trigger.center = personajeCtrPad.center;
        trigger.radius = this.transform.parent.localScale.x * personajeCtrPad.radius * 9;
    }


    // Si el compañero entra en el área o nosotros mismos nos acercamos a él, activamos el booleano que permite que nos siga con tan sólo pulsar un botón.
    private void OnTriggerEnter (Collider other)
    {
        if (other == personajeCtrCpn) 
        {
            movimientoPad.companyeroCer = true;
        }
    }


    // Si el compañero sale del área o nosotros mismos nos alejamos de él, desactivamos el booleano que permite que nos siga con tan sólo pulsar un botón.
    private void OnTriggerExit (Collider other)
    {
        if (other == personajeCtrCpn) 
        {
            movimientoPad.companyeroCer = false;
        }
    }
}