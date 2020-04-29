
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent (typeof (MeshCollider))]
public class SuperficieEscalable : MonoBehaviour
{
    [SerializeField] private bool movimientoX;
    private MeshCollider trigger;
    private MovimientoHistoria3 jugador;
    private float limite1, limite2;
    private Quaternion[] rotacionesEsc;


    // Inicializamos una unidad de variable.
    private void Start ()
    {
        jugador = GameObject.FindObjectOfType<MovimientoHistoria3> ();
        trigger = this.GetComponent<MeshCollider> ();
        trigger.convex = true;
        trigger.isTrigger = true;
        rotacionesEsc = new Quaternion[] { Quaternion.Euler (0, 0, 0), Quaternion.Euler (0, 90, 0), Quaternion.Euler (0, 180, 0), Quaternion.Euler (0, 270, 0) };
        this.gameObject.layer = LayerMask.NameToLayer ("SuperficieAdherible");
    }


    // Si el jugador toca la superficie, le permitimos trepar por ella y le indicamos si el movimiento es posible en X o Z.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            print (trigger.bounds.max.x);
            jugador.movimientoXEsc = movimientoX;
            if (movimientoX == true)
            {
                jugador.rotacionEsc = this.transform.position.z > jugador.transform.position.z ? rotacionesEsc[1] : rotacionesEsc[3];
            }
            else 
            {
                jugador.rotacionEsc = this.transform.position.x < jugador.transform.position.x ? rotacionesEsc[0] : rotacionesEsc[2];
            }
        }
    }


    // .
    private void OnTriggerStay (Collider other)
    {
        if (other.CompareTag ("Jugador") == true)
        {
            jugador.escalarPos = true;
        }
    }


    // No aseguramos de que el jugador no pueda seguir escalando al salir del área cubierta por el trigger.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true)
        {
            print ("Debería no escalar xD");
            jugador.escalarPos = false;
        }
    }
}