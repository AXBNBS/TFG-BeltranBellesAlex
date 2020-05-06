
using System.Collections.Generic;
using UnityEngine;



public class PlataformaMovil : MonoBehaviour
{
    // Para añadir los 2 colliders automáticamente cuando añadamos este script a un objeto.
    private void Reset ()
    {
        this.gameObject.AddComponent<BoxCollider> ();
        this.gameObject.AddComponent<BoxCollider> ();
    }


    // Si un avatar está sobre la plataforma, obtenemos la referencia a su script de movimiento.
    private void OnTriggerEnter (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true)
        {
            other.transform.parent = this.transform.parent;
            other.transform.localScale = Vector3.one;
            other.GetComponent<MovimientoHistoria2>().plataformaAbj = true;
            //print ("Gato Sánchez.");
        }
    }


    // Al salir del trigger de la plataforma, nos aseguramos de que no se aplique movimiento alguno por parte de la misma al avatar.
    private void OnTriggerExit (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true)
        {
            other.transform.parent = null;
            other.transform.localScale = Vector3.one;
            other.GetComponent<MovimientoHistoria2>().plataformaAbj = false;
            //print ("Perro Sánchez.");
        }
    }
}