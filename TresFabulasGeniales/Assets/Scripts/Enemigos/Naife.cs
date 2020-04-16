
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Naife : MonoBehaviour
{
    public enum Estado { normal, atacando, frenando };
    public Estado estado;

    [SerializeField] private bool quieto, sentidoHor, embestida, espera;
    private AreaNaifes padreScr;
    private CapsuleCollider capsula;
    private float centroY, radio, salud;
    private NavMeshAgent agente;
    private Animator animador;
    private Transform padreRot, objetivoTrf, modelo;
    private Vector3 destino, objetivoDir, deceleracion;
    private Quaternion rotacionObj;
    private List<Collider> collidersIgn;


    // Inicialización de variables.
    private void Start ()
    {
        estado = Estado.normal;
        quieto = true;
        padreScr = this.transform.parent.GetComponent<AreaNaifes> ();
        capsula = this.GetComponent<CapsuleCollider> ();
        centroY = capsula.bounds.center.y - capsula.bounds.extents.y * 0.8f;
        salud = padreScr.salud;
        agente = this.GetComponent<NavMeshAgent> ();
        agente.updateRotation = false;
        animador = this.GetComponentInChildren<Animator> ();
        padreRot = GameObject.Instantiate(new GameObject(), new Vector3 (this.transform.position.x, centroY, this.transform.position.z), Quaternion.identity).transform;
        padreRot.name = "Pivote " + this.name.ToLower ();
        padreRot.parent = padreScr.transform;
        modelo = animador.transform;
        collidersIgn = new List<Collider> ();
        //print (this.transform.localScale.x * capsula.radius);
        //print (capsula.bounds.extents.x);
    }


    // Según el estado en el que se encuentre el enemigo actualmente, realizamos distintas acciones: si está normal llamamos periódicamente a la función que alterna entre sus 2 estados del idle y nos aseguramos de que gire en caso de que no esté 
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
                if (agente.enabled == true) 
                {
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

                    RotarSegunVelocidad ();
                }

                break;
            default:
                if (collidersIgn.Count != 0) 
                {
                    DejarDeIgnorar ();
                }
                RotarSegunVelocidad ();

                if (agente.velocity.magnitude < padreScr.pararVel) 
                {
                    estado = Estado.atacando;
                    agente.velocity = Vector3.zero;
                    deceleracion = Vector3.zero;
                    agente.enabled = false;

                    if (espera == false) 
                    {
                        this.Invoke ("VolverALaCarga", 1.5f);
                    }
                    else 
                    {
                        //print ("Volvemos al pasado.");
                        VolverALaRutina ();
                    }

                    break;
                }

                deceleracion -= objetivoDir.normalized * Time.deltaTime * padreScr.frenadoVel;
                agente.velocity = objetivoDir.normalized * agente.speed + deceleracion;

                break;
        }

        RotarModelo ();
        Animar ();
    }


    // Pos debug como siempre.
    private void OnDrawGizmosSelected ()
    {
        if (capsula != null) 
        {
            Gizmos.color = Color.red;

            /*Gizmos.DrawWireSphere (padreRot.position, 5);
            Gizmos.DrawWireSphere (Vector3.forward * radio + padreRot.position, 5);
            Gizmos.DrawWireSphere (Vector3.right * radio + padreRot.position, 5);
            Gizmos.DrawWireSphere (Vector3.back * radio + padreRot.position, 5);
            Gizmos.DrawWireSphere (Vector3.left * radio + padreRot.position, 5);
            Gizmos.DrawWireCube (padreRot.position, new Vector3 (radio + capsula.bounds.size.x * 3.5f, 0.5f, radio + capsula.bounds.size.z * 3.5f));*/
            Gizmos.DrawWireSphere (capsula.bounds.center, capsula.bounds.extents.x);
        }
    }


    // Si un naife colisiona con el jugador mientras realiza una embestida, dejará de embestir y empezará a frenar, además se desactivarán las colisiones con el avatar brevemente y este recibirá daños. Si el choche se produce en el estado de 
    //frenado, la velocidad del agente pasará a ser nula.
    private void OnCollisionEnter (Collision collision)
    {
        switch (estado) 
        {
            case Estado.atacando:
                if (embestida == true && collision.transform.CompareTag ("Jugador") == true) 
                {
                    embestida = false;
                    estado = Estado.frenando;

                    Physics.IgnoreCollision (capsula, collision.collider, true);
                    collision.transform.GetComponent<Salud>().RecibirDanyo ();
                    collidersIgn.Add (collision.collider);
                }

                break;
            case Estado.frenando:
                agente.velocity = Vector3.zero;

                break;
        }
    }


    // El naife recibe el transform del jugador a atacar, deja de estar parado y pasa al estado de ataque.
    public void IniciarAtaque (Transform jugador) 
    {
        objetivoTrf = jugador;
        this.transform.parent = padreScr.transform;
        padreRot.rotation = Quaternion.identity;
        estado = Estado.atacando;
        agente.enabled = true;
        embestida = false;

        this.CancelInvoke ("QuietoOGirando");
    }


    // El naife vuelve a su estado normal, y lo preparamos para que encuentre un nuevo punto alrededor del cual girar.
    public void VolverALaRutina () 
    {
        if (embestida == false && Estado.frenando != estado) 
        {
            estado = Estado.normal;
            quieto = true;
            espera = false;
            agente.enabled = false;
            //print ("Estado normal.");

            this.CancelInvoke ("VolverALaCarga");
            this.Invoke ("QuietoOGirando", 1);
        }
        else 
        {
            espera = true;
        }
    }


    // El naife pierde la salud que corresponda, si se queda sin salud desactivamos el objeto y miramos si quedan otros naifes para atacar al jugador.
    public void Danyar (float danyo) 
    {
        salud -= danyo;
        if (salud < 0) 
        {
            padreScr.UnoMuerto (objetivoTrf);
            this.gameObject.SetActive (false);
        }
    }


    // Devuelve "true" si la salud del naife es menor a 0.
    public bool Vencido () 
    {
        return (salud < 0);
    }


    // Si el agente está moviéndose, nos aseguramos de que pare en el momento en que esté suficientemente cerca del radio que usará para rotar, definiendo también su nuevo padre y la rotación respecto al mismo. Si el agente ya no realiza ningún 
    //movimiento, este rotará respecto al padre hasta que alcance su rotación objetivo mientras el padre rota para simular que el naife corre en círculos.
    private void GirarAlrededor () 
    {
        if (agente.enabled == true)
        {
            RotarSegunVelocidad ();

            if (Mathf.Abs (Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (padreRot.position.x, padreRot.position.z)) - radio) < padreScr.distanciaMinObj)
            {
                agente.velocity = Vector3.zero;
                agente.enabled = false;
                this.transform.parent = padreRot;
                rotacionObj = Quaternion.Euler (this.transform.rotation.eulerAngles.x, Quaternion.LookRotation(this.transform.position - padreRot.position).eulerAngles.y + (sentidoHor == false ? +90 : -90), this.transform.rotation.z);
            }
        }
        else
        {
            if (Quaternion.Angle (this.transform.localRotation, rotacionObj) > 1)
            {
                this.transform.localRotation = Quaternion.Slerp (this.transform.localRotation, rotacionObj, Time.deltaTime * padreScr.velocidadRotGir);
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
                animador.SetBool ("frenando", false);
                
                break;
            case Estado.atacando:
                animador.SetBool ("moviendose", agente.enabled);
                animador.SetBool ("frenando", false);

                break;
            default:
                animador.SetBool ("frenando", true);

                break;
        }
    }


    // Si hay una línea recta sin obstáculos hasta el objetivo, el naife empieza su embestida contra el mismo.
    private void PuedoEmbestir () 
    {
        objetivoDir = new Vector3 (objetivoTrf.position.x - this.transform.position.x, 0, objetivoTrf.position.z - this.transform.position.z);
        embestida = !Physics.SphereCast (new Ray (capsula.bounds.center, objetivoDir), capsula.bounds.extents.x, objetivoDir.magnitude, padreScr.capasGirAtq, QueryTriggerInteraction.Ignore);
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


    // Se rota al naife para que la dirección hacia la que mira se corresponda con la velocidad del agente.
    private void RotarSegunVelocidad () 
    {
        this.transform.rotation = Quaternion.Slerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.eulerAngles.x, Mathf.Atan2 (agente.velocity.x, agente.velocity.z) * Mathf.Rad2Deg, this.transform.rotation.eulerAngles.z), 
            Time.deltaTime * padreScr.velocidadRotNor);
    }


    // Rotamos el modelo del naife de manera que, si este se encuentra tratando de embestir al jugador, agache un poco la cabeza.
    private void RotarModelo () 
    {
        if (Estado.frenando != estado) 
        {
            modelo.localRotation = Quaternion.Slerp (modelo.localRotation, embestida == false ? padreScr.modeloRotLoc[0] : padreScr.modeloRotLoc[1], Time.deltaTime * padreScr.velocidadRotMod);
        }
        else 
        {
            modelo.localRotation = Quaternion.Slerp (modelo.localRotation, padreScr.modeloRotLoc[2], Time.deltaTime * padreScr.velocidadRotMod);
        }
    }


    // Esta función se llama periódicamente en el estado normal para controlar que el naife alterna entre sus 2 idles. Cuando deja de estar quieto, encontramos un punto dentro del área enemiga alrededor del cuál dar vueltas, y alrededor del mismo 
    //el punto idóneo al cuál se dirigirá nuestro agente, decidiendo también el sentido en el cuál girará al llegar; si el enemigo pasa a estar quieto, dejará de ser hijo de este punto y reiniciaremos la rotación del antiguo padre.
    private void QuietoOGirando () 
    {
        quieto = !quieto;
        if (quieto == false) 
        {
            float aleatoriedad = Random.Range (-padreScr.radioGirVar, +padreScr.radioGirVar);
            float dimensionesXZ = capsula.bounds.size.z * 1.75f + padreScr.radioGirRan + aleatoriedad;

            radio = padreScr.radioGirRan + aleatoriedad;
            destino = padreScr.PosicionPivoteYDestino (padreRot, this.transform, new Vector3 (dimensionesXZ, 0.5f, dimensionesXZ), radio);
            sentidoHor = Random.Range (0f, 1f) > 0.5f;
            agente.enabled = true;

            agente.SetDestination (destino);
        }
        else 
        {
            this.transform.parent = padreScr.transform;
            padreRot.rotation = Quaternion.identity;
        }
    }


    // Tras haber frenado completamente después de una embestida, reactivamos el agente para que se prepare para volver a embestir al jugador. 
    private void VolverALaCarga () 
    {
        agente.enabled = true;
        embestida = false;
    }
}