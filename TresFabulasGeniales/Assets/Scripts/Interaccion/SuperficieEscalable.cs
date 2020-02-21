
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SuperficieEscalable : MonoBehaviour
{
    [SerializeField] private bool movimientoX;
    [SerializeField] private Collider trigger;
    private MovimientoHistoria3 jugador;
    public float limite1, limite2;


    // Inicializamos una unidad de variable.
    private void Start ()
    {
        jugador = GameObject.FindObjectOfType<MovimientoHistoria3> ();
        if (movimientoX == true)
        {
            limite1 = trigger.bounds.center.x + trigger.bounds.extents.x;
            limite2 = trigger.bounds.center.x - trigger.bounds.extents.x;
        }
        else 
        {
            limite1 = trigger.bounds.center.z + trigger.bounds.extents.z;
            limite2 = trigger.bounds.center.z - trigger.bounds.extents.z;
        }
        
    }


    // Si el jugador toca la superficie, le permitimos trepar por ella y le indicamos si el movimiento es posible en X o Z.
    private void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Jugador") 
        {
            jugador.escalarPos = true;
            jugador.movimientoXEsc = movimientoX;
            jugador.limiteEsc1 = limite1;
            jugador.limiteEsc2 = limite2;
        }
    }


    // No aseguramos de que el jugador no pueda seguir escalando al salir del área cubierta por el trigger.
    private void OnTriggerExit (Collider other)
    {
        if (other.tag == "Jugador")
        {
            jugador.escalarPos = false;
        }
    }
}