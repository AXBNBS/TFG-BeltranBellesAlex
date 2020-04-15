
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;



[RequireComponent (typeof (SphereCollider))]
public class AreaNaifes : MonoBehaviour
{
    public float[] segundosCmbLim;
    public float radioGirRan, radioGirVar, giroVel;
    public LayerMask capasGirAtq;
    public int velocidadRotNor, velocidadRotGir, distanciaMinObj, distanciaParIgn, frenadoVel, pararVel;
    [HideInInspector] public Quaternion[] modeloRotLoc;

    private SphereCollider trigger;
    private Naife[] naifes;


    // Inicialización de variables.
    private void Awake ()
    {
        modeloRotLoc = new Quaternion[] { Quaternion.Euler (0, 90, -20), Quaternion.Euler (0, 90, 5) };
        trigger = this.GetComponent<SphereCollider> ();
        trigger.isTrigger = true;
        naifes = this.GetComponentsInChildren<Naife> ();
    }


    // .
    private void Update ()
    {
        
    }


    // Dibujamos el área circular que recorrerá uno de los naifes.
    /*private void OnDrawGizmosSelected ()
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
    }*/


    // El entrar el jugador en la zona, encontramos al primer enemigo sin blanco asignado y hacemos que este vaya a atacarle.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true)
        {
            PrimeroSinBlanco().IniciarAtaque (other.transform);
        }
    }


    // Encuentra una posción adecuada para el que servirá como pivote de la rotación del naife, y devuelve también el extremo del círculo que formará el mismo hacia el cuál el enemigo ha de dirigirse.
    public Vector3 PosicionPivoteYDestino (Transform pivote, Transform naife, Vector3 cajaDim, float radio) 
    {
        Vector3 diferencia;
        float anguloChc, diferenciaChc;

        Vector3[] extremos = new Vector3[4];
        float mejorDif = 90;
        int mejorInd = 0;
        Vector3 centro = new Vector3 (naife.position.x, pivote.position.y, naife.position.z);
        Collider naifeCol = naife.GetComponent<Collider> ();
        List<Collider> obstaculos = Physics.OverlapBox(centro, cajaDim, Quaternion.identity, capasGirAtq, QueryTriggerInteraction.Ignore).ToList<Collider> ();

        obstaculos.Remove (naifeCol);

        print ("Antes del while.");
        while (obstaculos.Count != 0 || (Vector2.Distance (new Vector2 (trigger.bounds.center.x, trigger.bounds.center.z), new Vector2 (centro.x, centro.z)) + radio) > trigger.radius) 
        {
            foreach (Collider c in obstaculos) 
            {
                print (c.name);
            }
            print ("Antes del random.");
            centro = Random.insideUnitSphere * trigger.radius + trigger.bounds.center;
            centro.y = pivote.position.y;
            print ("Después del random.");
            obstaculos = Physics.OverlapBox(centro, cajaDim, Quaternion.identity, capasGirAtq, QueryTriggerInteraction.Ignore).ToList<Collider> ();

            obstaculos.Remove (naifeCol);
        }
        print ("Después del while.");

        pivote.position = centro;
        extremos[0] = Vector3.forward * radio + centro;
        extremos[1] = Vector3.back * radio + centro;
        extremos[2] = Vector3.right * radio + centro;
        extremos[3] = Vector3.left * radio + centro;
        diferencia = naife.position - centro;
        for (int p = 0; p < extremos.Length; p += 1)
        {
            anguloChc = Vector3.Angle (extremos[p], diferencia);
            diferenciaChc = Mathf.Abs (anguloChc - 90);
            if (diferenciaChc < mejorDif)
            {
                mejorDif = diferenciaChc;
                mejorInd = p;
            }
        }

        return extremos[mejorInd];
    }


    // Función que devuelve el primero de los naifes de la zona que no esté atacando a nadie.
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