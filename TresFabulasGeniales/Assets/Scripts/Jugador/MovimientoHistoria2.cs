
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;



public class MovimientoHistoria2 : MonoBehaviour
{
    public bool input, sueleado, perseguir, descansar;
    public Vector3 movimiento;
    public int saltoVel;
    public List<Transform> huesos;
    [HideInInspector] public float offsetY, offsetXZ;

    [SerializeField] private int movimientoVelNor, movimientoVelRed, rotacionVel, saltoDst, aleatoriedad;
    [SerializeField] private LayerMask capas, capasSinAvt;
    [SerializeField] private MovimientoHistoria2 companyeroMov;
    [SerializeField] private float pararDstSeg, pararDstAtq, ajusteCaiDst, multiplicadorSalBaj;
    [SerializeField] private bool saltador;
    private int gravedad, movimientoVel, empujeVel;
    private bool saltarInp, yendo, empujando, limitadoX, enemigosCer, saltado, cambiando, siguiendoAcb;
    private CharacterController characterCtr;
    private float horizontalInp, verticalInp, offsetBas, sueloDst, radioRotAtq;
    private Transform camaraTrf, objetivoSeg, companyeroTrf, enemigoTrf;
    private Animator animator;
    private Vector3 empuje;
    private NavMeshAgent mallaAgtNav;
    private ObjetoMovil empujado;
    private enum Estado { normal, siguiendo, atacando };
    private Estado estado;
    private AreaEnemiga areaEng;
    private Ataque ataqueScr;
    private Salud saludScr;
    private Enemigo enemigoScr;
    private HashSet<BichoPegajoso> bichosPeg;


