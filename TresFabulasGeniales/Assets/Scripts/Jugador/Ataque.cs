
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

    
    // Inicialización de variables.
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


    // Hacemos que el avatar rebote tras tocar la cabeza del enemigo y además le cause daño.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Rebote") == true && this.transform.position.y > other.transform.position.y) 
        {
            movimientoScr.movimiento.y = reboteVel;

            other.transform.parent.GetComponent<Enemigo>().Danyar (saltoFrz, true);
            characterCtr.Move (new Vector3 (0, reboteVel, 0) * Time.deltaTime);
        }
    }
}