
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EspaldaEnemigo : MonoBehaviour
{
    public List<Collider> obstaculos;

    
    // Inicialización de variables.
    private void Start ()
    {
        obstaculos = new List<Collider> ();
    }


    // Metemos al obstáculo en la lista si toca el trigger.
    private void OnTriggerEnter (Collider other)
    {
        if (other.isTrigger == false) 
        {
            obstaculos.Add (other);
        }
    }


    // Sacamos al obstáculo de la lista si ha salido del trigger.
    private void OnTriggerExit (Collider other)
    {
        if (other.isTrigger == false) 
        {
            obstaculos.Remove (other);
        }
    }
}