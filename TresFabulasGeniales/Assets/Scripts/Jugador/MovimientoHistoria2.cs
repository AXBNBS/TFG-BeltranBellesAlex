
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;



public class MovimientoHistoria2 : MonoBehaviour
{
    public bool input, sueleado, perseguir, descansar, companyeroCer, saltador, plataformaAbj;
    public Vector3 movimiento, aturdimientoImp, plataformaMov;
    public int saltoVel;
    public List<Transform> huesos;

    [SerializeField] private int movimientoVelNor, movimientoVelRed, rotacionVel, aleatoriedad, deslizVel, pendienteFrz;
    [SerializeField] private LayerMask capas, capasSinAvt;
    [SerializeField] private MovimientoHistoria2 companyeroMov;
    [SerializeField] private float pararDstSeg, ajusteCaiDst, multiplicadorSalBaj, deslizFrc, pendienteRayLon;
    private int gravedad, movimientoVel, empujeVel;
    private bool saltarInp, yendo, empujando, limitadoX, enemigosCer, saltado, cambiando, siguiendoAcb, deslizar, sueleadorToc, pendiente, empujadoFrm;
    private CharacterController characterCtr;
    private float offsetXZ, horizontalInp, verticalInp, offsetBas, radioRotAtq, radioEsfSue, saltoDst, aranyazoDst;
    private Transform camaraTrf, objetivoSeg, companyeroTrf, enemigoTrf;
    private Animator animator;
    private Vector3 empuje, normal, offsetEsfSue;
    private NavMeshAgent mallaAgtNav;
    private ObjetoMovil empujado;
    private enum Estado { normal, siguiendo, atacando };
    private Estado estado;
    private AreaEnemiga areaRan;
    private AreaNaifes areaNai;
    private Ataque ataqueScr;
    private Salud saludScr;
    private Enemigo ranaScr;
    private Naife naifeScr;
    private HashSet<BichoPegajoso> bichosPeg;
    private List<Vector3> normales;


    // Inicialización de variables.
    private void Start ()
    {
        List<Transform> huesosEli = new List<Transform> ();

        sueleado = true;
        huesos = this.transform.GetChild(6).GetChild(0).GetComponentsInChildren<Transform>().ToList<Transform> ();
        gravedad = -11;
        movimientoVel = movimientoVelNor;
        empujeVel = movimientoVelNor / 6;
        yendo = false;
        empujando = false;
        enemigosCer = false;
        saltado = false;
        cambiando = false;
        siguiendoAcb = false;
        characterCtr = this.GetComponent<CharacterController> ();
        radioRotAtq = characterCtr.bounds.size.x * this.transform.localScale.x * 2;
        radioEsfSue = this.transform.localScale.x * characterCtr.radius / 2;
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
        objetivoSeg = companyeroMov.transform.GetChild (1);
        companyeroTrf = companyeroMov.transform;
        animator = this.transform.GetChild(6).GetComponent<Animator> ();
        offsetEsfSue = Vector3.up;
        mallaAgtNav = this.GetComponent<NavMeshAgent> ();
        mallaAgtNav.speed = movimientoVelNor;
        offsetXZ = this.transform.localScale.x * characterCtr.radius * 6;
        offsetBas = mallaAgtNav.baseOffset;
        estado = Estado.normal;
        ataqueScr = this.GetComponent<Ataque> ();
        saludScr = this.GetComponent<Salud> ();
        bichosPeg = new HashSet<BichoPegajoso> ();
        normales = new List<Vector3> ();
        //this.transform.position = Vector3.zero;

        huesos.RemoveAt (0);
        foreach (Transform h in huesos) 
        {
            if (h.CompareTag ("HuesoRestringido") == true) 
            {
                huesosEli.Add (h);
            }
        }
        foreach (Transform e in huesosEli) 
        {
            huesos.Remove (e);
        }
    }


