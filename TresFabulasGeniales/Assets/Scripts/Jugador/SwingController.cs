
using UnityEngine.SceneManagement;
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
}