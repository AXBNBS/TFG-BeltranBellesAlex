
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Ataque : MonoBehaviour
{
    public bool input;

    [SerializeField] private int saltoFrz, aranyazoFrz;
    private MovimientoHistoria2 movimientoScr;
    private CharacterController characterCtr;

    
    // .
    private void Start ()
    {
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        characterCtr = this.GetComponent<CharacterController> ();
    }


    // .
    private void Update ()
    {
        
    }


    // .
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        Transform tocado = hit.transform;

        if (tocado.tag == "Enemigo" && movimientoScr.movimiento.y < -50 && this.transform.position.y > tocado.position.y) 
        {
            tocado.GetComponent<Enemigo>().Danyar (saltoFrz, true);

            //movimientoScr.movimiento.y /= 2;
            movimientoScr.Saltar ();
        }
    }
}