    // En el caso de que el input esté permitido, obtendremos el relativo al movimiento de las teclas/botones correspondiente y moveremos y animaremos al personaje en consecuencia. Se gestiona también el seguimiento del personaje no controlado en 
    //caso necesario.
    private void Update ()
    {
        //print (this.name + ": " + input);
        if (input == false || saludScr.aturdido == true)
        {
            horizontalInp = 0;
            verticalInp = 0;
            saltarInp = false;
        }
        else
        {
            horizontalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal"));
            verticalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical"));
            saltarInp = (empujando == false && deslizar == false) ? Input.GetButtonDown ("Salto") : false;
        }
        movimiento.x = 0;
        movimiento.z = 0;
        sueleado = Sueleado ();
        //print (this.name + "-- sueleado es: " + sueleado + " y mi movimiento en Y es " + movimiento.y);
        pendiente = EnPendiente ();
        if (mallaAgtNav.enabled == false) 
        {
            mallaAgtNav.baseOffset = offsetBas;
        }
        //print (this.name + ": " + mallaAgtNav.destination);
        //print (this.name + ": " + puntoAlt);

        switch (estado) 
        {
            case Estado.normal:
                if (saltarInp == true && sueleado == true)
                {
                    SaltarNormal ();
                }
                Mover (); 

                break;
            case Estado.siguiendo:
                Seguir ();
                //DesagruparSiEso ();

                break;
            default:
                if (enemigoTrf != null)
                {
                    if (saltador == true)
                    {
                        SaltarIA ();
                    }
                    IrHaciaEnemigo ();
                    MirarSiCambiarBlanco ();
                }

                break;
        }

        Animar ();
    }


    // Aplicamos la gravedad al personaje si este está en el aire.
    private void FixedUpdate ()
    {
        AplicarGravedad ();
    }


