
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[RequireComponent (typeof (SphereCollider))]
public class AreaNaifes : MonoBehaviour
{
    public float[] segundosCmbLim;
    public float radioGirRan, radioGirVar, giroVel;
    public LayerMask capasGirAtq;
    public int velocidadRotNor, velocidadRotGir, velocidadRotModEmbFrn, velocidadRotModEsp, distanciaMinObj, distanciaParIgn, frenadoVel, pararVel, salud;
    [HideInInspector] public Quaternion[] modeloRotLoc;
    [HideInInspector] public Naife[] naifes;
    [HideInInspector] public IDictionary<Transform, Naife> avataresPer;

    private SphereCollider trigger;
    private List<Transform> avataresDen;


    // Inicialización de variables.
    private void Awake ()
    {
        modeloRotLoc = new Quaternion[] { Quaternion.Euler (0, 90, -20), Quaternion.Euler (0, 90, 5) , Quaternion.Euler (0, 90, -35) };
        naifes = this.GetComponentsInChildren<Naife> ();
        avataresPer = new Dictionary<Transform, Naife> ();
        trigger = this.GetComponent<SphereCollider> ();
        trigger.isTrigger = true;
        avataresDen = new List<Transform> ();
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


    // El entrar el jugador en la zona, encontramos al primer enemigo sin blanco asignado. Si este ha podido ser encontrado sin problemas, inicia el ataque hacia el jugador y añadimos el par "avatar-naife" al diccionario.
    private void OnTriggerEnter (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true)
        {
            if (avataresPer.ContainsKey (other.transform) == false) 
            {
                Naife perseguidor = PrimeroSinBlanco ();

                if (perseguidor != null)
                {
                    perseguidor.IniciarAtaque (other.transform);
                    avataresPer.Add (other.transform, perseguidor);
                }
            }

            avataresDen.Add (other.transform);
        }
    }


    // Al salir el jugador de la zona, aquel enemigo que estuviese persiguiéndolo vuelve a su rutina habitual en caso de que no haya más avatares o, si los hay, estos ya tengan algún perseguidor asignado. En el caso de que esto no sea así, el naife
    //que ha perdido a su blanco se convertirá en el nuevo perseguidor del otro.
    private void OnTriggerExit (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true)
        {
            avataresDen.Remove (other.transform);
            if (other.GetComponent<MovimientoHistoria2>().input == true && avataresPer.ContainsKey (other.transform) == true) 
            {
                if (avataresDen.Count == 0 || avataresPer.ContainsKey (avataresDen[0]) == true) 
                {
                    avataresPer[other.transform].VolverALaRutina ();
                }
                else 
                {
                    avataresPer[other.transform].IniciarAtaque (avataresDen[0]);
                    avataresPer.Add (avataresDen[0], avataresPer[other.transform]);
                }
                avataresPer.Remove (other.transform);
            }
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

        //print ("Antes del while.");
        while (obstaculos.Count != 0 || (Vector2.Distance (new Vector2 (trigger.bounds.center.x, trigger.bounds.center.z), new Vector2 (centro.x, centro.z)) + radio) > trigger.radius) 
        {
            /*foreach (Collider c in obstaculos) 
            {
                print (c.name);
            }
            print ("Antes del random.");*/
            centro = Random.insideUnitSphere * trigger.radius + trigger.bounds.center;
            centro.y = pivote.position.y;
            //print ("Después del random.");
            obstaculos = Physics.OverlapBox(centro, cajaDim, Quaternion.identity, capasGirAtq, QueryTriggerInteraction.Ignore).ToList<Collider> ();

            obstaculos.Remove (naifeCol);
        }
        //print ("Después del while.");

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


    // Si uno de los naifes muere, miramos si queda alguno sin atacar a nadie y hacemos que este vaya tras el avatar que ha matado al primer naife (asumiendo que este sigue dentro de la zona). También eliminamos del diccionario el par "avatar-naife"
    //antiguo y lo sustituimos por el nuevo, si existe.
    public void UnoMuerto (Transform avatar) 
    {
        Naife perseguidor = PrimeroSinBlanco ();

        if (avataresPer.Remove (avatar) == true && perseguidor != null) 
        {
            perseguidor.IniciarAtaque (avatar);
            avataresPer.Add (avatar, perseguidor);
        }
    }


    // Si el avatar al que pertenece el character controller no está dentro del área, llamamos a "OnTriggerExit" para asegurarnos de que los naifes lo dejan en paz.
    public void MirarSiQuitoAvatar (Collider personajeCtr) 
    {
        if (avataresDen.Contains (personajeCtr.transform) == false) 
        {
            this.OnTriggerExit (personajeCtr);
        }
    }


    // Función que devuelve el primero de los naifes de la zona que no esté atacando a nadie.
    private Naife PrimeroSinBlanco () 
    {
        foreach (Naife n in naifes) 
        {
            if (n.Vencido () == false && Naife.Estado.normal == n.estado) 
            {
                return n;
            }
        }
        return null;
    }
}