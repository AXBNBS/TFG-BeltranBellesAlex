
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent (typeof (SphereCollider))]
public class AreaNaifes : MonoBehaviour
{
    public float[] segundosCmbLim;
    public float radioGirRan, radioGirVar, giroVel, rotacionVel;
    public LayerMask capasGirAtq;
    public int distanciaMinObj, distanciaParIgn, frenadoVel, pararVel;

    private SphereCollider trigger;
    private Naife[] naifes;


    // Inicialización de variables.
    private void Awake ()
    {
        trigger = this.GetComponent<SphereCollider> ();
        trigger.isTrigger = true;
        naifes = this.GetComponentsInChildren<Naife> ();
    }


    // .
    private void Update ()
    {
        
    }


    // Dibujamos el área circular que recorrerá uno de los naifes.
    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + Vector3.forward * radioGirRan);
        //Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + (Vector3.forward + Vector3.right) * (radioGirRan + aleatoriedad));
        Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + Vector3.right * radioGirRan);
        //Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + (Vector3.back + Vector3.right) * (radioGirRan + aleatoriedad));
        Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position - Vector3.forward * radioGirRan);
        //Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + (Vector3.back + Vector3.left) * (radioGirRan + aleatoriedad));
        Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position - Vector3.right * radioGirRan);
        //Gizmos.DrawLine (this.transform.GetChild(0).position, this.transform.GetChild(0).position + (Vector3.forward + Vector3.left) * (radioGirRan + aleatoriedad));
    }


    // .
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true)
        {
            PrimeroSinBlanco().IniciarAtaque (other.transform);
        }
    }


    // Recibe una posición inicial y el área en la cuál ha de comprobarse que la zona esté libre. Si la posición recibida inicialmente no tiene obstáculos alrededor, este pasará a ser el nuevo centro de la rotación del naife; en caso contrario,
    //iremos obteniendo posiciones aleatorias dentro del área para convertir estas en el centro sobre el cuál girará el enemigo.
    public Vector3 PuntoAleatorioDentro (Vector3 posicionIni, Vector3 cajaDim) 
    {
        Vector3 centro = trigger.bounds.Contains (posicionIni) == true ? posicionIni : new Vector3 (Random.Range (trigger.bounds.min.x, trigger.bounds.max.x), posicionIni.y, Random.Range (trigger.bounds.min.z, trigger.bounds.max.z));

        while (Physics.CheckBox (centro, cajaDim, Quaternion.identity, capasGirAtq, QueryTriggerInteraction.Ignore) == true) 
        {
            centro.x = Random.Range (trigger.bounds.min.x, trigger.bounds.max.x);
            centro.z = Random.Range (trigger.bounds.min.z, trigger.bounds.max.z);
        }

        return centro;
    }


    private Naife PrimeroSinBlanco () 
    {
        foreach (Naife n in naifes) 
        {
            if (Naife.Estado.normal == n.estado) 
            {
                return n;
            }
        }
        return null;
    }
}