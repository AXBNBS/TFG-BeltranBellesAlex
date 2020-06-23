
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Naife : MonoBehaviour
{
    public enum Estado { normal, atacando, frenando, saltando, muriendo };
    public Estado estado;

    [SerializeField] private bool alturaCmp;
    public bool quieto, sentidoHor, embestida, espera, animarFin, gatosCer, agresivo, saltado, chocado, controladoAvt;
    private AreaNaifes padreScr;
    private CapsuleCollider capsula;
    private float centroY, radio, salud, tiempoEmb, saltoDstMax, saltoDst;
    private NavMeshAgent agente;
    private Animator animador;
    private Transform padreRot, objetivoTrf, modelo;
    private Vector3 destino, destinoSal, objetivoDir, deceleracion, saltoDir, sueloOff;
    private Quaternion rotacionObj;
    private List<Collider> collidersIgn;
    private EspaldaEnemigo[] espalda;
    private NavMeshPath camino;


    // Inicialización de variables.
    private void Start ()
    {
        estado = Estado.normal;
        quieto = true;
        padreScr = this.transform.parent.GetComponent<AreaNaifes> ();
        capsula = this.GetComponent<CapsuleCollider> ();
        centroY = capsula.bounds.center.y - capsula.bounds.extents.y * 0.8f;
        salud = padreScr.salud;
        saltoDstMax = padreScr.longitudRaySal - capsula.bounds.extents.x;
        agente = this.GetComponent<NavMeshAgent> ();
        agente.updateRotation = false;
        animador = this.GetComponentInChildren<Animator> ();
        padreRot = GameObject.Instantiate(new GameObject (), new Vector3 (this.transform.position.x, centroY, this.transform.position.z), Quaternion.identity).transform;
        padreRot.name = "Pivote " + this.name.ToLower ();
        padreRot.parent = padreScr.transform;
        modelo = animador.transform;
        sueloOff = capsula.bounds.size.y * Vector3.down;
        collidersIgn = new List<Collider> ();
        espalda = new EspaldaEnemigo[] { this.transform.GetChild(1).GetComponent<EspaldaEnemigo> (), this.transform.GetChild(2).GetComponent<EspaldaEnemigo> () };
        camino = new NavMeshPath ();
    }


    // Según el estado en el que se encuentre el enemigo actualmente, realizamos distintas acciones: si está normal llamamos periódicamente a la función que alterna entre sus 2 estados del idle y nos aseguramos de que gire en caso de que no esté 
    //quieto, si está atacando y con su agente habilitado hacemos que embista al jugador en cuanto pueda, y pare si ha pasado de largo y lleva un mínimo de tiempo embistiendo, si esta frenando hacemos que la velocidad el agente descienda poco a poco
    //y se prepare para volver al estado normal o de ataque (según convenga) tras un breve periodo de tiempo, si está saltando nos aseguramos de que lo haga en dirección a su destino fijado y con una velocidad adecuada, parando si llega a este o 
    //o choca para volver de nuevo a su estado de ataque o normal. En cualquier caso, siempre rotamos el modelo y lo animamos como corresponda.
    private void Update ()
    {
        if (collidersIgn.Count != 0)
        {
            DejarDeIgnorar ();
        }
        switch (estado) 
        {
            case Estado.normal:
                if (gatosCer == false && this.IsInvoking ("QuietoOGirando") == false) 
                {
                    this.Invoke ("QuietoOGirando", Random.Range (padreScr.segundosCmbLim[0], padreScr.segundosCmbLim[1]));
                }
                if (quieto == false) 
                {
                    GirarAlrededor ();
                    if (alturaCmp == true && agente.enabled == false) 
                    {
                        AjustarAltura ();
                    }
                }

                break;
            case Estado.atacando:
                if (agente.enabled == true) 
                {
                    if (CaminoHaciaObjetivo (objetivoTrf.position) == true) 
                    {
                        agente.SetDestination (objetivoTrf.position);

                        if (embestida == true)
                        {
                            if (padreScr.tiempoMinEmb > tiempoEmb || Vector3.Angle (new Vector3 (objetivoTrf.position.x - this.transform.position.x, 0, objetivoTrf.position.z - this.transform.position.z), objetivoDir) < 100)
                            {
                                agente.velocity = objetivoDir * agente.speed;
                                tiempoEmb += Time.deltaTime;
                            }
                            else
                            {
                                tiempoEmb = 0;
                                embestida = false;
                                estado = Estado.frenando;
                            }
                        }
                        else
                        {
                            PuedoEmbestir ();
                        }
                    }
                    else 
                    {
                        if (embestida == true) 
                        {
                            tiempoEmb = 0;
                            embestida = false;
                            estado = Estado.frenando;
                        }
                        else 
                        {
                            VolverALaRutina ();
                        }

                        padreScr.avataresPer.Remove (objetivoTrf);
                    }

                    RotarSegunVelocidad ();
                }

                break;
            case Estado.frenando:
                RotarSegunVelocidad ();

                if (agente.velocity.magnitude < padreScr.pararVel) 
                {
                    estado = Estado.atacando;
                    //agente.velocity = Vector3.zero;
                    deceleracion = Vector3.zero;
                    agente.enabled = false;
                    //print ("Suposadament.");

                    if (espera == false) 
                    {
                        //print ("Vuelvo a la carga en 2 secs.");
                        this.Invoke ("VolverALaCarga", controladoAvt == true ? 1 : 1.5f);
                    }
                    else 
                    {
                        //print ("Vuelta al pasado desde el estado de frenado.");
                        VolverALaRutina ();
                    }

                    break;
                }

                deceleracion -= objetivoDir * Time.deltaTime * padreScr.frenadoVel;
                agente.velocity = objetivoDir * agente.speed + deceleracion;

                break;
            case Estado.saltando:
                float distancia = Vector3.Distance (this.transform.position, destinoSal);

                agente.velocity = saltoDir * padreScr.saltoVelMax * distancia / saltoDst;
                if (chocado == true || padreScr.distanciaMinObj > distancia) 
                {
                    estado = Estado.atacando;
                    agente.enabled = false;
                    saltado = false;
                    chocado = false;
                    //print ("Salto completado.");

                    if (agresivo == true) 
                    {
                        //print ("Vuelvo a la carga en 1 sec.");
                        this.Invoke ("VolverALaCarga", 0.5f);
                    }
                    else 
                    {
                        //print ("Vuelta al pasado desde el estado de salto.");
                        VolverALaRutina ();
                    }
                }

                break;
        }

        RotarModelo ();
        if (animarFin == false) 
        {
            Animar ();
        }
    }


    // Pos debug como siempre.
    private void OnDrawGizmosSelected ()
    {
        if (capsula != null) 
        {
            Gizmos.color = Color.red;

            //capsula.bounds.size.z * 1.75f + padreScr.radioGirRan + aleatoriedad
            Gizmos.DrawWireCube (this.transform.position, new Vector3 ((capsula.bounds.extents.z + padreScr.radioGirRan + 10) * 2, 0, (capsula.bounds.extents.z + padreScr.radioGirRan + 10) * 2));
            Gizmos.DrawRay (capsula.bounds.center, capsula.bounds.size.y * Vector3.down);
        }
        /*Gizmos.DrawRay (this.transform.position, this.transform.forward * saltoDstMax);
        Gizmos.DrawRay (this.transform.position, -this.transform.forward * saltoDstMax);
        Gizmos.DrawWireSphere (destinoSal, 5);
        if (capsula != null) 
        {

            Gizmos.DrawWireSphere (padreRot.position, 5);
            Gizmos.DrawWireSphere (Vector3.forward * radio + padreRot.position, 5);
            Gizmos.DrawWireSphere (Vector3.right * radio + padreRot.position, 5);
            Gizmos.DrawWireSphere (Vector3.back * radio + padreRot.position, 5);
            Gizmos.DrawWireSphere (Vector3.left * radio + padreRot.position, 5);
            Gizmos.DrawWireCube (padreRot.position, new Vector3 (radio + capsula.bounds.size.x * 3.5f, 0.5f, radio + capsula.bounds.size.z * 3.5f));
            Gizmos.DrawWireSphere (capsula.bounds.center, capsula.bounds.extents.x);
        }*/
    }


    // Si un naife colisiona con el jugador mientras realiza una embestida, dejará de embestir y empezará a frenar, además se desactivarán las colisiones con el avatar brevemente y este recibirá daños. Si el choche se produce en el estado de 
    //frenado, la velocidad del agente pasará a ser nula.
    private void OnCollisionEnter (Collision collision)
    {
        switch (estado) 
        {
            case Estado.normal:
                if (quieto == false && collision.transform.CompareTag ("Jugador") == true) 
                {
                    Physics.IgnoreCollision (capsula, collision.collider, true);
                    collidersIgn.Add (collision.collider);
                }

                break;
            case Estado.atacando:
                if (embestida == true && collision.transform.CompareTag ("Jugador") == true) 
                {
                    Physics.IgnoreCollision (capsula, collision.collider, true);
                    collision.transform.GetComponent<Salud>().RecibirDanyo (agente.velocity.normalized);
                    collidersIgn.Add (collision.collider);
                }
                else 
                {
                    if (embestida == true && collision.transform.CompareTag ("Enemigo") == false) 
                    {
                        //print ("Weno cuidao.");
                        embestida = false;
                        estado = Estado.frenando;
                        agente.velocity = Vector3.zero;
                    }
                }

                break;
            case Estado.frenando:
                agente.velocity = Vector3.zero;

                break;
            case Estado.saltando:
                if (espalda[this.transform.forward == saltoDir ? 1 : 0].obstaculos.Contains (collision.collider) == true) 
                {
                    chocado = true;
                }

                break;
        }
    }


    // El naife recibe el transform del jugador a atacar, deja de estar parado y pasa al estado de ataque.
    public void IniciarAtaque (Transform jugador) 
    {
        if (CaminoHaciaObjetivo (jugador.position) == true) 
        {
            objetivoTrf = jugador;
            controladoAvt = objetivoTrf.GetComponent<Ataque>().input;
            this.transform.parent = padreScr.transform;
            padreRot.rotation = Quaternion.identity;
            estado = Estado.atacando;
            agente.enabled = true;
            embestida = false;

            ChecarNavMesh ();
            this.CancelInvoke ("QuietoOGirando");
            padreScr.avataresPer.Add (jugador, this);
        }
    }


    // El naife vuelve a su estado normal, y lo preparamos para que encuentre un nuevo punto alrededor del cual girar.
    public void VolverALaRutina () 
    {
        if (embestida == false && Estado.frenando != estado && Estado.saltando != estado) 
        {
            estado = Estado.normal;
            quieto = true;
            espera = false;
            agente.enabled = false;
            //print ("Suposadament.");

            this.CancelInvoke ("VolverALaCarga");
            this.Invoke ("QuietoOGirando", 0.5f);
        }
        else 
        {
            //print ("Se me llamó para esperar.");
            espera = true;
        }
    }


    // El naife pierde la salud que corresponda, si se queda sin salud desactivamos el objeto y miramos si quedan otros naifes para atacar al jugador.
    public void Danyar (float danyo, bool controlado) 
    {
        salud -= danyo;
        controladoAvt = controlado;

        if (salud > 0) 
        {
            Saltar ();
        }
        else
        {
            GestionarMuerte ();
        }
    }


    // Devuelve "true" si la salud del naife es menor a 0.
    public bool Vencido () 
    {
        return (salud < 0);
    }


    // Devuelve una distancia de salto adecuada respecto al collider del enemigo para que Violeta la use para decidir cuando saltar si su IA se enfrenta al enemigo.
    public float ObtenerDistanciaDeSaltoOptima () 
    {
        return (capsula.bounds.extents.x * 1.5f);
    }


    // Devuelve una distancia respecto al collider del enemigo que sea adecuada para que Abedul pueda decidir cuando arañar al mismo.
    public float ObtenerDistanciaDeAranyazoOptima (Bounds limites)
    {
        return (capsula.bounds.extents.x + limites.extents.x * 1.2f);
    }


    // Se llama cuando el área ha pasado de estar totalmente vacía a con un gato dentro o viceversa, y sólo para los naifes que en ese momento no estén atacando. Permite hacer que todos los naifes sin blanco estén constantemente dando vueltas o 
    //revertir su estado al habitual.
    public void CorrerSiempre (bool siempre) 
    {
        gatosCer = siempre;

        this.CancelInvoke ("QuietoOGirando");
        if (gatosCer == true && quieto == true) 
        {
            QuietoOGirando ();
        }
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
            case Estado.frenando:
                animador.SetBool ("frenando", true);

                break;
            case Estado.saltando:
                if (saltado == false) 
                {
                    saltado = true;

                    animador.SetTrigger ("saltando");
                }

                break;
            default:
                animarFin = true;

                animador.SetTrigger ("derrotado");

                break;
        }
    }


    // Si hay una línea recta sin obstáculos hasta el objetivo, el naife empieza su embestida contra el mismo.
    private void PuedoEmbestir () 
    {
        objetivoDir = new Vector3 (objetivoTrf.position.x - this.transform.position.x, 0, objetivoTrf.position.z - this.transform.position.z);
        if (objetivoDir.magnitude == 0) 
        {
            objetivoDir = Vector3.Distance (this.transform.position + this.transform.forward, padreScr.transform.position) < Vector3.Distance (this.transform.position - this.transform.forward, padreScr.transform.position) ? this.transform.forward : 
                -this.transform.forward;
        }
        //print (objetivoDir);
        embestida = Mathf.Abs (capsula.bounds.min.y - objetivoTrf.position.y) < 10 && !Physics.SphereCast (new Ray (capsula.bounds.center, objetivoDir), capsula.bounds.extents.x, objetivoDir.magnitude, padreScr.capasGirAtq, QueryTriggerInteraction.Ignore);
        objetivoDir = objetivoDir.normalized;
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
            if (Estado.muriendo != estado) 
            {
                if (embestida == false)
                {
                    modelo.localRotation = Quaternion.Slerp (modelo.localRotation, padreScr.modeloRotLoc[0], Time.deltaTime * padreScr.velocidadRotModEsp);
                }
                else
                {
                    modelo.localRotation = Quaternion.Slerp (modelo.localRotation, padreScr.modeloRotLoc[1], Time.deltaTime * padreScr.velocidadRotModEmbFrn);
                }
            }
            else 
            {
                modelo.localRotation = Quaternion.Slerp (modelo.localRotation, padreScr.modeloRotLoc[3], Time.deltaTime * padreScr.velocidadRotModMue);
            }
        }
        else 
        {
            modelo.localRotation = Quaternion.Slerp (modelo.localRotation, padreScr.modeloRotLoc[2], Time.deltaTime * padreScr.velocidadRotModEmbFrn);
        }
    }


    // Si el naife se ha quedado sin salud, cambiamos al estado de muerte, desactivamos todos los colliders, el agente, el script de parpadeo, cancelamos todos los "invokes" y iniciamos otro para desactivar el objeto tras un tiempo.
    private void GestionarMuerte () 
    {
        estado = Estado.muriendo;
        agente.enabled = false;
        capsula.enabled = false;
        this.GetComponent<Parpadeo>().enabled = false;
        this.transform.parent = padreScr.transform;
        //this.transform.localPosition = new Vector3 (this.transform.localPosition.x, padreScr.muertePosYLoc, this.transform.localPosition.z);

        GameObject.Destroy (this.GetComponent<Rigidbody> ());
        this.transform.GetChild(0).gameObject.SetActive (false);
        this.CancelInvoke ("QuietoOGirando");
        this.CancelInvoke ("VolverALaCarga");
        this.Invoke ("Desaparecer", 5);
        this.Invoke ("Venganza", 2);
    }


    // Si el naife se encuentra quieto en su estado de ataque o de patrulla, lanzamos un rayo hacia adelante o atrás (según el lado por el cuál esté siendo atacado) y si hay suficiente distancia libre o directamente no hay ningún obstáculo, hacemos 
    //que salte la distancia que corresponda.
    private void Saltar (Vector3 direccion = new Vector3 ())
    {
        saltoDir = direccion;
        //print (saltoDir);
        if ((agente.enabled == false && Estado.atacando == estado) || (quieto == true && Estado.normal == estado)) 
        {
            if (saltoDir == Vector3.zero) 
            {
                saltoDir = Vector3.Distance (this.transform.position + this.transform.forward, objetivoTrf.position) > Vector3.Distance (this.transform.position - this.transform.forward, objetivoTrf.position) ? this.transform.forward :
                    -this.transform.forward;
            }
            if (Physics.SphereCast (new Ray (capsula.bounds.center, saltoDir), capsula.bounds.extents.x, out RaycastHit datosRay, padreScr.longitudRaySal, padreScr.capasSal, QueryTriggerInteraction.Ignore) == true)
            {
                if (datosRay.distance > capsula.bounds.extents.x * 3)
                {
                    destinoSal = this.transform.position + (datosRay.distance - capsula.bounds.extents.x) * saltoDir;
                    if (Physics.Raycast (destinoSal, Vector3.down, capsula.bounds.size.y * 1.2f, padreScr.sueloCap, QueryTriggerInteraction.Ignore) == true)
                    {
                        saltoDst = datosRay.distance;

                        PasarASalto ();
                    }
                    else 
                    {
                        if (direccion == Vector3.zero) 
                        {
                            Saltar (-1 * saltoDir);
                        }
                    }
                }
            }
            else
            {
                destinoSal = this.transform.position + saltoDstMax * saltoDir;
                if (Physics.Raycast (destinoSal, Vector3.down, capsula.bounds.size.y * 1.2f, padreScr.sueloCap, QueryTriggerInteraction.Ignore) == true)
                {
                    saltoDst = saltoDstMax;

                    PasarASalto ();
                }
                else 
                {
                    if (direccion == Vector3.zero) 
                    {
                        Saltar (-1 * saltoDir);
                    }
                }
            }
        }
    }


    // Ejecuta todos los cambios necesarios para pasar al estado de salto.
    private void PasarASalto ()
    {
        agresivo = Estado.atacando == estado;
        estado = Estado.saltando;
        agente.enabled = true;

        //print ("Todos los invokes anulados.");
        this.CancelInvoke ("QuietoOGirando");
        this.CancelInvoke ("VolverALaCarga");
    }


    // Esta función se llama periódicamente en el estado normal para controlar que el naife alterna entre sus 2 idles. Cuando deja de estar quieto, encontramos un punto dentro del área enemiga alrededor del cuál dar vueltas, y alrededor del mismo 
    //el punto idóneo al cuál se dirigirá nuestro agente, decidiendo también el sentido en el cuál girará al llegar; si el enemigo pasa a estar quieto, dejará de ser hijo de este punto y reiniciaremos la rotación del antiguo padre.
    private void QuietoOGirando () 
    {
        quieto = !quieto;
        if (quieto == false) 
        {
            float aleatoriedad = Random.Range (-padreScr.radioGirVar, +padreScr.radioGirVar);
            float dimensionesXZ = capsula.bounds.extents.z + padreScr.radioGirRan + aleatoriedad;

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

            ChecarNavMesh ();
        }
    }


    // Comprueba si el agente se encuentra en una posición que existe dentro del NavMesh y, si este no es el caso, se le mueve a la posición más cercana perteneciente al NavMesh que encontremos.
    private void ChecarNavMesh () 
    {
        NavMesh.SamplePosition (this.transform.position, out NavMeshHit datos, capsula.bounds.size.x, NavMesh.AllAreas);

        if (datos.position.magnitude != float.PositiveInfinity && (Mathf.Approximately (this.transform.position.x, datos.position.x) == false || Mathf.Approximately (this.transform.position.x, datos.position.z) == false)) 
        {
            this.transform.position = new Vector3 (datos.position.x, this.transform.position.y, datos.position.z);
        }
    }


    // Se lanza un raycast desde el centro del enemigo hasta abajo y, si este choca con algún collider, ajustamos la posición del naife para que este se posicione correctamente apoyado sobre el suelo.
    private void AjustarAltura () 
    {
        if (Physics.Raycast (capsula.bounds.center, Vector3.down, out RaycastHit info, capsula.bounds.size.y, padreScr.sueloCap, QueryTriggerInteraction.Ignore) == true) 
        {
            this.transform.position = new Vector3 (this.transform.position.x, capsula.bounds.size.y + info.point.y, this.transform.position.z);
        }
    }


    // Devuelve verdadero si encontramos un camino completo desde la posición del naife a la de su objetivo, suponiendo que este se encuentre a su misma altura.
    private bool CaminoHaciaObjetivo (Vector3 avatarPos) 
    {
        Vector3 naifePosSue = this.transform.position + sueloOff;

        NavMesh.CalculatePath (naifePosSue, new Vector3 (avatarPos.x, naifePosSue.y, avatarPos.z), NavMesh.AllAreas, camino);
        print (this.name + ": " + camino.status);

        return NavMeshPathStatus.PathComplete == camino.status;
    }


    // Tras haber frenado completamente después de una embestida, reactivamos el agente para que se prepare para volver a embestir al jugador. 
    private void VolverALaCarga () 
    {
        agente.enabled = true;
        embestida = false;
    }


    // Tras la animación de muerte, se llama a esta función para desactivar el objeto del naife y que el script del padre realice las gestiones que correspondan.
    private void Desaparecer () 
    {
        this.gameObject.SetActive (false);
    }


    // El script del padre realiza las gestiones que correspondan para que, si hay algún naife con vida, este empiece a atacar al avatar.
    private void Venganza () 
    {
        padreScr.UnoMuerto (objetivoTrf);
    }
}