    // Si el avatar jugable ha caído sobre otro, empujarlo hacia el primer lado de este que se encuentre libre.
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        //print (hit.transform.name);
        switch (hit.transform.tag) 
        {
            case "Jugador":
            case "Resbaladizo":
            case "Hablable":
                if (sueleado == false && empujadoFrm == false) 
                {
                    //print ("Wenas jeje");
                    empujadoFrm = true;
                    if (Physics.Raycast (hit.point, Vector3.right, out RaycastHit datosRay1, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
                    {
                        empuje = Vector3.right;

                        return;
                    }

                    if (Physics.Raycast (hit.point, Vector3.left, out RaycastHit datosRay2, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
                    {
                        empuje = Vector3.left;

                        return;
                    }

                    if (Physics.Raycast (hit.point, Vector3.forward, out RaycastHit datosRay3, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
                    {
                        empuje = Vector3.forward;

                        return;
                    }

                    if (Physics.Raycast (hit.point, Vector3.back, out RaycastHit datosRay4, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false) 
                    {
                        empuje = Vector3.back;

                        return;
                    }

                    RaycastHit[] datosRay = new RaycastHit[] { datosRay1, datosRay2, datosRay3, datosRay4 };
                    float mayorDst = 0;
                    int mejor = 0;

                    for (int d = 0; d < datosRay.Length; d += 1) 
                    {
                        if (datosRay[d].distance > mayorDst) 
                        {
                            mayorDst = datosRay[d].distance;
                            mejor = d;
                        }
                    }
                    switch (mejor) 
                    {
                        case 0:
                            empuje = Vector3.right;

                            break;
                        case 1:
                            empuje = Vector3.left;

                            break;
                        case 2:
                            empuje = Vector3.forward;

                            break;
                        default:
                            empuje = Vector3.back;

                            break;
                    }
                }

                break;
            default:
                empuje = Vector3.zero;

                normales.Add (hit.normal);

                break;
        }
    }


    // Pal debug y tal.
    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere (this.transform.position + offsetEsfSue, radioEsfSue);
        //Gizmos.DrawRay (this.transform.position + Vector3.up, Vector3.down * pendienteRayLon);
    }


    // Independiente de si el jugador o el compañero ha entrado en la zona peligrosa, el compañero empieza a atacar si estaba siguiendo al jugador.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("AreaEnemiga") == true) 
        {
            enemigosCer = true;
            if (other.TryGetComponent (out areaNai) == false) 
            {
                areaRan = other.GetComponent<AreaEnemiga> ();
            }

            ComenzarAtaque ();
        }
    }


    // Pone "sueleado" a "true" para evitar que se reproduzca la animación de estar en el aire cuando realmente no lo está.
    private void OnTriggerStay (Collider other)
    {
        if (other.CompareTag ("Sueleador") == true)
        {
            sueleadorToc = true;
        }
    }


    // Si el avatar ha salido de una zona con enemigos, desactivamos el booleano que indica si hay enemigos cerca. También miramos si ha salido de un sueleador para desactivar el booleano que lo indica.
    private void OnTriggerExit (Collider other)
    {
        switch (other.tag) 
        {
            case "Sueleador":
                sueleadorToc = false;

                break;
            case "AreaEnemiga":
                enemigosCer = false;
                //enemigoTrf = null;

                //TratarDeVolver ();

                break;
        } 
    }


    // Se pone "sueleado" a "true" durante 0.1 segundos ya que si no puede detectar que está en el aire durante un momento y reproducir la animación equivocada.
    public void GestionarCambio ()
    {
        cambiando = true;

        this.Invoke ("FinalizarCambio", 0.1f);
    }


    // Devuelve "true" si el personaje en cuestión está en el aire.
    public bool EstaEnElAire ()
    {
        return (animator.GetCurrentAnimatorStateInfo(0).IsTag ("Aire"));
    }


    // Hacemos que la variable que hace que se siga al compañero sea verdadera y activamos el "NavMeshAgent" para poder seguir al otro personaje.
    public void GestionarSeguimiento (bool comenzar)
    {
        mallaAgtNav.enabled = comenzar;
        estado = comenzar == true ? Estado.siguiendo : Estado.normal;
        if (enemigosCer == false)
        {
            mallaAgtNav.stoppingDistance = pararDstSeg;
            CambioDePersonajesYAgrupacion.instancia.juntos = comenzar;
            if (comenzar == false)
            {
                siguiendoAcb = true;

                this.Invoke ("FinalizarSeguimiento", 0.1f);
            }
        }
    }


    // Activamos el booleano que permite el empuje, definimos en que eje se permite y recibimos el objeto sobre el cual se aplicará el empuje.
    public void ComenzarEmpuje (bool enEjeX, ObjetoMovil objeto) 
    {
        empujando = true;
        limitadoX = enEjeX;
        empujado = objeto;
    }


    // Desactivamos el booleano que nos permite empujar.
    public void PararEmpuje () 
    {
        empujando = false;
    }


    // El compañero pasa a atacar y encuentra el enemigo más cercano a elle dentro de la zona actual.
    public void ComenzarAtaque () 
    {
        if (enemigosCer == true && input == false) 
        {
            estado = Estado.atacando;
            perseguir = true;
            mallaAgtNav.enabled = true;
            CambioDePersonajesYAgrupacion.instancia.juntos = false;

            this.Invoke ("PosicionEnemigoCercano", 0.1f);
        }
    }


    // Si el combate ha terminado (se han eliminado todos los enemigos de la zona), el avatar vuelve al estado normal, se desactiva el NavMeshAgent y se le indica que no hay enemigos cerca.
    public void CombateTerminado () 
    {
        estado = Estado.normal;
        mallaAgtNav.enabled = false;
        enemigosCer = false;
        areaRan = null;
        areaNai = null;
    }


    // Un bicho se pega al cuerpo del gato, pudiendo reducir la velocidad a la que este puede desplazarse.
    public void Pegado (BichoPegajoso bicho) 
    {
        bichosPeg.Add (bicho);
        //print (bichosPeg.Count);

        if (bichosPeg.Count > 3) 
        {
            movimientoVel = movimientoVelRed;
            mallaAgtNav.speed = movimientoVelRed;
        }
    }


    // Se despegan los bichos indicados del cuerpo del gato, pudiendo restaurar la velocidad a la que este puede desplazarse.
    public void Despegar (List<BichoPegajoso> bichos) 
    {
        foreach (BichoPegajoso b in bichos) 
        {
            b.SalirVolando ();
            bichosPeg.Remove (b);
        }
        
        //print (bichosPeg.Count);
        if (bichosPeg.Count < 4)
        {
            movimientoVel = movimientoVelNor;
            mallaAgtNav.speed = movimientoVelNor;
        }
    }


    // Comprobamos si tenemos alguna zona con naifes asignada y, en caso afirmativo, miramos si hay que excluir a este avatar que acabamos de pasar a controlar de la misma.
    public void ComprobacionNaifes () 
    {
        if (areaNai != null) 
        {
            areaNai.MirarSiQuitoAvatar (characterCtr);
        }
    }


    // Obtiene la posición del enemigo más lejano dentro de la zona en la que se encuentra el jugador actualmente, prefiriendo aquellos enemigos que tengan como objetivo al avatar que llama a esta función.
    public void PosicionEnemigoLejano ()
    {
        float evaluada;

        Transform resultado = null;
        float distanciaMax = 0;

        if (areaNai != null) 
        {
            if (areaNai.avataresPer.ContainsKey (this.transform) == true) 
            {
                resultado = areaNai.avataresPer[this.transform].transform;
            }
        }
        else 
        {
            foreach (Enemigo e in areaRan.enemigos)
            {
                if (e.Vencido () == false && e.avatarTrf == this.transform)
                {
                    evaluada = Vector3.Distance (e.transform.position, this.transform.position);
                    if (evaluada > distanciaMax)
                    {
                        distanciaMax = evaluada;
                        resultado = e.transform;
                    }
                }
            }
        }
        enemigoTrf = resultado;
        if (enemigoTrf == null)
        {
            if (areaNai != null)
            {
                foreach (Naife n in areaNai.naifes)  
                {
                    if (n.Vencido () == false) 
                    {
                        evaluada = Vector3.Distance (n.transform.position, this.transform.position);
                        if (evaluada > distanciaMax)
                        {
                            distanciaMax = evaluada;
                            resultado = n.transform;
                        }
                    }
                }
            }
            else
            {
                foreach (Enemigo e in areaRan.enemigos)
                {
                    if (e.Vencido () == false)
                    {
                        evaluada = Vector3.Distance (e.transform.position, this.transform.position);
                        if (evaluada > distanciaMax)
                        {
                            distanciaMax = evaluada;
                            resultado = e.transform;
                        }
                    }
                }
            }
            enemigoTrf = resultado;
        }

        if (enemigoTrf != null)
        {
            ranaScr = enemigoTrf.GetComponent<Enemigo> ();
            naifeScr = enemigoTrf.GetComponent<Naife> ();
            aranyazoDst = naifeScr != null ?  naifeScr.ObtenerDistanciaDeAranyazoOptima (characterCtr.bounds) : ranaScr.ObtenerDistanciaDeAranyazoOptima (characterCtr.bounds);
        }
        else
        {
            CombateTerminado ();
        }
    }


    // Obtiene la posición del enemigo más cercano dentro de la zona en la que se encuentra el jugador actualmente, prefiriendo aquellos enemigos que tengan como objetivo al avatar que llama a esta función.
    private void PosicionEnemigoCercano ()
    {
        float evaluada;

        Transform resultado = null;
        float distanciaMin = Mathf.Infinity;

        if (areaNai != null) 
        {
            if (areaNai.avataresPer.ContainsKey (this.transform) == true)
            {
                resultado = areaNai.avataresPer[this.transform].transform;
            }
        }
        else 
        {
            foreach (Enemigo e in areaRan.enemigos)
            {
                if (e.Vencido () == false && e.avatarTrf == this.transform)
                {
                    evaluada = Vector3.Distance (this.transform.position, e.transform.position);
                    if (evaluada < distanciaMin)
                    {
                        distanciaMin = evaluada;
                        resultado = e.transform;
                    }
                }
            }
        }
        enemigoTrf = resultado;
        if (enemigoTrf == null)
        {
            if (areaNai != null)
            {
                foreach (Naife n in areaNai.naifes)
                {
                    if (n.Vencido () == false)
                    {
                        evaluada = Vector3.Distance (this.transform.position, n.transform.position);
                        if (evaluada < distanciaMin)
                        {
                            distanciaMin = evaluada;
                            resultado = n.transform;
                        }
                    }
                }
            }
            else 
            {
                foreach (Enemigo e in areaRan.enemigos)
                {
                    if (e.Vencido () == false)
                    {
                        evaluada = Vector3.Distance (this.transform.position, e.transform.position);
                        if (evaluada < distanciaMin)
                        {
                            distanciaMin = evaluada;
                            resultado = e.transform;
                        }
                    }
                }
            }
            enemigoTrf = resultado;
        }

        if (enemigoTrf != null)
        {
            ranaScr = enemigoTrf.GetComponent<Enemigo> ();
            naifeScr = enemigoTrf.GetComponent<Naife> ();
            if (saltador == true) 
            {
                saltoDst = naifeScr != null ? naifeScr.ObtenerDistanciaDeSaltoOptima () : ranaScr.ObtenerDistanciaDeSaltoOptima ();
            }
            else 
            {
                aranyazoDst = naifeScr != null ? naifeScr.ObtenerDistanciaDeAranyazoOptima (characterCtr.bounds) : ranaScr.ObtenerDistanciaDeAranyazoOptima (characterCtr.bounds);
            }
        }
        else
        {
            CombateTerminado ();
        }
    }


    // Lanzamos un raycast hacia abajo de no mucha mayor longitud que la altura del personaje para comprobar si este está tocando el suelo o no.
    private bool Sueleado ()
    {
        switch (estado) 
        {
            case Estado.normal:
                if (plataformaAbj == false) 
                {
                    return movimiento.y < 0 ? Physics.CheckSphere (this.transform.position + offsetEsfSue, radioEsfSue, capasSinAvt, QueryTriggerInteraction.Ignore) : false;
                }
                else 
                {
                    return true;
                }
            case Estado.siguiendo:
                return true;
            default:
                return mallaAgtNav.baseOffset == offsetBas;
        }
    }


    // Si el personaje está en el suelo y se ha pulsado el botón de salto, haremos que salte.
    private void SaltarNormal ()
    {
        movimiento.y = saltoVel;
        saltado = true;

        this.Invoke ("FinImpulso", 0.1f);
    }


    // El personaje controlado por IA salta si está en el suelo y suficientemente cerca del enemigo, simulamos aplicar gravedad cambiando el "base offset" del agente.
    private void SaltarIA () 
    {
        if (saludScr.aturdido == false && mallaAgtNav.baseOffset == offsetBas && Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (enemigoTrf.position.x, enemigoTrf.position.z)) < saltoDst) 
        {
            SaltarNormal ();
        }

        mallaAgtNav.baseOffset += Time.deltaTime * movimiento.y;
        //print (movimiento.y);
        if (mallaAgtNav.baseOffset <= offsetBas) 
        {
            mallaAgtNav.baseOffset = offsetBas;
            perseguir = true;
        }
    }


    // Le aplicamos gravedad al personaje y, si además está siendo movido por el jugador, lo movemos y rotamos adecuadamente hacia la dirección del movimiento, teniendo en cuenta además la posición de la cámara para que el movimiento sea relativo a 
    //la misma. Movemos también el objeto empujado en caso de que haya alguno.
    private void Mover () 
    {
        if (horizontalInp != 0 || verticalInp != 0)
        {
            Vector3 relativoCam;

            if (empujando == false)
            {
                relativoCam = (camaraTrf.right * horizontalInp + camaraTrf.forward * verticalInp).normalized * movimientoVel;
                movimiento.x = relativoCam.x;
                movimiento.z = relativoCam.z;
                this.transform.rotation = Quaternion.Slerp (this.transform.rotation, Quaternion.Euler (0, Mathf.Atan2 (movimiento.x, movimiento.z) * Mathf.Rad2Deg + 90, 0), Time.deltaTime * rotacionVel);
            }
            else 
            {
                relativoCam = (camaraTrf.right * horizontalInp + camaraTrf.forward * verticalInp);
                if (limitadoX == true)
                {
                    movimiento.x = relativoCam.x;
                }
                else
                {
                    movimiento.z = relativoCam.z;
                }
            }
        }
        if (characterCtr.collisionFlags != CollisionFlags.None) 
        {
            movimiento += empuje * movimientoVel / 2;
        }

        if (empujando == false) 
        {
            CollisionFlags banderitas;

            if (deslizar == true)
            {
                float inputX = movimiento.x;
                float inputZ = movimiento.z;

                //print (this.name + "-> movimiento en X antes del desliz: " + movimiento.x);
                //print (this.name + "-> movimiento en Z antes del desliz: " + movimiento.z);
                //print (this.name + "-> normal usada: " + normal);
                movimiento.x = (1 - normal.y) * normal.x * (1 - deslizFrc) * deslizVel;
                movimiento.z = (1 - normal.y) * normal.z * (1 - deslizFrc) * deslizVel;
                if (Mathf.Sign (movimiento.x) == Mathf.Sign (inputX))
                {
                    movimiento.x += inputX;
                }
                if (Mathf.Sign (movimiento.z) == Mathf.Sign (inputZ))
                {
                    movimiento.z += inputZ;
                }
                //print (this.name + "-> movimiento en X después del desliz: " + movimiento.x);
                //print (this.name + "-> movimiento en Z después del desliz: " + movimiento.z);
            }
            if (saludScr.aturdido == true) 
            {
                movimiento.x = aturdimientoImp.x * movimientoVel * 2;
                movimiento.z = aturdimientoImp.z * movimientoVel * 2;
            }
            if (pendiente == false)
            {
                banderitas = characterCtr.Move (Time.deltaTime * movimiento);
                /*else 
                {
                    banderitas = characterCtr.Move (new Vector3 (movimiento.x + plataformaMov.x, plataformaMov.y, movimiento.z + plataformaMov.z) * Time.deltaTime);
                }*/
                //print (this.name + ": en terreno normal. Velocidad del controlador: " + characterCtr.velocity);
            }
            else 
            {
                banderitas = characterCtr.Move ((Vector3.down * pendienteFrz + movimiento) * Time.deltaTime);
                //print (this.name + ": tremendo motor el unidades este.");
                //print (this.name + ": en una pendiente. Velocidad del controlador: " + characterCtr.velocity);
            }
            deslizar = ObtenerMenorPendiente () > characterCtr.slopeLimit || (sueleado == false && banderitas != CollisionFlags.None && banderitas != CollisionFlags.Sides);
            //print (this.name + "-> Menor ángulo obtenido: " + ObtenerMenorPendiente ());
            //print (this.name + "-> ¿Estoy tocando el suelo? " + sueleado);
            //print (this.name + "-> ¿Estoy en una pendiente? " + pendiente);
            //print (this.name + "-> ¿Estoy deslizándome? " + deslizar);
        }
        else 
        {
            Vector3 movimientoEmp = new Vector3(movimiento.x, 0, movimiento.z).normalized * empujeVel;

            characterCtr.Move (Time.deltaTime * movimientoEmp);
            empujado.Mover (movimientoEmp); 
        }

        empujadoFrm = false;

        normales.Clear ();
    }


    // Es que sino flota.
    private void AplicarGravedad () 
    {
        switch (estado) 
        {
            case Estado.normal:
                if (sueleado == true && saltado == false && saludScr.aturdido == false)
                {
                    //if (plataformaAbj == false) 
                    //{
                        movimiento.y = -10;
                    /*}
                    else 
                    {
                        movimiento.y = 0;
                    }*/
                    empuje = Vector3.zero;
                }
                else 
                {
                    if ((characterCtr.collisionFlags & CollisionFlags.Above) != 0) 
                    {
                        movimiento.y = 0;
                    }
                    if (input == true && saltador == true && movimiento.y > 0 && Input.GetButton ("Salto") == false) 
                    {
                        movimiento.y += gravedad * multiplicadorSalBaj;
                    }
                    else 
                    {
                        movimiento.y += gravedad;
                    }
                }

                break;
            case Estado.siguiendo:
                movimiento.y = 0;

                break;
            default:
                if (mallaAgtNav.baseOffset == offsetBas)
                {
                    movimiento.y = 0;
                }
                else 
                {
                    movimiento.y += gravedad;
                }

                break;
        }
    }


    // Seguimos al personaje si no estamos lo suficientemente cerca de él y el punto a seguir no está en una zona no accesible, también rotamos mirando hacia el punto que estamos siguiendo o hacia el otro personaje según la situación.
    private void Seguir ()
    {
        Vector3 mirarDir, objetivoRot;
        Quaternion rotacionY;

        Vector3 posicionPnt = objetivoSeg.position;
        Vector3 diferencia = posicionPnt - objetivoSeg.parent.position;
        Ray rayo = new Ray (objetivoSeg.parent.position, diferencia);
        
        //yendo = Vector3.Distance (this.transform.position, posicionPnt) > mallaAgtNav.stoppingDistance && Physics.Raycast (rayo, diferencia.magnitude, capas, QueryTriggerInteraction.Ignore) == false;
        yendo = Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (posicionPnt.x, posicionPnt.z)) > mallaAgtNav.stoppingDistance && 
            Physics.Raycast (rayo, diferencia.magnitude, capas, QueryTriggerInteraction.Ignore) == false;

        if (yendo == false)
        {
            mallaAgtNav.SetDestination (this.transform.position);

            mirarDir = (companyeroTrf.position - this.transform.position).normalized;
            objetivoRot = Quaternion.LookRotation(mirarDir).eulerAngles;
            rotacionY = Quaternion.Euler (this.transform.rotation.eulerAngles.x, objetivoRot.y + 90, this.transform.rotation.eulerAngles.z);
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionY, rotacionVel * Time.deltaTime);
        }
        else 
        {
            if (mallaAgtNav.destination != posicionPnt) 
            {
                mallaAgtNav.SetDestination (posicionPnt);
            }

            mirarDir = (posicionPnt - this.transform.position).normalized;
            objetivoRot = Quaternion.LookRotation(mirarDir).eulerAngles;
            rotacionY = Quaternion.Euler (this.transform.rotation.eulerAngles.x, objetivoRot.y + 90, this.transform.rotation.eulerAngles.z);
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionY, rotacionVel / 2 * Time.deltaTime);
        }
    }


