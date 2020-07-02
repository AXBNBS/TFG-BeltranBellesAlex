
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent (typeof (BoxCollider))]
public class SuperficieEscalable : MonoBehaviour
{
    [SerializeField] private bool movimientoX, max;
    private float twiiPosIdeXZ;
    private float[] limites;
    private Quaternion[] rotacionesEsc;
    private BoxCollider trigger;
    private MovimientoHistoria3 jugadorMovScr;


    // Inicializamos una unidad de variable.
    private void Start ()
    {
        CharacterController jugadorChrCtr = GameObject.FindGameObjectWithTag("Jugador").GetComponent<CharacterController> ();

        rotacionesEsc = new Quaternion[] { Quaternion.Euler (0, 0, 0), Quaternion.Euler (0, 90, 0), Quaternion.Euler (0, 180, 0), Quaternion.Euler (0, 270, 0) };
        trigger = this.GetComponent<BoxCollider> ();
        trigger.isTrigger = true;
        if (movimientoX == true) 
        {
            twiiPosIdeXZ = (max == true ? trigger.bounds.max.z : trigger.bounds.min.z) + jugadorChrCtr.bounds.extents.z;
        }
        else 
        {
            twiiPosIdeXZ = (max == true ? trigger.bounds.max.x : trigger.bounds.min.x) + jugadorChrCtr.bounds.extents.x;
        }
        jugadorMovScr = jugadorChrCtr.GetComponent<MovimientoHistoria3> ();
        limites = new float[4];
        if (movimientoX == true) 
        {
            limites[0] = trigger.bounds.max.x - jugadorChrCtr.bounds.extents.x;
            limites[1] = trigger.bounds.min.x + jugadorChrCtr.bounds.extents.x;
        }
        else 
        {
            limites[0] = trigger.bounds.max.z - jugadorChrCtr.bounds.extents.z;
            limites[1] = trigger.bounds.min.z + jugadorChrCtr.bounds.extents.z;
        }
        limites[2] = trigger.bounds.max.y - jugadorChrCtr.bounds.extents.y;
        limites[3] = trigger.bounds.min.y + jugadorChrCtr.bounds.extents.y;
        this.gameObject.layer = LayerMask.NameToLayer ("SuperficieAdherible");
    }


    // Si el jugador toca la superficie, le permitimos trepar por ella y le indicamos si el movimiento es posible en X o Z.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            jugadorMovScr.movimientoXEsc = movimientoX;
            jugadorMovScr.escaladaXZ = twiiPosIdeXZ;
            jugadorMovScr.limitesEsc = limites;
            if (movimientoX == true)
            {
                jugadorMovScr.rotacionEsc = this.transform.position.z > jugadorMovScr.transform.position.z ? rotacionesEsc[1] : rotacionesEsc[3];
            }
            else 
            {
                jugadorMovScr.rotacionEsc = this.transform.position.x < jugadorMovScr.transform.position.x ? rotacionesEsc[0] : rotacionesEsc[2];
            }
        }
    }


    // .
    private void OnTriggerStay (Collider other)
    {
        if (other.CompareTag ("Jugador") == true)
        {
            jugadorMovScr.escalarPos = true;
        }
    }


    // No aseguramos de que el jugador no pueda seguir escalando al salir del área cubierta por el trigger.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true)
        {
            //print ("Debería no escalar xD");
            jugadorMovScr.escalarPos = false;
        }
    }
}