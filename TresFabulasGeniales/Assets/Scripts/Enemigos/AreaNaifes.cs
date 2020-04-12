
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent (typeof (SphereCollider))]
public class AreaNaifes : MonoBehaviour
{
    public float[] segundosCmbLim;
    public float radioGirRan, radioGirVar, nuevoPntOff;
    public LayerMask capasGir;
    public int distanciaMinObj;


    // .
    private void Start ()
    {
        this.GetComponent<SphereCollider>().isTrigger = true;
    }


    // .
    private void Update ()
    {
        
    }


    // Dibujamos el área circular que recorrerá uno de los naifes.
    private void OnDrawGizmosSelected ()
    {
        float aleatoriedad = Random.Range (-radioGirVar, +radioGirVar);

        Gizmos.color = Color.red;

        Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + this.transform.GetChild(0).forward * (radioGirRan + aleatoriedad));
        //Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + (Vector3.forward + Vector3.right) * (radioGirRan + aleatoriedad));
        Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + this.transform.GetChild(0).right * (radioGirRan + aleatoriedad));
        //Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + (Vector3.back + Vector3.right) * (radioGirRan + aleatoriedad));
        Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position - this.transform.GetChild(0).forward * (radioGirRan + aleatoriedad));
        //Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + (Vector3.back + Vector3.left) * (radioGirRan + aleatoriedad));
        Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position - this.transform.GetChild(0).right * (radioGirRan + aleatoriedad));
        //Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + (Vector3.forward + Vector3.left) * (radioGirRan + aleatoriedad));
    }
}