    // Si la distancia en Y entra ambos personajes es mayor a 5, automáticamente los desagrupamos.
    /*private void DesagruparSiEso ()
    {
        if (Mathf.Abs (this.transform.position.y - companyeroMov.transform.position.y) > 5)
        {
            GestionarSeguimiento (false);

            CambioDePersonajesYAgrupacion.instancia.juntos = false;
        }
    }*/


    // Gestiona las animaciones del personaje de acuerdo a su situación actual.
    private void Animar ()
    {
        switch (estado) 
        {
            case Estado.normal:
                animator.SetBool ("moviendose", movimiento.x != 0 || movimiento.z != 0);
                animator.SetBool ("tocandoSuelo", sueleado == true && deslizar == false && (saltado == false || cambiando == true || siguiendoAcb == true));
                animator.SetFloat ("velocidadY", movimiento.y);

                break;
            case Estado.siguiendo:
                animator.SetBool ("moviendose", yendo);
                animator.SetBool ("tocandoSuelo", true);

                break;
            default:
                animator.SetBool ("moviendose", saltador == true || mallaAgtNav.velocity != Vector3.zero);
                animator.SetBool ("tocandoSuelo", mallaAgtNav.baseOffset == offsetBas);
                animator.SetFloat ("velocidadY", movimiento.y);

                break;
        }
    }


