
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Naife : MonoBehaviour
{
    [SerializeField] private bool quieto, sentidoHor;
    private AreaNaifes padreScr;
    private Vector3[] ruta;
    private CapsuleCollider capsula;
    private float centroY;
    private NavMeshAgent agente;
    private int indicePnt;
    

    // .
    private void Start ()
    { 
        padreScr = this.transform.parent.GetComponent<AreaNaifes> ();
        ruta = new Vector3[4];
        capsula = this.GetComponent<CapsuleCollider> ();
        centroY = capsula.bounds.center.y - capsula.bounds.extents.y * 0.8f;
        agente = this.GetComponent<NavMeshAgent> ();

        this.InvokeRepeating ("QuietoOGirando", 0, UnityEngine.Random.Range (padreScr.segundosCmbLim[0], padreScr.segundosCmbLim[1]));
    }


    // .
    private void Update ()
    {
        if (quieto == false && Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (agente.destination.x, agente.destination.z)) < padreScr.distanciaMinObj)
        {
            if (sentidoHor == false) 
            {
                indicePnt = indicePnt != ruta.Length - 1 ? indicePnt + 1 : 0;
            }
            else 
            {
                indicePnt = indicePnt != 0 ? indicePnt - 1 : ruta.Length - 1;
            }

            agente.SetDestination (ruta[indicePnt]);
        }

        agente.velocity = agente.desiredVelocity;
    }


    // .
    private void OnDrawGizmosSelected ()
    {
        if (padreScr != null) 
        {
            Gizmos.DrawWireSphere (this.transform.position, padreScr.radioGirRan);
        }
        //Gizmos.DrawWireCube (this.transform.position, new Vector3 (padreScr.radioGirRan + capsula.bounds.size.x, 0.5f, padreScr.radioGirRan + capsula.bounds.size.x));
    }


    // .
    private void QuietoOGirando () 
    {
        quieto = !quieto;
        if (quieto == false) 
        {
            float aleatoriedad = UnityEngine.Random.Range (-padreScr.radioGirVar, +padreScr.radioGirVar);
            Vector3 centro = new Vector3 (this.transform.position.x, centroY, this.transform.position.z);
            float dimensionesXZ = padreScr.radioGirRan + aleatoriedad + capsula.bounds.size.z;

            while (Physics.CheckBox (centro, new Vector3 (dimensionesXZ, 0.5f, dimensionesXZ), Quaternion.Euler (0, 45, 0), padreScr.capasGir, QueryTriggerInteraction.Ignore) == true) 
            {
                centro = new Vector3 (centro.x + UnityEngine.Random.Range (-padreScr.nuevoPntOff, +padreScr.nuevoPntOff), centroY, centro.z + UnityEngine.Random.Range (-padreScr.nuevoPntOff, +padreScr.nuevoPntOff));
            }
            ruta[0] = this.transform.forward * (padreScr.radioGirRan + aleatoriedad) + centro;
            ruta[1] = this.transform.right * (padreScr.radioGirRan + aleatoriedad) + centro;
            ruta[2] = -this.transform.forward * (padreScr.radioGirRan + aleatoriedad) + centro;
            ruta[3] = -this.transform.right * (padreScr.radioGirRan + aleatoriedad) + centro;
            sentidoHor = UnityEngine.Random.Range (0f, 1f) > 0.5f;
            agente.isStopped = false;

            agente.SetDestination (ruta[UnityEngine.Random.Range (0, ruta.Length)]);
        }
        else 
        {
            agente.isStopped = true;
        }
    }
}