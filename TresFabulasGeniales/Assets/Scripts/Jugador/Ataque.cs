
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Ataque : MonoBehaviour
{
    public bool input;

    [SerializeField] private float saltoFrz, aranyazoFrz;
    private MovimientoHistoria2 movimientoScr;
    private CharacterController characterCtr;
    private float reboteVel;
    private NavMeshAgent agente;
    public bool danyado;

    
    // Inicialización de variables.
    private void Start ()
    {
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        characterCtr = this.GetComponent<CharacterController> ();
        reboteVel = +movimientoScr.saltoVel;
        agente = this.GetComponent<NavMeshAgent> ();
        danyado = false;
    }


    // Hacemos que el avatar rebote tras tocar la cabeza del enemigo y además le cause daño.
    private void OnTriggerStay (Collider other)
    {
        if (danyado == false && movimientoScr.sueleado == false && other.CompareTag ("Rebote") == true) 
        {
            danyado = true;
            movimientoScr.movimiento.y = reboteVel;

            if (movimientoScr.input == true)
            {
                other.transform.parent.GetComponent<Enemigo>().Danyar (saltoFrz, true);
                characterCtr.Move (new Vector3 (0, reboteVel, 0) * Time.deltaTime);
            }
            else
            {
                other.transform.parent.GetComponent<Enemigo>().Danyar (saltoFrz, true);

                agente.baseOffset += reboteVel * Time.deltaTime;
            }
        }
    }


    // .
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Rebote") == true) 
        {
            danyado = false;

            if (other.transform.parent.GetComponent<Enemigo>().ChecarDerrotado () == true) 
            {
                movimientoScr.PosicionEnemigoCercano ();
            }
        }
    }
}