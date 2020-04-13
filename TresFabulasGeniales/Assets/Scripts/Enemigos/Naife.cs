
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Naife : MonoBehaviour
{
    [SerializeField] private bool quieto, sentidoHor;
    private AreaNaifes padreScr;
    private Vector3[] extremosCir;
    private CapsuleCollider capsula;
    private float centroY, radio, destinoDst;
    private NavMeshAgent agente;
    private int indicePnt;
    private Transform padreRot;
    private Vector3 destino;
    

    // .
    private void Start ()
    {
        quieto = true;
        padreScr = this.transform.parent.GetComponent<AreaNaifes> ();
        extremosCir = new Vector3[4];
        capsula = this.GetComponent<CapsuleCollider> ();
        centroY = capsula.bounds.center.y - capsula.bounds.extents.y * 0.8f;
        agente = this.GetComponent<NavMeshAgent> ();
        padreRot = GameObject.Instantiate(new GameObject(), new Vector3 (this.transform.position.x, centroY, this.transform.position.z), Quaternion.identity).transform;
        padreRot.parent = padreScr.transform;

        this.InvokeRepeating ("QuietoOGirando", 0, UnityEngine.Random.Range (padreScr.segundosCmbLim[0], padreScr.segundosCmbLim[1]));
    }


    // .
    private void Update ()
    {
        if (quieto == false)
        {
            if (agente.isStopped == false) 
            {
                if (Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (destino.x, destino.z)) < padreScr.distanciaMinObj) 
                {
                    agente.isStopped = true;
                    this.transform.parent = padreRot;
                }
            }
            else 
            {
                padreRot.Rotate (new Vector3 (0, sentidoHor == false ? +padreScr.rotacionVel : -padreScr.rotacionVel, 0) * Time.deltaTime);
            }
        }

        //agente.velocity = agente.desiredVelocity;
    }


    // .
    private void OnDrawGizmosSelected ()
    {
        if (padreScr != null) 
        {
            //Gizmos.DrawWireSphere (centro, 5);
            //Gizmos.DrawWireSphere (ruta[0], 5);
            //Gizmos.DrawWireSphere (ruta[1], 5);
            //Gizmos.DrawWireSphere (ruta[2], 5);
            //Gizmos.DrawWireSphere (ruta[3], 5);
            //Gizmos.DrawWireCube (this.transform.position, new Vector3 (padreScr.radioGirRan + capsula.bounds.size.x, 0.5f, padreScr.radioGirRan + capsula.bounds.size.x));
        }
    }


    // .
    private void QuietoOGirando () 
    {
        quieto = !quieto;
        if (quieto == false) 
        {
            float anguloChc, diferenciaChc;
            Vector3 diferencia;

            float aleatoriedad = UnityEngine.Random.Range (-padreScr.radioGirVar, +padreScr.radioGirVar);
            float dimensionesXZ = padreScr.radioGirRan + capsula.bounds.size.z + aleatoriedad;
            float mejorDif = 90;
            int mejorInd = 0;

            padreRot.position = padreScr.PuntoAleatorioDentro (new Vector3 (this.transform.position.x, centroY, this.transform.position.z), new Vector3 (dimensionesXZ, 0.5f, dimensionesXZ));
            /*while (Physics.CheckBox (padreRot.position, new Vector3 (dimensionesXZ, 0.5f, dimensionesXZ), Quaternion.identity, padreScr.capasGir, QueryTriggerInteraction.Ignore) == true) 
            {
                padreRot.position = new Vector3 (padreRot.position.x + UnityEngine.Random.Range (-padreScr.nuevoPntOff, +padreScr.nuevoPntOff), centroY, padreRot.position.z + UnityEngine.Random.Range (-padreScr.nuevoPntOff, +padreScr.nuevoPntOff));
            }*/
            radio = padreScr.radioGirRan + aleatoriedad;
            extremosCir[0] = Vector3.forward * radio + padreRot.position;
            extremosCir[1] = Vector3.back * radio + padreRot.position;
            extremosCir[2] = Vector3.right * radio + padreRot.position;
            extremosCir[3] = Vector3.left * radio + padreRot.position;
            diferencia = this.transform.position - padreRot.position;
            for (int p = 0; p < extremosCir.Length; p += 1)
            {
                anguloChc = Vector3.Angle (diferencia, extremosCir[p]);
                diferenciaChc = Mathf.Abs (anguloChc - 90);
                if (diferenciaChc < mejorDif) 
                {
                    mejorDif = diferenciaChc;
                    mejorInd = p;
                }
            }
            sentidoHor = UnityEngine.Random.Range (0f, 1f) > 0.5f;
            agente.isStopped = false;
            destino = extremosCir[mejorInd];

            agente.SetDestination (destino);
        }
        else 
        {
            this.transform.parent = padreScr.transform;
        }
    }
}