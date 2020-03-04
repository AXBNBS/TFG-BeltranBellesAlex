
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;



public class MovimientoHistoria3 : MonoBehaviour
{
    public bool input, movimientoXEsc, movimientoXBal, escalarPos;
    public Balanceo balanceo;
    public Vector3 enganchePnt;
    public float limiteBal, limiteEsc1, limiteEsc2;
    public Quaternion rotacionEsc;
    public Transform bolaSup;

    [SerializeField] private int movimientoVel, rotacionVel, saltoVel, escaladaVel, gravedad;
    [SerializeField] private LayerMask capas;
    private Camera camara;
    private Vector3 direccionMov, previaPos, impulsoBal, impulsoCai, impulsoPar;
    private CharacterController characterCtr;
    private float sueloDst, offsetBas;
    private int verticalInp, horizontalInp;
    private bool saltoInp, engancharseInp, saltado, impulsoMnt;
    private LineRenderer renderizadorLin;
    private Quaternion[] rotacionesBal;
    private Quaternion rotacionBal;
    private enum Estado { normal, trepando, rodando, balanceandose };
    private Estado estado;
    private NavMeshAgent agente;


    // Inicialización de variables.
    private void Start ()
    {
        escalarPos = false;
        camara = GameObject.FindGameObjectWithTag("CamaraPrincipal").GetComponent<Camera> ();
        direccionMov = Vector3.zero;
        characterCtr = this.GetComponent<CharacterController> ();
        //characterCtr.velocity ;
        previaPos = this.transform.localPosition;
        sueloDst = characterCtr.height / 2 + 0.1f;
        saltado = false;
        renderizadorLin = this.GetComponent<LineRenderer> ();
        rotacionesBal = new Quaternion[] { Quaternion.Euler (0, 0, 0), Quaternion.Euler (0, 90, 0), Quaternion.Euler (0, 180, 0), Quaternion.Euler (0, 270, 0) };
        estado = Estado.normal;
        agente = this.GetComponent<NavMeshAgent> ();
        offsetBas = agente.baseOffset;

        agente.SetDestination (new Vector3 (0, 0, -50));
    }


    // Determinamos el estado actual en el que se encuentra el personaje y actuamos en consecuencia, haciendo que siga caminando (afectado también por la gravedad), trepando, desplazándose con una bola o balanceándose.
    private void Update ()
    {
        direccionMov.x = 0;
        direccionMov.z = 0;
        if (input == true)
        {
            verticalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical"));
            horizontalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal"));
            saltoInp = estado != Estado.balanceandose ? Input.GetButtonDown ("Salto") : false;
            engancharseInp = estado != Estado.trepando && estado != Estado.rodando ? Mathf.RoundToInt (Input.GetAxisRaw ("Engancharse")) != 0 : false;
        }
        else 
        {
            verticalInp = 0;
            horizontalInp = 0;
            saltoInp = false;
            engancharseInp = false;
        }

        DeterminarEstado ();
        switch (estado)
        {
            case Estado.normal:
                SaltarSuelo ();
                Caminar ();

                break;
            case Estado.trepando:
                SaltarPared ();
                Trepar ();

                break;
            case Estado.rodando:
                MantenerEquilibrio ();

                break;
            default:
                Balancear ();
                CambiarPosicionCuerda ();

                break;
        }

        agente.baseOffset += Time.deltaTime * direccionMov.y;
        if (agente.baseOffset <= offsetBas) 
        {
            agente.baseOffset = offsetBas;
        }
        if (impulsoMnt == false) 
        {
            impulsoBal = Vector3.zero;
        }
        impulsoMnt = false;
        previaPos = this.transform.localPosition;
    }


    // Aplicamos la gravedad a intervalos de tiempo fijos para que la velocidad del PC no afecte a la jugabilidad.
    private void FixedUpdate ()
    {
        AplicarGravedad ();
    }


