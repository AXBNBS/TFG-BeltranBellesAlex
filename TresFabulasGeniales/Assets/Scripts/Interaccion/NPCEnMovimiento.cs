
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class NPCEnMovimiento : MonoBehaviour
{
    [SerializeField] private int cambioDst, rotacionVel, extraRotY;
    private NavMeshAgent agente;
    private List<Vector3> ruta;
    private int rutaInd;
    private Transform modeloTrf;


    // Inicialización de variables.
    private void Start ()
    {
        Transform[] rutaTrf = this.transform.GetChild(0).GetComponentsInChildren<Transform> ();

        agente = this.GetComponent<NavMeshAgent> ();
        agente.updateRotation = false;
        ruta = new List<Vector3> ();
        modeloTrf = this.GetComponentInChildren<Animator>().transform;

        for (int t = 1; t < rutaTrf.Length; t += 1) 
        {
            ruta.Add (rutaTrf[t].position);
        }
    }


    // Mientras el agente esté activo, se llama a una función que se encarga de que este siga la ruta establecida.
    private void Update ()
    {
        if (agente.enabled == true) 
        {
            SeguirRuta ();
        }
    }


    // Pa ver los puntos de la ruta bien.
    private void OnDrawGizmosSelected ()
    {
        if (ruta != null) 
        {
            Gizmos.color = Color.red;

            foreach (Vector3 p in ruta) 
            {
                Gizmos.DrawWireSphere (p, 5);
            }
        }
    }


    // El agente mira en la dirección a la cuál se dirige y además comprueba constantemente la distancia a la que se encuentra respecto al punto hacia el que se está moviendo. En caso de que esta distancia sea muy pequeña, hacemos que el siguiente
    //punto en la ruta se convierta en nuestro próximo objetivo.
    private void SeguirRuta () 
    {
        modeloTrf.rotation = Quaternion.Slerp (modeloTrf.rotation, Quaternion.Euler (modeloTrf.rotation.eulerAngles.x, Mathf.Atan2 (agente.velocity.x, agente.velocity.z) * Mathf.Rad2Deg + extraRotY, 
            modeloTrf.rotation.eulerAngles.z), Time.deltaTime * rotacionVel);
        if (Vector3.Distance (this.transform.position, ruta[rutaInd]) < cambioDst)
        {
            rutaInd = ruta.Count - 1 != rutaInd ? rutaInd + 1 : 0;
        }

        if (agente.destination.x != ruta[rutaInd].x || agente.destination.z != ruta[rutaInd].z) 
        {
            agente.SetDestination (ruta[rutaInd]);
        }
    }
}