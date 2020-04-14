
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Naife : MonoBehaviour
{
    public enum Estado { normal, atacando, frenando };
    public Estado estado;

    [SerializeField] private bool quieto, sentidoHor, embestida;
    private AreaNaifes padreScr;
    private Vector3[] extremosCir;
    private CapsuleCollider capsula;
    private float centroY, radio, destinoDst;
    private NavMeshAgent agente;
    private int indicePnt;
    private Transform padreRot, objetivoTrf;
    private Vector3 destino, objetivoDir, deceleracion;
    private Quaternion rotacionObj;
    private Animator animador;
    //private Rigidbody cuerpoRig;
    private List<Collider> collidersIgn;


    // Inicialización de variables.
    private void Start ()
    {
        estado = Estado.normal;
        quieto = true;
        padreScr = this.transform.parent.GetComponent<AreaNaifes> ();
        extremosCir = new Vector3[4];
        capsula = this.GetComponent<CapsuleCollider> ();
        centroY = capsula.bounds.center.y - capsula.bounds.extents.y * 0.8f;
        agente = this.GetComponent<NavMeshAgent> ();
        padreRot = GameObject.Instantiate(new GameObject(), new Vector3 (this.transform.position.x, centroY, this.transform.position.z), Quaternion.identity).transform;
        padreRot.name = "Pivote " + this.name.ToLower ();
        padreRot.parent = padreScr.transform;
        animador = this.GetComponentInChildren<Animator> ();
        //cuerpoRig = this.GetComponent<Rigidbody> ();
        collidersIgn = new List<Collider> ();
        //print (this.transform.localScale.x * capsula.radius);
        //print (capsula.bounds.extents.x);
    }


    // Según el estado en el que se encuentre el enemigo actualmente, realizamos distintas acciones: si está normal llamamos periódicamente a la función que alterna entre sus 2 estados del idle y nos aseguramos de que gire en caso de que nos esté 
    //quieto, ACTUALIZAR. En cualquier caso, siempre lo animamos como corresponda.
    private void Update ()
    {
        switch (estado) 
        {
            case Estado.normal:
                if (this.IsInvoking ("QuietoOGirando") == false) 
                {
                    this.Invoke ("QuietoOGirando", Random.Range (padreScr.segundosCmbLim[0], padreScr.segundosCmbLim[1]));
                }
                if (quieto == false) 
                {
                    GirarAlrededor ();
                }

                break;
            case Estado.atacando:
                agente.SetDestination (objetivoTrf.position);

                if (embestida == true) 
                {
                    if (Vector3.Angle (new Vector3 (objetivoTrf.position.x - this.transform.position.x, 0, objetivoTrf.position.z - this.transform.position.z), objetivoDir) < 90) 
                    {
                        agente.velocity = objetivoDir.normalized * agente.speed;
                    }
                    else 
                    {
                        embestida = false;
                        estado = Estado.frenando;
                    }
                }
                else 
                {
                    PuedoEmbestir ();
                }

                break;
            default:
                if (collidersIgn.Count != 0) 
                {
                    DejarDeIgnorar ();
                }

                print (agente.velocity.magnitude);
                if (agente.velocity.magnitude < padreScr.pararVel) 
                {
                    estado = Estado.atacando;
                }
                deceleracion -= objetivoDir.normalized * Time.deltaTime * padreScr.frenadoVel;
                agente.velocity = objetivoDir.normalized * agente.speed + deceleracion;

                break;
        }

        Animar ();
    }


    // Pos debug como siempre.
    /*private void OnDrawGizmosSelected ()
    {
        if (padreScr != null) 
        {
            Gizmos.color = Color.red;

            //Gizmos.DrawWireSphere (centro, 5);
            //Gizmos.DrawWireSphere (ruta[0], 5);
            //Gizmos.DrawWireSphere (ruta[1], 5);
            //Gizmos.DrawWireSphere (ruta[2], 5);
            //Gizmos.DrawWireSphere (ruta[3], 5);
            Gizmos.DrawWireCube (padreRot.position, new Vector3 (radio + capsula.bounds.size.x * 3.5f, 0.5f, radio + capsula.bounds.size.z * 3.5f));
        }
    }*/


    // .
    private void OnCollisionEnter (Collision collision)
    {
        if (embestida == true && collision.transform.CompareTag ("Jugador") == true) 
        {
            embestida = false;
            estado = Estado.frenando;

            Physics.IgnoreCollision (capsula, collision.collider, true);
            collision.transform.GetComponent<Salud>().RecibirDanyo ();
            collidersIgn.Add (collision.collider);
        }
        if (Estado.frenando == estado) 
        {
            agente.velocity = Vector3.zero;
        }
    }


    // El naife recibe el transform del jugador a atacar, deja de estar parado y pasa al estado de ataque.
    public void IniciarAtaque (Transform jugador) 
    {
        objetivoTrf = jugador;
        this.transform.parent = padreScr.transform;
        estado = Estado.atacando;
        agente.isStopped = false;
        embestida = false;

        this.CancelInvoke ("QuietoOGirando");
    }


    // Si el agente está moviéndose, nos aseguramos de que pare en el momento en que esté suficientemente cerca del radio que usará para rotar, definiendo también su nuevo padre y la rotación respecto al mismo. Si el agente ya no realiza ningún 
    //movimiento, este rotará respecto al padre hasta que alcance su rotación objetivo mientras el padre rota para simular que el naife corre en círculos.
    private void GirarAlrededor () 
    {
        if (agente.isStopped == false)
        {
            if (Mathf.Abs (Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (padreRot.position.x, padreRot.position.z)) - radio) < padreScr.distanciaMinObj)
            {
                agente.velocity = Vector3.zero;
                agente.isStopped = true;
                this.transform.parent = padreRot;
                rotacionObj = Quaternion.Euler (this.transform.rotation.eulerAngles.x, Quaternion.LookRotation(this.transform.position - padreRot.position).eulerAngles.y + (sentidoHor == false ? +90 : -90), this.transform.rotation.z);
            }
        }
        else
        {
            if (Quaternion.Angle (this.transform.localRotation, rotacionObj) > 1)
            {
                this.transform.localRotation = Quaternion.Slerp (this.transform.localRotation, rotacionObj, Time.deltaTime * padreScr.rotacionVel);
            }

            padreRot.Rotate (new Vector3 (0, sentidoHor == false ? +padreScr.giroVel : -padreScr.giroVel, 0) * Time.deltaTime);
        }
    }


    // Animamos el naife en consecuencia de su estado actual.
    private void Animar () 
    {
        switch (estado) 
        {
            case Estado.normal:
                animador.SetBool ("moviendose", !quieto);
                
                break;
            case Estado.atacando:
                animador.SetBool ("moviendose", true);

                break;
            default:

                break;
        }
    }


    // Si hay una línea recta sin obstáculos hasta el objetivo, el naife empieza su embestida contra el mismo.
    private void PuedoEmbestir () 
    {
        Vector3 esferaSupCen = new Vector3 (capsula.bounds.center.x, capsula.bounds.center.y + this.transform.localScale.y * (capsula.height / 2 - capsula.radius), capsula.bounds.center.z);

        objetivoDir = new Vector3 (objetivoTrf.position.x - this.transform.position.x, 0, objetivoTrf.position.z - this.transform.position.z);
        embestida = !Physics.CapsuleCast (esferaSupCen, new Vector3 (esferaSupCen.x, esferaSupCen.y - this.transform.localScale.y * (capsula.height - capsula.radius * 2), esferaSupCen.z), capsula.bounds.extents.x, objetivoDir, objetivoDir.magnitude,
            padreScr.capasGirAtq, QueryTriggerInteraction.Ignore);
    }


    // Los colliders que se ha establecido previamente que han de ser ignorados son revisados y, si hay la suficiente distancia entre ellos y el collider del naife estos vuelven a ser tenidos en cuenta y eliminados de la lista de ignorados.
    private void DejarDeIgnorar () 
    {
        List<Collider> eliminar = new List<Collider> ();

        foreach (Collider c in collidersIgn) 
        {
            if (Vector2.Distance (new Vector2 (capsula.bounds.center.x, capsula.bounds.center.z), new Vector2 (c.bounds.center.x, c.bounds.center.z)) > padreScr.distanciaParIgn) 
            {
                Physics.IgnoreCollision (capsula, c, false);
                eliminar.Add (c);
            }
        }
        foreach (Collider c in eliminar) 
        {
            collidersIgn.Remove (c);
        }
    }


    // Esta función se llama periódicamente en el estado normal para controlar que el naife alterna entre sus 2 idles. Cuando deja de estar quieto, encontramos un punto dentro del área enemiga alrededor del cuál dar vueltas, y alrededor del mismo 
    //el punto idóneo al cuál se dirigirá nuestro agente, decidiendo también el sentido en el cuál girará al llegar; si el enemigo pasa a estar quieto, dejará de ser hijo de este punto y reiniciaremos la rotación del antiguo padre.
    private void QuietoOGirando () 
    {
        quieto = !quieto;
        if (quieto == false) 
        {
            float anguloChc, diferenciaChc;
            Vector3 diferencia;

            float aleatoriedad = Random.Range (-padreScr.radioGirVar, +padreScr.radioGirVar);
            float dimensionesXZ = capsula.bounds.size.z * 3.5f + padreScr.radioGirRan + aleatoriedad;
            float mejorDif = 90;
            int mejorInd = 0;

            padreRot.position = padreScr.PuntoAleatorioDentro (new Vector3 (this.transform.position.x, centroY, this.transform.position.z), new Vector3 (dimensionesXZ, 0.5f, dimensionesXZ));
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
            sentidoHor = Random.Range (0f, 1f) > 0.5f;
            agente.isStopped = false;
            destino = extremosCir[mejorInd];

            agente.SetDestination (destino);
        }
        else 
        {
            this.transform.parent = padreScr.transform;
            padreRot.rotation = Quaternion.identity;
        }
    }
}