
using UnityEngine.SceneManagement;
using UnityEngine;



public class MovimientoHistoria3 : MonoBehaviour
{
    public bool input;
    public Balanceo balanceo;
    public Vector3 enganchePnt;

    [SerializeField] private int movimientoVel, rotacionVel, saltoVel, gravedad;
    [SerializeField] private LayerMask capas;
    private Camera camara;
    private Vector3 direccionMov, previaPos;
    private CharacterController characterCtr;
    private enum Estado { Balanceandose, Cayendo, Caminando };
    private Estado estado;
    private float sueloDst;
    private bool enganchado, movimientoX;


    // Inicialización de variables.
    private void Start ()
    {
        camara = GameObject.FindGameObjectWithTag("CamaraPrincipal").GetComponent<Camera> ();
        direccionMov = Vector3.zero;
        characterCtr = this.GetComponent<CharacterController> ();
        estado = Estado.Caminando;
        balanceo.twiiTrf.parent = balanceo.enganche.engancheTrf;
        previaPos = this.transform.localPosition;
        sueloDst = characterCtr.height + 0.1f;
        enganchado = false;
    }


    // Determinamos el estado actual en el que se encuentra el personaje y actuamos en consecuencia, haciendo que siga caminando, balanceándose o caiga.
    private void Update ()
    {
        DeterminarEstado ();
        switch (estado)
        {
            case Estado.Caminando:
                Caminar ();

                break;
            case Estado.Balanceandose:
                Balancear ();

                break;
            default:
                Caer ();

                break;
        }

        previaPos = this.transform.localPosition;
    }


    // Si golpeamos el objeto que provoca que reaparezcamos, la escena es reiniciada.
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if (hit.transform.tag == "Reaparecer")
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
        }
    }


    // .
    private void OnCollisionEnter (Collision collision)
    {
        Vector3 indeseadoMov = collision.GetContact(0).normal * Vector3.Dot (balanceo.twii.velocidad, collision.GetContact(0).normal);

        balanceo.twii.velocidad = balanceo.twii.velocidad - (indeseadoMov * 1.2f);

        if (collision.transform.tag == "Reaparecer")
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
        }
    }


    // Lanzamos un raycast hacia abajo de no mucha mayor longitud que la altura del personaje para comprobar si este está tocando el suelo o no.
    private bool Sueleado ()
    {
        //print ("Grounded");
        return Physics.Raycast (this.transform.position, -Vector3.up, sueloDst);
    }


    // .
    private void DeterminarEstado ()
    {
        if (Sueleado () == true)
        {
            estado = Estado.Caminando;
            enganchado = false;
        }
        else if (enganchePnt != Vector3.zero && Input.GetButtonDown ("Engancharse") == true && Physics.Raycast (this.transform.position, enganchePnt - this.transform.position, 15, capas, QueryTriggerInteraction.Ignore) == true)
        {
            if (estado == Estado.Caminando)
            {
                balanceo.twii.velocidad = direccionMov;
            }
            estado = Estado.Balanceandose;
            //enganchado = true;

            balanceo.CambiarEnganche (enganchePnt);
        }
        /*else if (Input.GetButtonDown ("Engancharse") == true)
        {
            RaycastHit infoRay;

            Ray ray = camara.ScreenPointToRay (Input.mousePosition);

            if (Physics.Raycast (ray, out infoRay, capas) == true)
            {
                if (estado == Estado.Caminando)
                {
                    balanceo.twii.velocidad = direccionMov;
                }
                estado = Estado.Balanceandose;
                enganchado = true;

                balanceo.CambiarEnganche (infoRay.point);
            }
        }*/
        else if (Input.GetButtonDown ("Soltarse") == true)
        {
            if (estado == Estado.Balanceandose)
            {
                estado = Estado.Cayendo;
                enganchado = false;
            }
        }
    }


    // .
    private void Balancear ()
    {
        int horizontalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal"));
        int verticalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical"));

        /*if (Input.GetKey (KeyCode.W) == true)
        {
            balanceo.twii.velocidad += balanceo.twii.velocidad.normalized * 2;
        }*/
        if (horizontalInp != 0)
        {
            balanceo.twii.velocidad += camara.transform.right * horizontalInp / 2;
        }
        if (verticalInp != 0) 
        {
            balanceo.twii.velocidad += camara.transform.forward * verticalInp / 2;
        }
        if (movimientoX == true)
        {
            balanceo.twii.velocidad.z = 0;
            this.transform.position = Vector3.Lerp (this.transform.position, new Vector3 (this.transform.position.x, this.transform.position.y, enganchePnt.z), Time.deltaTime);
        }
        else 
        {
            balanceo.twii.velocidad.x = 0;
            this.transform.position = Vector3.Lerp (this.transform.position, new Vector3 (enganchePnt.x, this.transform.position.y, this.transform.position.z), Time.deltaTime);
        }
        /*if (verticalInp == +1)
        {
            balanceo.twii.velocidad += balanceo.twii.velocidad.normalized * 2;
        }*/
        /*if (Input.GetKey (KeyCode.A) == true)
        {
            balanceo.twii.velocidad -= camara.transform.right * 1.2f;
        }
        if (Input.GetKey (KeyCode.D) == true)
        {
            balanceo.twii.velocidad += camara.transform.right * 1.2f;
        }*/
        this.transform.localPosition = balanceo.Mover (this.transform.localPosition, previaPos, Time.deltaTime);
        previaPos = this.transform.localPosition;
    }


    // .
    private void Caer ()
    {
        balanceo.cuerda.longitud = Mathf.Infinity;
        this.transform.localPosition = balanceo.Caida (this.transform.localPosition, Time.deltaTime);
        previaPos = this.transform.localPosition;
    }


    // .
    private void Caminar ()
    {
        float angulo;
        Quaternion rotacion;

        balanceo.twii.velocidad = Vector3.zero;
        if (characterCtr.isGrounded == true)
        {
            direccionMov = new Vector3 (Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal")), 0, Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical")));
            direccionMov = camara.transform.TransformDirection (direccionMov);
            direccionMov *= movimientoVel;
            if (Input.GetButton ("Salto") == true)
            {
                direccionMov.y = saltoVel;
            }
            else
            {
                direccionMov.y = 0;
            }
        }
        direccionMov.y += gravedad * Time.deltaTime;
        if (direccionMov.x != 0 || direccionMov.z != 0)
        {
            angulo = Mathf.Atan2 (direccionMov.x, direccionMov.z) * Mathf.Rad2Deg;
            rotacion = Quaternion.Euler (this.transform.rotation.x, angulo, this.transform.rotation.z);
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacion, rotacionVel * Time.deltaTime);
        }

        characterCtr.Move (direccionMov * Time.deltaTime);
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