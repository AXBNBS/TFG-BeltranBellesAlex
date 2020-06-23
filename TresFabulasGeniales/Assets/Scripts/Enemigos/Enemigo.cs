
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Enemigo : MonoBehaviour
{
    public Transform avatarTrf;
    public bool acercarse, danyado;
    [HideInInspector] public int prioridad;

    [SerializeField] private float puntosGol, rangoAtq, limiteY;
    [SerializeField] private int saltoDef, aranyazoDef, aleatoriedad, velocidadMov, velocidadRot, distanciaMaxObj, saltoRayDst, pararSalDst, saltoVel, aturdimientoVelY;
    [SerializeField] private LayerMask capasSal, capsulaCap, rayoCap;
    private LayerMask jugadorCap;
    private bool perseguir, reposicionado, objetivo1, alcanzadoObj, parado, atacado, cercanoAvt, saltando, esperar;
    private Vector3 posicionIni, destinoRnd, offsetObj, destinoSal, destino;
    private NavMeshAgent agente;
    private CharacterController personajeCtr;
    private Transform objetivoTrf, ataqueCenTrf, centro;
    private EspacioPersonalEnemigo espacioPer;
    private AreaEnemiga zona;
    private int indicePnt;
    private Animator animador;
    private GameObject rebotador, jugador;
    private float cooldown, acercarseDst, centroY, saltoDst, distanciaPntSal, gravedad, cambioY, personajeCtrRad, revisarTmp, cercanoDst;
    private EspaldaEnemigo espalda;

    
    // Inicialización de variables.
    private void Start ()
    {
        jugadorCap = LayerMask.GetMask (new string[] { "Violeta", "Abedul" });
        perseguir = false;
        reposicionado = false;
        alcanzadoObj = false;
        parado = false;
        atacado = false;
        posicionIni = this.transform.position;
        agente = this.GetComponent<NavMeshAgent> ();
        agente.updatePosition = false;
        agente.updateRotation = false;
        agente.destination = posicionIni;
        personajeCtr = this.GetComponent<CharacterController> ();
        objetivoTrf = null;
        ataqueCenTrf = this.transform.GetChild (1);
        centro = this.transform.GetChild (3);
        espacioPer = this.GetComponentInChildren<EspacioPersonalEnemigo> ();
        zona = this.transform.parent.GetComponent<AreaEnemiga> ();
        indicePnt = -1;
        animador = this.GetComponentInChildren<Animator> ();
        rebotador = this.transform.GetChild(0).gameObject;
        jugador = GameObject.FindGameObjectWithTag ("Jugador");
        acercarseDst = jugador.GetComponent<SphereCollider>().radius * jugador.transform.localScale.x * 3;
        centroY = centro.position.y;
        saltoDst = saltoRayDst - this.transform.localScale.x * personajeCtr.radius;
        gravedad = -10;
        personajeCtrRad = this.transform.localScale.x * personajeCtr.radius;
        revisarTmp = 0.5f / zona.vivos * prioridad;
        cercanoDst = jugador.GetComponent<SphereCollider>().radius * jugador.transform.localScale.x + personajeCtrRad;
        espalda = this.GetComponentInChildren<EspaldaEnemigo> ();

        this.InvokeRepeating ("RutaLibre", revisarTmp, 0.5f);
    }


    // Si hay que perseguir a alguien y el enemigo está activo, nos dirigimos directamente al objetivo en caso de que este no esté por encima de nosotros; de lo contrario, el enemigo se mueve de forma aleatoria para tratar de evitar que el jugador
    //caiga sobre él y le haga daño, la nueva posición se definirá cada cierto tiempo.
    private void Update ()
    {
        cooldown -= Time.deltaTime;
        alcanzadoObj = acercarse == false ? Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (agente.destination.x, agente.destination.z)) < distanciaMaxObj :
            espacioPer.companyerosCer.Count != 0 || Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (objetivoTrf.position.x, objetivoTrf.position.z)) < acercarseDst;

        if (perseguir == true)
        {
            this.StopCoroutine ("VolverAPosicionInicial");
            if (avatarTrf.name == "Abedul" || objetivoTrf.position.y < centroY)
            {
                destino = objetivoTrf.position + offsetObj;

                MoverAgenteYControlador (destino, true, true);
                //print (this.name + ": siguiendo a " + objetivoTrf.parent.name + " yendo al punto " + agente.destination);
                if (this.IsInvoking ("Reposicionando") == true)
                {
                    this.CancelInvoke ("Reposicionado");
                }
                MirarSiAtaco ();
            }
            else
            {
                if (this.IsInvoking ("Reposicionado") == false)
                {
                    //print (this.name + ": mi nueva posición es " + destinoRnd);

                    this.Invoke ("Reposicionado", Random.Range (1f, 2f));
                }

                //print (this.name + ": evitando a " + objetivoTrf.parent.name + " yendo al punto " + destinoRnd);
                MoverAgenteYControlador (destino);
                /*if (reposicionado == false)
                {
                    if (this.IsInvoking ("Reposicionado") == false)
                    {
                        destinoRnd = new Vector3 (avatarTrf.position.x + Random.Range (-aleatoriedad, +aleatoriedad), this.transform.position.y, avatarTrf.position.z + Random.Range (-aleatoriedad, +aleatoriedad));

                        print (this.name + ": nueva invocación.");
                        this.Invoke ("Reposicionado", Random.Range (1f, 2f));
                    }
                }
                else
                {
                    reposicionado = false;
                }*/
            }
        }
        else
        {
            //print (this.name + ": volviendo a la posición inicial, en el punto " + posicionIni);
            if (alcanzadoObj == false && puntosGol >= 0)
            {
                this.StartCoroutine ("VolverAPosicionInicial");
            }
        }

        Animar ();
    }


    // Si el enemigo ha muerto, lo acercamos lentamente al suelo para que cuadre con la animación de muerte.
    private void FixedUpdate ()
    {
        AplicarGravedad ();
    }


    // Puede servir para ver que hacer cuando no podemos llegar al punto.
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if (saltando == true && espalda.obstaculos.Contains (hit.collider) == true) 
        {
            //print (this.name + ": encontré el obstáculo " + hit.transform.name + " mientras saltaba.");
            saltando = false;
        }
        if (hit.transform.CompareTag ("Enemigo") == true || hit.transform.CompareTag ("Jugador") == true) 
        {
            Transform tocadoTrf = hit.transform;
            Vector3 movimiento = Mathf.Abs (Vector2.Angle (new Vector2 (hit.moveDirection.x, hit.moveDirection.z), new Vector2 (tocadoTrf.forward.x, tocadoTrf.forward.z)) - 90) < Mathf.Abs (Vector2.Angle 
                (new Vector2 (hit.moveDirection.x, hit.moveDirection.z), new Vector2 (tocadoTrf.right.x, tocadoTrf.right.z)) - 90) ? tocadoTrf.forward : tocadoTrf.right;

            tocadoTrf.Translate (movimiento);
            //print (this.name + ": me choqué con " + tocadoTrf.name + " mientras me movía. Le apliqué una fuerza de " + movimiento + " para apartarle (magnitud: " + movimiento.magnitude + ").");
        }
    }


    // Si el enemigo entra en la esfera que cubre al jugador que es su blanco, consideramos que se encuentra cerca del mismo.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true && other.transform == avatarTrf)
        {
            cercanoAvt = true;
            cooldown = Random.Range (1.15f, 1.35f);
        }
    }


    // Si el enemigo sale de la esfera que cubre al jugador que es us blanco, consideramos que se encuentra lejos del mismo.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true && other.transform == avatarTrf)
        {
            cercanoAvt = false;
        }
    }


    // Para marcar el objetivo de los ranoncios.
    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;

        /*if (personajeCtr != null && this.name == "Enemigo prototipo (1)") 
        {
            Gizmos.DrawWireSphere (capsulaIni, this.transform.localScale.x * personajeCtr.radius);
            Gizmos.DrawWireSphere (capsulaFin, this.transform.localScale.x * personajeCtr.radius);
        }*/
        if (agente != null) 
        {
            Gizmos.DrawWireSphere (agente.destination, 1);
        }
        /*if (ataqueCenTrf != null) 
        {
            Gizmos.DrawWireSphere (ataqueCenTrf.position, rangoAtq);
        }*/
        /*if (centro != null) 
        {
            Gizmos.DrawRay (centro.position, -this.transform.forward * saltoDst);
        }*/
        //Gizmos.DrawRay (this.transform.position,  -this.transform.forward * saltoDst);
        //Gizmos.DrawWireCube (this.transform.position, new Vector3 (300, 0, 300));
    }


    // Se indica al enemigo que tiene que perseguir/atacar a alguien.
    public void AtacarA (Transform jugador, bool uno) 
    {
        int indiceArr;
        float distanciaChc;

        float distanciaMin = float.MaxValue;

        perseguir = true;
        parado = false;
        avatarTrf = jugador;
        cercanoAvt = Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (avatarTrf.position.x, avatarTrf.position.z)) < cercanoDst;
        objetivoTrf = jugador.GetChild (3);
        offsetObj = Vector3.zero;
        objetivo1 = uno;
        for (int p = 0; p < objetivoTrf.childCount; p += 1)
        {
            indiceArr = objetivo1 == false ? p : p + zona.tomadosPnt.Length / 2;
            distanciaChc = Vector3.Distance (objetivoTrf.GetChild(p).position, this.transform.position);
            if (zona.tomadosPnt[indiceArr] == false && distanciaChc < distanciaMin)
            {
                distanciaMin = distanciaChc;
                offsetObj = objetivoTrf.GetChild(p).position - objetivoTrf.position;
                indicePnt = indiceArr;
            }
        }
        if (indicePnt != -1)
        {
            acercarse = false;
            zona.tomadosPnt[indicePnt] = true;
        }
        else 
        {
            acercarse = true;
        }
    }


    // El enemigo deja de perseguir a su objetivo y vuelve a su posición inicial, en caso de seguir activo.
    public void Parar () 
    {
        perseguir = false;
        saltando = false;
        parado = true;
        acercarse = false;
        agente.destination = posicionIni;

        this.CancelInvoke ("Saltar");
        this.CancelInvoke ("Reposicionando");
    }


    // Se recibe el valor del daño recibido, y si es un salto o no, para tener en cuenta el valor de defensa del enemigo según el tipo de ataque. Además desactivaremos al enemigo en caso de que su salud tras el golpe sea menor de 0.
    public void Danyar (float danyo, bool salto) 
    {
        puntosGol -= salto  == true ? danyo / saltoDef : danyo / aranyazoDef;
        danyado = true;
        parado = true;
        cambioY = aturdimientoVelY;

        zona.Alerta (this.transform, avatarTrf);
        this.CancelInvoke ("Moverse");
        this.Invoke ("AcabarAturdimiento", 0.3f);
    }


    // Devuelve true o false dependiendo de si la salud del enemigo está por debajo de 0, y también lo desactiva si es el caso.
    public bool ChecarDerrotado () 
    {
        if (puntosGol >= 0) 
        {
            return false;
        }
        else
        {
            posicionIni = this.transform.position;
            perseguir = false;
            personajeCtr.enabled = false;
            this.GetComponent<Parpadeo>().enabled = false;
            this.transform.position = new Vector3 (this.transform.position.x, posicionIni.y, this.transform.position.z);

            animador.SetBool ("aturdido", false);
            animador.SetBool ("saltando", false);
            rebotador.SetActive (false);
            this.Invoke ("Desaparecer", 5);

            return true;
        }
    }


    // Simplemente devuelve true si el enemigo tiene su salud por debajo de 0.
    public bool Vencido () 
    {
        return (puntosGol < 0);
    }


    // Nos aseguramos de que el punto seguido por el enemigo vuelve a ser nulo.
    public void SinBlanco () 
    {
        if (indicePnt != -1) 
        {
            zona.tomadosPnt[indicePnt] = false;
            indicePnt = -1;
        }
    }


    // Si detectamos un obstáculo detrás, saltamos una distancia algo anterior a la del mismo; en caso contrario, saltamos la distancia de salto por defecto.
    public void Saltar () 
    {
        RaycastHit infoRay;

        if (Physics.Raycast (centro.position, -this.transform.forward, out infoRay, saltoRayDst, capasSal, QueryTriggerInteraction.Ignore) == true)
        {
            if (infoRay.distance > personajeCtrRad * 3)
            {
                destinoSal = this.transform.position - this.transform.forward * (infoRay.distance - this.transform.localScale.x * personajeCtr.radius);
                saltando = true;
                cambioY = infoRay.distance * saltoVel / saltoDst;
                //print (this.name + ": encontré el obstáculo " + infoRay.transform.name + " antes de saltar.");
            }
            /*else 
            {
                print (this.name + ": ¿pasa esto siquiera?");
            }*/
        }
        else 
        {
            destinoSal = this.transform.position - this.transform.forward * saltoDst;
            saltando = true;
            cambioY = saltoVel;
        }
        parado = false;
        destinoSal.y = posicionIni.y;
        distanciaPntSal = Vector3.Distance (this.transform.position, destinoSal);
    }


    // Devuelve una distancia de salto adecuada respecto al collider del enemigo para que Violeta la use para decidir cuando saltar si su IA se enfrenta al enemigo.
    public float ObtenerDistanciaDeSaltoOptima () 
    {
        return (personajeCtr.bounds.extents.x * 1.5f);
    }


    // Devuelve una distancia respecto al collider del enemigo que sea adecuada para que Abedul pueda decidir cuando arañar al mismo.
    public float ObtenerDistanciaDeAranyazoOptima (Bounds limites)
    {
        return (personajeCtr.bounds.extents.x + limites.extents.x * 1.2f);
    }


    // Tras cierto tiempo, marcamos como verdadero que el enemigo se ha reposicionado para poder hacerlo de nuevo.
    private void Reposicionado () 
    {
        destino = new Vector3 (avatarTrf.position.x + Random.Range (-aleatoriedad, +aleatoriedad), this.transform.position.y, avatarTrf.position.z + Random.Range (-aleatoriedad, +aleatoriedad));
    }


    // Si el nuevo objetivo es distinto al anteriormente asignado y no estamos cerca del jugador.
    private void MoverAgenteYControlador (Vector3 objetivo, bool mirarObj = false, bool importaCer = false)
    {
        if (parado == false && esperar == false) 
        {
            if (saltando == false)
            {
                if (danyado == false)
                {
                    objetivo.y = this.transform.position.y;
                    //print ("Objetivo real: " + objetivo);

                    if ((importaCer == false || cercanoAvt == false) && agente.destination != objetivo)
                    {
                        agente.SetDestination (objetivo);
                        //print ("Objetivo en agente: " + agente.destination);
                    }

                    if (alcanzadoObj == false)
                    {
                        /*if (destino == destinoRnd) 
                        {
                            print (this.name + ": ACÁ ESTAMOS REALIZANDO EL MOVIMIENTO.");
                        }*/
                        personajeCtr.Move (agente.desiredVelocity.normalized * Time.deltaTime * velocidadMov);

                        agente.velocity = personajeCtr.velocity;
                        if (agente.velocity != Vector3.zero)
                        {
                            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.eulerAngles.x, Mathf.Atan2 (personajeCtr.velocity.x, personajeCtr.velocity.z) * Mathf.Rad2Deg,
                                this.transform.rotation.eulerAngles.z), Time.deltaTime * velocidadRot);
                        }
                    }
                }
            }
            else
            {
                if (agente.destination != destinoSal)
                {
                    agente.SetDestination (destinoSal);
                }
                personajeCtr.Move (agente.desiredVelocity.normalized * Time.deltaTime * Vector3.Distance (this.transform.position, destinoSal) * velocidadMov * 2 / distanciaPntSal);

                agente.velocity = personajeCtr.velocity;
                if (Vector3.Distance (this.transform.position, destinoSal) < pararSalDst)
                {
                    saltando = false;
                    parado = true;

                    this.Invoke ("Moverse", 1.5f);
                }
            }

            agente.nextPosition = this.transform.position;
        }
        if (mirarObj == true && (alcanzadoObj == true || esperar == true))
        {
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.eulerAngles.x, Quaternion.LookRotation(this.transform.position - avatarTrf.position).eulerAngles.y + 180,
                this.transform.rotation.eulerAngles.z), Time.deltaTime * velocidadRot);
        }
    }


    // Si la salud del enemigo está por debajo de 0, hacemos que reproduzca su animación de muerte, en caso contrario, hacemos que reproduzca su animación de correr o estar parado según si se está desplazando o no.
    private void Animar () 
    {
        if (puntosGol < 0) 
        {
            animador.SetTrigger ("derrotado");
        }
        else 
        {
            animador.SetBool ("moviendose", alcanzadoObj == false && parado == false && esperar == false && agente.velocity != Vector3.zero);
            animador.SetBool ("aturdido", danyado);
            animador.SetBool ("saltando", saltando);
            if (cercanoAvt == true && danyado == false && saltando == false && cooldown < 0 && objetivoTrf.position.y < centroY) 
            {
                cooldown = Random.Range (1.15f, 1.35f);

                animador.SetTrigger ("atacado");
            }
        }
    }


    // Tras completar su animación de muerte, desactivamos el objeto que representa al enemigo.
    private void Desaparecer () 
    {
        zona.LiberarRanuras (avatarTrf);
        if (avatarTrf.name == "Abedul" || objetivoTrf.position.y < centroY)
        {
            zona.APor (avatarTrf, objetivo1);
        }
        this.gameObject.SetActive (false);

        zona.vivos -= 1;
    }


    // Si se está realizando la animación de ataque, todavía no se ha causado daño a nadie durante la reproducción de esta, y se ha realizado ya más de 55% de la misma, todos los avatares dentro de un cierto rango reciben daños.
    private void MirarSiAtaco () 
    {
        if (animador.GetCurrentAnimatorStateInfo(0).IsTag ("Ataque") == true)
        {
            if (atacado == false && animador.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.55f)
            {
                Collider[] avataresCol = Physics.OverlapSphere (ataqueCenTrf.position, rangoAtq, jugadorCap, QueryTriggerInteraction.Ignore);

                foreach (Collider a in avataresCol)
                {
                    a.GetComponent<Salud>().RecibirDanyo (Vector3.zero);
                }

                atacado = true;
            }
        }
        else 
        {
            atacado = false;
        }
    }


    // Para desactivar la animación de aturdimiento tras un tiempo.
    private void AcabarAturdimiento () 
    {
        danyado = false;
        cooldown = 2;

        if (puntosGol >= 0) 
        {
            this.Invoke ("Saltar", 0.4f);
        }
    }


    // Para que vuelvan a moverse un tiempo después de haber saltado.
    private void Moverse () 
    {
        parado = false;
    }


    // Hacemos que la gravedad afecte a las ranas siempre y cuando sus coordenadas en Y estén por encima de un cierto valor.
    private void AplicarGravedad () 
    {
        if (this.transform.position.y > limiteY) 
        {
            this.transform.Translate (new Vector3 (0, cambioY, 0) * Time.deltaTime);
        }

        if (puntosGol >= 0 && this.transform.position.y <= posicionIni.y)
        {
            cambioY = 0;
            this.transform.position = new Vector3 (this.transform.position.x, posicionIni.y, this.transform.position.z);
        }
        else
        {
            cambioY += gravedad;
        }
    }


    // El enemigo tendrá que esperar si su destino o el camino hacia el mismo contiene algún obstáculo.
    private void RutaLibre () 
    {
        if (perseguir == true && acercarse == false && alcanzadoObj == false && parado == false && danyado == false && saltando == false && puntosGol >= 0)
        {
            Vector3 esferaCen1 = new Vector3 (destino.x, personajeCtr.bounds.center.y + this.transform.localScale.y * (personajeCtr.height / 2 - personajeCtr.radius), destino.z);
            Vector3 esferaCen2 = new Vector3 (esferaCen1.x, esferaCen1.y - this.transform.localScale.y * (personajeCtr.height - personajeCtr.radius * 2), esferaCen1.z);

            if (Physics.CheckCapsule (esferaCen1, esferaCen2, personajeCtrRad, capsulaCap, QueryTriggerInteraction.Ignore) == true || Physics.Linecast (centro.position, new Vector3 (destino.x, centro.position.y, destino.z), 
                rayoCap, QueryTriggerInteraction.Ignore) == true)
            {
                esperar = true;
            }
            else 
            {
                esperar = false;
            }
        }
        else 
        {
            esperar = false;
        }
    }


    // Esta corutina se llamará repetidamente cuando no haya avatares en la zona y hará que tras 2 segundos los enemigos se acerquen, frame tras frame, a su posición inicial, una vez se llegue a la misma descartaremos todas las llamadas que haya en
    //curso relativas a esta misma corutina.
    private IEnumerator VolverAPosicionInicial () 
    {
        yield return new WaitForSeconds (1.5f);

        parado = false;

        MoverAgenteYControlador (posicionIni);
        if (Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (posicionIni.x, posicionIni.z)) < distanciaMaxObj) 
        {
            this.StopAllCoroutines ();
        }
    }
}