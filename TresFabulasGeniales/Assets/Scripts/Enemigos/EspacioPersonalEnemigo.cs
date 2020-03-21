
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EspacioPersonalEnemigo : MonoBehaviour
{
    public List<Collider> companyerosCer;

    private Enemigo enemigoScr;


    // .
    private void Start ()
    {
        companyerosCer = new List<Collider> ();
        enemigoScr = this.transform.parent.GetComponent<Enemigo> ();
    }


    // .
    private void Update ()
    {
        
    }


    // .
    private void OnTriggerEnter (Collider other)
    {
        if (enemigoScr.acercarse == true && other.CompareTag ("Enemigo") == true && other.GetComponent<Enemigo>().prioridad > enemigoScr.prioridad) 
        {
            companyerosCer.Add (other);
        }
    }


    // .
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Enemigo") == true)
        {
            this.StartCoroutine ("EliminarElemento", other);
        }
    }


    // .
    private IEnumerator EliminarElemento (Collider eliminar) 
    {
        yield return new WaitForSeconds (1);

        companyerosCer.Remove (eliminar);
    }
}