    // Inicialización de variables.
    private void Start ()
    {
        List<Transform> huesosEli = new List<Transform> ();

        sueleado = true;
        huesos = this.transform.GetChild(5).GetChild(0).GetComponentsInChildren<Transform>().ToList<Transform> ();
        gravedad = -11;
        movimientoVel = movimientoVelNor;
        empujeVel = movimientoVel / 3;
        yendo = false;
        empujando = false;
        enemigosCer = false;
        saltado = false;
        cambiando = false;
        siguiendoAcb = false;
        characterCtr = this.GetComponent<CharacterController> ();
        sueloDst = 1.5f;
        radioRotAtq = characterCtr.bounds.size.x * this.transform.localScale.x * 2;
        offsetY = this.transform.localScale.y * characterCtr.height;
        offsetXZ = this.transform.localScale.x * characterCtr.radius * 3;
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
        objetivoSeg = companyeroMov.transform.GetChild (1);
        companyeroTrf = companyeroMov.transform;
        animator = this.GetComponentInChildren<Animator> ();
        mallaAgtNav = this.GetComponent<NavMeshAgent> ();
        mallaAgtNav.speed = movimientoVelNor;
        offsetBas = mallaAgtNav.baseOffset;
        estado = Estado.normal;
        ataqueScr = this.GetComponent<Ataque> ();
        saludScr = this.GetComponent<Salud> ();
        bichosPeg = new HashSet<BichoPegajoso> ();

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


    // En el caso de que el input esté permitido, obtendremos el relativo al movimiento de las teclas/botones correspondiente y moveremos y animaremos al personaje en consecuencia. Se gestiona también el seguimiento del personaje no controlado en caso
    //necesario.
    private void Update ()
    {
        if (input == false)
        {
            horizontalInp = 0;
            verticalInp = 0;
            saltarInp = false;
        }
        else
        {
            horizontalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal"));
            verticalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical"));
            saltarInp = empujando == false ? Input.GetButtonDown ("Salto") : false;
        }
        movimiento.x = 0;
        movimiento.z = 0;
        sueleado = Sueleado ();
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
                DesagruparSiEso ();

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
        Transform tocado = hit.transform;

        switch (tocado.tag) 
        {
            case "Jugador":
                if (sueleado == false) 
                {
                    Vector3 centroSup = new Vector3 (tocado.position.x, tocado.position.y + offsetY, tocado.position.z);

                    if (Physics.Raycast (centroSup, tocado.right, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
                    {
                        empuje = tocado.right;

                        return;
                    }

                    if (Physics.Raycast (centroSup, -tocado.right, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
                    {
                        empuje = -tocado.right;

                        return;
                    }

                    if (Physics.Raycast (centroSup, tocado.forward, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
                    {
                        empuje = tocado.forward;

                        return;
                    }

                    empuje = -tocado.forward;
                }

                break;
            default:
                empuje = Vector3.zero;

                break;
        }
    }


    // Pal debug y tal.
    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawRay (this.transform.position, -Vector3.up * sueloDst);
    }


    // Independiente de si el jugador o el compañero ha entrado en la zona peligrosa, el compañero empieza a atacar si estaba siguiendo al jugador.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("AreaEnemiga") == true) 
        {
            enemigosCer = true;
            areaEng = other.GetComponent<AreaEnemiga> ();

            ComenzarAtaque ();
        }
    }


    // Pone "sueleado" a "true" para evitar que se reproduzca la animación de estar en el aire cuando realmente no lo está.
    /*private void OnTriggerStay (Collider other)
    {
        if (other.CompareTag ("Sueleador") == true)
        {
            sueleado = true;
        }
    }*/


    // Si el avatar ha salido de una zona con enemigos, desactivamos el booleano que indica si hay enemigos cerca.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("AreaEnemiga") == true) 
        {
            enemigosCer = false;
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
            mallaAgtNav.stoppingDistance = pararDstAtq;
            CambioDePersonajesYAgrupacion.instancia.juntos = false;

            //if (saltador == true)
            //{
                this.Invoke ("PosicionEnemigoCercano", 0.1f);
            /*}
            else 
            {
                this.Invoke ("PosicionEnemigoLejano", 0.1f);
            }*/
        }
    }


    // Si el combate ha terminado (se han eliminado todos los enemigos de la zona), el avatar vuelve al estado normal, se desactiva el NavMeshAgent y se le indica que no hay enemigos cerca.
    public void CombateTerminado () 
    {
        estado = Estado.normal;
        mallaAgtNav.enabled = false;
        enemigosCer = false;
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


    // Obtiene la posición del enemigo más lejano dentro de la zona en la que se encuentra el jugador actualmente, prefiriendo aquellos enemigos que tengan como objetivo al avatar que llama a esta función.
    public void PosicionEnemigoLejano ()
    {
        /*puntoAlt = new Vector3 (Random.Range (-aleatoriedad, +aleatoriedad) + enemigoTrf.position.x, this.transform.position.y, Random.Range (-aleatoriedad, +aleatoriedad) + enemigoTrf.position.z);
        while (Vector2.Distance (new Vector2 (enemigoTrf.position.x, enemigoTrf.position.z), new Vector2 (puntoAlt.x, puntoAlt.z)) < 
            enemigoTrf.GetComponent<CharacterController>().radius * enemigoTrf.localScale.x + this.transform.localScale.x * characterCtr.radius) 
        {
            puntoAlt = new Vector3 (Random.Range (-aleatoriedad, +aleatoriedad) + enemigoTrf.position.x, this.transform.position.y, Random.Range (-aleatoriedad, +aleatoriedad) + enemigoTrf.position.z);
        }
        evitando = true;*/
        float evaluada;

        Transform resultado = null;
        float distanciaMax = 0;

        foreach (Enemigo e in areaEng.enemigos)
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
        enemigoTrf = resultado;
        if (enemigoTrf == null)
        {
            foreach (Enemigo e in areaEng.enemigos)
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

        if (enemigoTrf != null)
        {
            enemigoScr = enemigoTrf.GetComponent<Enemigo> ();
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

        foreach (Enemigo e in areaEng.enemigos)
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
        enemigoTrf = resultado;
        if (enemigoTrf == null)
        {
            foreach (Enemigo e in areaEng.enemigos)
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

        if (enemigoTrf != null)
        {
            enemigoScr = enemigoTrf.GetComponent<Enemigo> ();
        }
        else
        {
            CombateTerminado ();
        }
    }


    // Lanzamos un raycast hacia abajo de no mucha mayor longitud que la altura del personaje para comprobar si este está tocando el suelo o no.
    private bool Sueleado ()
    {
        return Physics.Raycast (this.transform.position, -Vector3.up, sueloDst, capasSinAvt, QueryTriggerInteraction.Ignore);
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
                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.x, Mathf.Atan2 (movimiento.x, movimiento.z) * Mathf.Rad2Deg + 90, this.transform.rotation.z), 
                    rotacionVel * Time.deltaTime);
            }
            else 
            {
                relativoCam = (camaraTrf.right * horizontalInp + camaraTrf.forward * verticalInp);
                if (limitadoX == true)
                {
                    movimiento.z = relativoCam.z;
                }
                else
                {
                    movimiento.x = relativoCam.x;
                }
                movimiento = movimiento.normalized * empujeVel;
            }
        }
        movimiento += empuje * movimientoVel / 2;

        characterCtr.Move (Time.deltaTime * movimiento);
        if (empujando == true) 
        {
            empujado.Mover (movimiento); 
        }
    }


    // Es que sino flota.
    private void AplicarGravedad () 
    {
        switch (estado) 
        {
            case Estado.normal:
                if (sueleado == true && saltado == false && saludScr.aturdido == false)
                {
                    movimiento.y = -3;
                    empuje = Vector3.zero;
                }
                else 
                {
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
        
        yendo = Vector3.Distance (this.transform.position, posicionPnt) > mallaAgtNav.stoppingDistance && Physics.Raycast (rayo, diferencia.magnitude, capas, QueryTriggerInteraction.Ignore) == false;

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
            mallaAgtNav.SetDestination (posicionPnt);

            mirarDir = (posicionPnt - this.transform.position).normalized;
            objetivoRot = Quaternion.LookRotation(mirarDir).eulerAngles;
            rotacionY = Quaternion.Euler (this.transform.rotation.eulerAngles.x, objetivoRot.y + 90, this.transform.rotation.eulerAngles.z);
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionY, rotacionVel / 2 * Time.deltaTime);
        }
    }


    // Si la distancia en Y entra ambos personajes es mayor a 5, automáticamente los desagrupamos.
    private void DesagruparSiEso ()
    {
        if (Mathf.Abs (this.transform.position.y - companyeroMov.transform.position.y) > 5)
        {
            GestionarSeguimiento (false);

            CambioDePersonajesYAgrupacion.instancia.juntos = false;
        }
    }


    // Gestiona las animaciones del personaje de acuerdo a su situación actual.
    private void Animar ()
    {
        switch (estado) 
        {
            case Estado.normal:
                animator.SetBool ("moviendose", movimiento.x != 0 || movimiento.z != 0);
                animator.SetBool ("tocandoSuelo", saltado == false && (sueleado == true || cambiando == true || siguiendoAcb == true));
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
        if (perseguir == true && (mallaAgtNav.baseOffset == offsetBas || mallaAgtNav.baseOffset - offsetBas > ajusteCaiDst)) 
        {
            /*if (mallaAgtNav.baseOffset - offsetBas > ajusteCaiDst) 
            {
                print ("Ajustando a la altura de " + mallaAgtNav.baseOffset);
            }*/
            if (saludScr.aturdido == false)
            {
                if (saltador == true)
                {
                    mallaAgtNav.SetDestination (enemigoTrf.position);
                }
                else
                {
                    if (descansar == false)
                    {
                        if (Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (enemigoTrf.position.x, enemigoTrf.position.z)) > pararDstAtq)
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

                if (saltador == false || Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (enemigoTrf.position.x, enemigoTrf.position.z)) > radioRotAtq)
                {
                    this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (this.transform.rotation.eulerAngles.x,
                        Quaternion.LookRotation(this.transform.position - enemigoTrf.position).eulerAngles.y - 90, this.transform.rotation.eulerAngles.z), Time.deltaTime * rotacionVel);
                }
            }
            else 
            {
                mallaAgtNav.SetDestination (this.transform.position);
            }
        }
    }


    // En el caso de que el blanco actual haya sido vencido, buscamos a un nuevo objetivo al que atacar.
    private void MirarSiCambiarBlanco () 
    {
        if (enemigoScr.Vencido () == true) 
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
}