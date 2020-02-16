
using UnityEngine.SceneManagement;
using UnityEngine;



public class MovimientoHistoria3 : MonoBehaviour
{
    public Balanceo balanceo;

    [SerializeField] private float movimientoVel, saltoVel, gravedad;
    [SerializeField] private LayerMask capas;
    private Camera camara;
    private Vector3 direccionMov, previaPos, golpePos;
    private CharacterController characterCtr;
    private enum Estado { Balanceandose, Cayendo, Caminando };
    private Estado estado;
    private float sueloDst;


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
        if (hit.transform.name == "Reaparecer")
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
        }
    }


    // .
    private void OnCollisionEnter (Collision collision)
    {
        Vector3 indeseadoMov = collision.GetContact(0).normal * Vector3.Dot (balanceo.twii.velocidad, collision.GetContact(0).normal);

        balanceo.twii.velocidad = balanceo.twii.velocidad - (indeseadoMov * 1.2f);
        golpePos = this.transform.position;

        if (collision.transform.name == "Reaparecer")
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
        }
        else if (Mathf.RoundToInt (Input.GetAxisRaw ("Engancharse")) != 0)
        {
            RaycastHit hit;

            Ray ray = camara.ScreenPointToRay (Input.mousePosition);

            if (Physics.Raycast (ray, out hit, capas) == true)
            {
                if (estado == Estado.Caminando)
                {
                    balanceo.twii.velocidad = direccionMov;
                }
                estado = Estado.Balanceandose;

                balanceo.CambiarEnganche (hit.point);
            }
        }
        else if (Input.GetButtonDown ("Soltarse") == true)
        {
            if (estado == Estado.Balanceandose)
            {
                estado = Estado.Cayendo;
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
            balanceo.twii.velocidad += camara.transform.right * 1.2f * horizontalInp;
        }
        if (verticalInp == +1)
        {
            balanceo.twii.velocidad += balanceo.twii.velocidad.normalized * 2;
        }
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
        this.transform.localPosition = balanceo.Fall (this.transform.localPosition, Time.deltaTime);
        previaPos = this.transform.localPosition;
    }


    // .
    private void Caminar ()
    {
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

        characterCtr.Move (direccionMov * Time.deltaTime);
    }
}