    // Hacemos que la posición del enemigo a atacar sea el destino del agente.
    private void IrHaciaEnemigo ()
    {
        //(mallaAgtNav.baseOffset == offsetBas || mallaAgtNav.baseOffset - offsetBas > ajusteCaiDst)
        if (saludScr.aturdido == false)
        {
            if (perseguir == true && movimiento.y >= 0)
            {
                if (saltador == true)
                {
                    mallaAgtNav.SetDestination (enemigoTrf.position);
                }
                else
                {
                    if (descansar == false)
                    {
                        if (Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (enemigoTrf.position.x, enemigoTrf.position.z)) > aranyazoDst)
                        {
                            mallaAgtNav.SetDestination (enemigoTrf.position);
                        }
                        else
                        {
                            mallaAgtNav.SetDestination (this.transform.position);
                            ataqueScr.IniciarAtaque ();
                        }
                    }
                    else
                    {
                        mallaAgtNav.SetDestination (this.transform.position);
                    }
                }

                if (saltador == false || Vector2.Distance(new Vector2(this.transform.position.x, this.transform.position.z), new Vector2(enemigoTrf.position.x, enemigoTrf.position.z)) > radioRotAtq)
                {
                    this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.eulerAngles.x,
                        Quaternion.LookRotation(this.transform.position - enemigoTrf.position).eulerAngles.y - 90, this.transform.rotation.eulerAngles.z), Time.deltaTime * rotacionVel);
                }
            }
        }
        else
        {
            mallaAgtNav.velocity = sueleado == true ? aturdimientoImp * movimientoVel * 4 : Vector3.zero;
        }
    }


    // En el caso de que el blanco actual haya sido vencido, buscamos a un nuevo objetivo al que atacar.
    private void MirarSiCambiarBlanco () 
    {
        if (naifeScr != null ? naifeScr.Vencido () == true : ranaScr.Vencido () == true) 
        {
            PosicionEnemigoCercano ();
        }
    }


    // Usamos esta función para poner la variable a "false" tras un pequeño intervalo de tiempo.
    private void FinImpulso ()
    {
        saltado = false;
    }


    // Para evitar que reproduzca la animación de estar en el aire.
    private void FinalizarCambio ()
    {
        cambiando = false;
    }


    // Para evitar de nuevo lo de la animación.
    private void FinalizarSeguimiento () 
    {
        siguiendoAcb = false;
    }


    // Encuentra el ángulo de la menor pendiente que el personaje está pisando en ese momento a través de las normales.
    private float ObtenerMenorPendiente ()
    {
        //bool angulo0 = false;
        /*if (characterCtr.collisionFlags == CollisionFlags.None)
        {
            print (this.name + ": en el aire.");
        }
        if ((characterCtr.collisionFlags & CollisionFlags.Sides) != 0)
        {
            print (this.name + ": tocando los lados.");
        }
        if (characterCtr.collisionFlags == CollisionFlags.Sides)
        {
            print (this.name + ": tocando solamente los lados.");
        }
        if ((characterCtr.collisionFlags & CollisionFlags.Above) != 0)
        {
            print (this.name + ": tocando el techo.");
        }
        if (characterCtr.collisionFlags == CollisionFlags.Above)
        {
            print (this.name + ": tocando solamente el techo.");
        }
        if ((characterCtr.collisionFlags & CollisionFlags.Below) != 0)
        {
            print (this.name + ": tocando el suelo.");
        }
        if (characterCtr.collisionFlags == CollisionFlags.Below)
        {
            print (this.name + ": tocando solamente el suelo.");
        }*/
        /*if (pendiente == false && characterCtr.collisionFlags != CollisionFlags.Below)
        {
            print (this.name + "-- Devuelvo 0 porque no detecto estar en pendiente y mis banderitas de colisión son distintas a sólo abajo.");
            return 0;
        }
        else 
        {*/
            if (sueleado == true)
            {
                List<Vector3> ignorar = new List<Vector3> ();
                foreach (Vector3 v in normales)
                {
                    if (Mathf.Abs (Vector3.Angle (Vector3.up, v) - 90) < 1)
                    {
                        //print (this.name + "-- Eliminando " + v + ".");
                        ignorar.Add (v);
                    }
                }
                foreach (Vector3 v in ignorar)
                {
                    normales.Remove (v);
                }
            }

            if (normales.Count != 0)
            {
                float angulo;

                float inclinacion = 90;

                foreach (Vector3 n in normales)
                {
                    angulo = Vector3.Angle (Vector3.up, n);
                    //print (this.name + "-- " + n);
                    if (angulo < inclinacion)
                    {
                        inclinacion = angulo;
                        normal = n;
                    }
                }
                //print (this.name + "-- Devuelvo " + inclinacion + " ángulo que forma la normal menos inclinada que he encontrado con Vector3.up.");

                return inclinacion;
            }
            else
            {
                //print (this.name + "-- No hay normales.");
                return 0;
            }
        //}
    }


    // Determina si el personaje se encuentra en una pendiente o no.
    private bool EnPendiente () 
    {
        if (movimiento.y > 0)
        {
            //print (this.name + "-- no estoy en pendiente.");
            return false;
        }

        if (Physics.Raycast (this.transform.position + Vector3.up, Vector3.down, out RaycastHit datosRay, pendienteRayLon, capasSinAvt, QueryTriggerInteraction.Ignore) == true)
        {
            normales.Add (datosRay.normal);
            //print (this.name + "-- inclinación de la pendiente: " + Vector3.Angle (datosRay.normal, Vector3.up));

            return datosRay.normal != Vector3.up;
        }
        else 
        {
            //print (this.name + "-- no estoy en pendiente.");
            return false;
        }
    }


    // Si el personaje sigue en estado.
    private void TratarDeVolver () 
    {
        if (mallaAgtNav.enabled == true && Estado.atacando == estado) 
        {
            mallaAgtNav.SetDestination (areaNai.GetComponent<SphereCollider>().bounds.center);
        }
    }
}