    // Si golpeamos el objeto que provoca que reaparezcamos, la escena es reiniciada.
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if (hit.transform.CompareTag ("Reaparecer") == true)
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
        }
    }


    // A ver si debugueamos.
    /*private void OnDrawGizmos ()
    {
        Gizmos.DrawRay (this.transform.position, -Vector3.up * sueloDst);   
    }*/


    // Lanzamos un raycast hacia abajo de no mucha mayor longitud que la altura del personaje para comprobar si este está tocando el suelo o no.
    private bool Sueleado ()
    {
        return saltado == false ? Physics.Raycast (this.transform.position, -Vector3.up, sueloDst) : false;
    }


    // Si el personaje no está balanceándose pero está por debajo del enganche, dentro de un cierto radio respecto a él, se está pulsando el botón de balancearse y no hay obstáculos entre él y el punto, pasamos a columpiarnos. En cambio, si sí está
    //enganchado y el botón deja de pulsarse, el personaje empieza a caer teniendo en cuenta su inercia tras el balanceo.
    private void DeterminarEstado ()
    {
        switch (estado) 
        {
            case Estado.normal:
                if (escalarPos == true && Sueleado () == false) 
                {
                    estado = Estado.trepando;
                    impulsoCai = Vector3.zero;

                    break;
                }

                if (bolaSup != null) 
                {
                    estado = Estado.rodando;
                    this.transform.parent = bolaSup;

                    break;
                }

                if (engancharseInp == true && this.transform.position.y < limiteBal && enganchePnt != Vector3.zero && Sueleado () == false &&
                    Physics.Raycast (this.transform.position, enganchePnt - this.transform.position, 15, capas, QueryTriggerInteraction.Ignore) == false) 
                {
                    estado = Estado.balanceandose;
                    balanceo.twii.velocidad = impulsoBal * 2;
                    renderizadorLin.enabled = true;
                    if (movimientoXBal == true)
                    {
                        rotacionBal = Quaternion.Angle (this.transform.rotation, rotacionesBal[1]) < Quaternion.Angle (this.transform.rotation, rotacionesBal[3]) ? rotacionesBal[1] : rotacionesBal[3];
                    }
                    else
                    {
                        rotacionBal = Quaternion.Angle (this.transform.rotation, rotacionesBal[0]) < Quaternion.Angle (this.transform.rotation, rotacionesBal[2]) ? rotacionesBal[0] : rotacionesBal[2];
                    }

                    balanceo.CambiarEnganche (enganchePnt);

                    break;
                }

                break;
            case Estado.trepando:
                if (escalarPos == false) 
                {
                    estado = Estado.normal;
                    direccionMov.y = impulsoPar.y;
                }

                break;
            case Estado.rodando:
                if (saltoInp == true) 
                {
                    estado = Estado.normal;
                    this.transform.parent = null;
                    bolaSup = null;
                    direccionMov.y = saltoVel;
                }

                break;
            default:
                if (engancharseInp == false)
                {
                    estado = Estado.normal;
                    renderizadorLin.enabled = false;
                    this.transform.parent = null;
                    impulsoCai = new Vector3 (balanceo.twii.velocidad.x, 0, balanceo.twii.velocidad.z);
                    direccionMov.y = balanceo.twii.velocidad.y;
                }

                break;
        }
    }


    // Se le permite al jugador influir en el balanceo usando el input para impulsar al personaje (aunque a partir de cierta altura no es posible ya que sino el movimiento deja de ser realista), además, limitamos el movimiento a un sólo eje,
    //haciendo que la posición del personaje quede fija en el otro y también rotamos al personaje de manera que se alinee con el movimiento.
    private void Balancear ()
    {
        if (this.transform.position.y < limiteBal) 
        {
            if (verticalInp != 0)
            {
                balanceo.twii.velocidad += camara.transform.forward * verticalInp;
            }
            if (horizontalInp != 0)
            {
                balanceo.twii.velocidad += camara.transform.right * horizontalInp;
            }
        }
        if (movimientoXBal == true)
        {
            balanceo.twii.velocidad.z = 0;
            this.transform.position = Vector3.Lerp (this.transform.position, new Vector3 (this.transform.position.x, this.transform.position.y, enganchePnt.z), Time.deltaTime);
        }
        else 
        {
            balanceo.twii.velocidad.x = 0;
            this.transform.position = Vector3.Lerp (this.transform.position, new Vector3 (enganchePnt.x, this.transform.position.y, this.transform.position.z), Time.deltaTime);
        }
        this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionBal, Time.deltaTime);
        this.transform.localPosition = balanceo.Mover (this.transform.localPosition, previaPos, Time.deltaTime);
    }


    // En principio no haría falta (?).
    /*private void Caer ()
    {
        balanceo.cuerda.longitud = Mathf.Infinity;
        this.transform.localPosition = balanceo.Caida (this.transform.localPosition, Time.deltaTime);
    }*/


    // Movemos al personaje mientras camina o cae y lo rotamos acorde al movimiento, también tenemos en cuenta el impulso que puede estar recibiendo mientras cae tras balancearse.
    private void Caminar ()
    {
        balanceo.twii.velocidad = Vector3.zero;

        if ((verticalInp != 0 || horizontalInp != 0) && impulsoPar == Vector3.zero) 
        {
            Vector3 relativoCam = (camara.transform.right * horizontalInp + camara.transform.forward * verticalInp).normalized * movimientoVel;

            if (impulsoCai == Vector3.zero) 
            {
                direccionMov.x = relativoCam.x;
                direccionMov.z = relativoCam.z;
            }
            else
            {
                direccionMov.x = relativoCam.x / 2;
                direccionMov.z = relativoCam.z / 2;
            }
            impulsoBal = direccionMov;
            impulsoMnt = true;

            Rotar ();
        }

        if (impulsoPar == Vector3.zero)
        {
            direccionMov += impulsoCai;
        }
        else 
        {
            if (movimientoXEsc == true)
            {
                direccionMov.z = impulsoPar.z;
            }
            else 
            {
                direccionMov.x = impulsoPar.x;
            }

            Rotar ();
        }

        //characterCtr.Move (direccionMov * Time.deltaTime);

        if (characterCtr.isGrounded == true) 
        {
            impulsoCai = Vector3.zero;
        }
    }


    // Hacemos que el personaje rote de acuerdo con la dirección hacia donde se mueve.
    private void Rotar () 
    {
        float angulo;
        Quaternion rotacion;

        angulo = Mathf.Atan2 (direccionMov.x, direccionMov.z) * Mathf.Rad2Deg;
        rotacion = Quaternion.Euler (this.transform.rotation.x, angulo, this.transform.rotation.z);
        this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacion, rotacionVel * Time.deltaTime);
    }


    // Si el personaje está tocando el suelo o enganchado, la gravedad aplicada es casi nula; en caso contrario, se incrementa según cae.
    private void AplicarGravedad () 
    {
        if (estado != Estado.normal && estado != Estado.rodando)
        {
            direccionMov.y = 0;
        }
        else 
        {
            if (Sueleado () == true)
            {
                direccionMov.y = 0;
            }
            else 
            {
                direccionMov.y += gravedad;
                if (direccionMov.y < 0) 
                {
                    impulsoPar = Vector3.zero;
                }
            }
        }
    }


    // Si el personaje está en el suelo y se ha pulsado el botón de salto, aplicamos una fuerza vertical que le permite saltar.
    private void SaltarSuelo () 
    {
        if (saltoInp == true && Sueleado () == true) 
        {
            direccionMov.y = saltoVel;
            saltado = true;

            this.Invoke ("FinImpulso", 0.1f);
        }
    }


    // Usamos está función para poner la variable a "false" tras un pequeño intervalo de tiempo.
    private void FinImpulso () 
    {
        saltado = false;
    }


    // Se llama si el personaje se está balanceando y permite mostrar la cuerda que le mantiene colgado de manera correcta.
    private void CambiarPosicionCuerda () 
    {
        renderizadorLin.SetPosition (0, enganchePnt);
        renderizadorLin.SetPosition (1, this.transform.position);
    }


    // Twii salta hacia arriba y en la dirección opuesta a la pared por la que está trepando. 
    private void SaltarPared () 
    {
        if (saltoInp == true) 
        {
            impulsoPar = -this.transform.forward * saltoVel;
            impulsoPar.y = saltoVel;
        }
    }


    // Según el tipo de superficie escalable, el input en horizontal se usará para movernos en un eje u otro.
    private void Trepar () 
    {
        if (verticalInp != 0 || horizontalInp != 0)
        {
            Vector3 relativoCam;

            if (movimientoXEsc == true) 
            {
                relativoCam = (camara.transform.right * horizontalInp + camara.transform.up * verticalInp);
                direccionMov.x = relativoCam.x;
                if ((direccionMov.x > 0 && this.transform.position.x >= limiteEsc1) || (direccionMov.x < 0 && this.transform.position.x <= limiteEsc2))
                {
                    direccionMov.x = 0;
                }
            }
            else 
            {
                relativoCam = (camara.transform.forward * horizontalInp + camara.transform.up * verticalInp);
                direccionMov.z = relativoCam.z;
                if ((direccionMov.z > 0 && this.transform.position.z >= limiteEsc1) || (direccionMov.z < 0 && this.transform.position.z <= limiteEsc2)) 
                {
                    direccionMov.z = 0;
                }
            }
            direccionMov.y = relativoCam.y;
            direccionMov = direccionMov.normalized * escaladaVel;
        }
        direccionMov += impulsoPar;
        this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionEsc, Time.deltaTime * rotacionVel);

        characterCtr.Move (direccionMov * Time.deltaTime);
    }


    // .
    /*private void SaltarBola () 
    {
        if (saltoInp == true) 
        {
            direccionMov.y = saltoVel;
        }
    }*/


    // Miramos la posición hacia la que se mueve la bola para rotar al jugador de manera acorde, y también nos aseguramos de que su posición se corresponda con la parte superior de la bola.
    private void MantenerEquilibrio () 
    {
        if (verticalInp != 0 || horizontalInp != 0)
        {
            Vector3 relativoCam = (camara.transform.right * horizontalInp + camara.transform.forward * verticalInp);

            direccionMov.x = relativoCam.x;
            direccionMov.z = relativoCam.z;

            Rotar ();
        }

        this.transform.position = Vector3.Lerp (this.transform.position, bolaSup.position, movimientoVel * Time.deltaTime);
    }
}
/*using UnityEngine.SceneManagement;
using UnityEngine;



public class SwingController : MonoBehaviour
{

    public Camera cam;
    public Balanceo pendulum;

    [SerializeField] private float speed, jumpSpeed, gravity;
    [SerializeField] private LayerMask capas;
    private Vector3 moveDirection;
    private CharacterController controller;
    private enum State { Swinging, Falling, Walking };
    private State state;
    private Vector3 previousPosition;
    private float distToGround;
    private Vector3 hitPos;


    // .
    private void Start ()
    {
        moveDirection = Vector3.zero;
        controller = this.GetComponent<CharacterController> ();
        state = State.Walking;
        pendulum.twiiTrf.parent = pendulum.enganche.engancheTrf;
        previousPosition = this.transform.localPosition;
        distToGround = controller.height + 0.1f;
    }


    // .
    private void Update ()
    {
        DetermineState ();

        switch (state)
        {
            case State.Swinging:
                DoSwingAction ();

                break;
            case State.Falling:
                DoFallingAction ();

                break;
            case State.Walking:
                DoWalkingAction ();

                break;
        }

        previousPosition = this.transform.localPosition;
    }


    // .
    private bool IsGrounded()
    {
        //print ("Grounded");
        return Physics.Raycast (this.transform.position, -Vector3.up, distToGround);
    }


    // .
    private void DetermineState ()
    {
        // Determine State
        if (IsGrounded () == true)
        {
            state = State.Walking;
        }
        else if (Mathf.RoundToInt (Input.GetAxisRaw ("Engancharse")) != 0)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay (Input.mousePosition);

            if (Physics.Raycast (ray, out hit, capas) == true)
            {
                if (state == State.Walking)
                {
                    pendulum.twii.velocidad = moveDirection;
                }
                pendulum.CambiarEnganche(hit.point);
                state = State.Swinging;

            }
        }
        else if (Input.GetButtonDown ("Fire2") == true)
        {
            if (state == State.Swinging)
            {
                state = State.Falling;
            }
        }
    }


    // .
    private void DoSwingAction ()
    {
        if (Input.GetKey(KeyCode.W))
        {
            pendulum.twii.velocidad += pendulum.twii.velocidad.normalized * 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            pendulum.twii.velocidad -= cam.transform.right * 1.2f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            pendulum.twii.velocidad += cam.transform.right * 1.2f;
        }
        this.transform.localPosition = pendulum.Mover (transform.localPosition, previousPosition, Time.deltaTime);
        previousPosition = this.transform.localPosition;
    }


    // .
    private void DoFallingAction ()
    {
        pendulum.cuerda.longitud = Mathf.Infinity;
        this.transform.localPosition = pendulum.Fall (transform.localPosition, Time.deltaTime);
        previousPosition = this.transform.localPosition;
    }


    // .
    private void DoWalkingAction ()
    {
        pendulum.twii.velocidad = Vector3.zero;
        if (controller.isGrounded == true)
        {
            moveDirection = new Vector3 (Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal")), 0, Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical")));
            moveDirection = cam.transform.TransformDirection (moveDirection);
            moveDirection.y = 0;
            moveDirection *= speed;

            if (Input.GetButton ("Salto") == true)
            {
                moveDirection.y = jumpSpeed;
            }

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move (moveDirection * Time.deltaTime);
    }


    // .
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if (hit.gameObject.name == "Respawn")
        {
            //if too far from arena, reset level
            SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
        }
    }


    // .
    private void OnCollisionEnter (Collision collision)
    {
        Vector3 undesiredMotion = collision.contacts[0].normal * Vector3.Dot(pendulum.twii.velocidad, collision.contacts[0].normal);

        pendulum.twii.velocidad = pendulum.twii.velocidad - (undesiredMotion * 1.2f);
        hitPos = transform.position;

        if (collision.gameObject.name == "Respawn")
        {
            //if too far from arena, reset level
            SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
        }
    }
}*/