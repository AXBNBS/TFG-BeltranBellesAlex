
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Ataque : MonoBehaviour
{
    public bool input;

    [SerializeField] private int saltoFrz, aranyazoFrz;
    private MovimientoHistoria2 movimientoScr;
    private CharacterController characterCtr;
    private float reboteVel;

    
    // .
    private void Start ()
    {
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        characterCtr = this.GetComponent<CharacterController> ();
        reboteVel = +movimientoScr.saltoVel;
    }


    // .
    private void Update ()
    {

    }


    // .
    /*private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        Transform tocado = hit.transform;

        if (characterCtr.isGrounded == false && tocado.tag == "Enemigo" && movimientoScr.movimiento.y < -50 && this.transform.position.y > tocado.position.y) 
        {
            tocado.GetComponent<Enemigo>().Danyar (saltoFrz, true);
            movimientoScr.Saltar ();
            print ("llamado");
        }
    }*/


    // .
    private void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Rebote" && this.transform.position.y > other.transform.position.y) 
        {
            movimientoScr.movimiento.y = reboteVel;

            other.transform.parent.GetComponent<Enemigo>().Danyar (saltoFrz, true);
            characterCtr.Move (new Vector3 (0, reboteVel, 0) * Time.deltaTime);
        }
    }
}