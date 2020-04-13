
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent (typeof (SphereCollider))]
public class AreaNaifes : MonoBehaviour
{
    public float[] segundosCmbLim;
    public float radioGirRan, radioGirVar, nuevoPntOff, rotacionVel;
    public LayerMask capasGir;
    public int distanciaMinObj;

    private SphereCollider trigger;


    // .
    private void Start ()
    {
        trigger = this.GetComponent<SphereCollider> ();
        trigger.isTrigger = true;
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


    // .
    public Vector3 PuntoAleatorioDentro (Vector3 posicionIni, Vector3 cajaDim) 
    {
        Vector3 centro = trigger.bounds.Contains (posicionIni) == true ? posicionIni : new Vector3 (Random.Range (trigger.bounds.min.x, trigger.bounds.max.x), posicionIni.y, Random.Range (trigger.bounds.min.z, trigger.bounds.max.z));

        while (Physics.CheckBox (centro, cajaDim, Quaternion.identity, capasGir, QueryTriggerInteraction.Ignore) == true) 
        {
            centro.x = Random.Range (trigger.bounds.min.x, trigger.bounds.max.x);
            centro.z = Random.Range (trigger.bounds.min.z, trigger.bounds.max.z);
        }

        return centro;
    }
}