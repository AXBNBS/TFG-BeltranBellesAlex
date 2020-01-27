
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MovimientoCharacterController : MonoBehaviour
{
    public bool input;

    [SerializeField] private int movimientoVel, rotacionVel, saltoVel;
    [SerializeField] private LayerMask capas;
    private int gravedad;
    private bool saltarInp;
    private CharacterController characterCtr;
    private float horizontalInp, verticalInp, offsetY, offsetXZ;
    private Transform camaraTrf;
    private Animator animator;
    public Vector3 movimiento, empuje;
    private Vector3[] offsets;


    // Inicialización de variables.
    private void Start ()
    {
        gravedad = -10;
        characterCtr = this.GetComponent<CharacterController> ();
        offsetY = this.transform.localScale.y * characterCtr.height;
        offsetXZ = this.transform.localScale.x * characterCtr.radius * 3;
        offsets = new Vector3[] {new Vector3 (+offsetXZ, offsetY, 0), new Vector3 (-offsetXZ, offsetY, 0), new Vector3 (0, offsetY, +offsetXZ), new Vector3 (0, offsetY, -offsetXZ)};
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
        animator = this.GetComponent<Animator> ();
    }


    // En el caso de que el input esté permitido, obtendremos el relativo al movimiento de las teclas/botones correspondiente y moveremos y animaremos al personaje en 
    //consecuencia.
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
            saltarInp = Input.GetButtonDown ("Salto");
        }
        movimiento.x = 0;
        movimiento.z = 0;

        if (characterCtr.isGrounded == true && saltarInp == true)
        {
            Saltar ();
        }
        Mover (horizontalInp, verticalInp);
        Animar ();
    }


    // Si el personaje ha caído sobre otro, empujarlo hacia el primer lado de este que se encuentre libre.
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if (hit.transform.tag == "Jugador" && this.transform.position.y > hit.transform.position.y)
        {
            Vector3 centroSup = new Vector3(hit.transform.position.x, hit.transform.position.y + offsetY, hit.transform.position.z);

            if (Physics.Raycast(centroSup, hit.transform.right, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
            {
                empuje = hit.transform.right;

                return;
            }

            if (Physics.Raycast(centroSup, hit.transform.forward, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
            {
                empuje = hit.transform.forward;

                return;
            }

            if (Physics.Raycast(centroSup, -hit.transform.forward, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
            {
                empuje = -hit.transform.forward;

                return;
            }

            if (Physics.Raycast(centroSup, -hit.transform.right, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
            {
                empuje = -hit.transform.right;

                return;
            }
        }
        else
        {
            empuje = Vector3.zero;
        }
    }


    /*private void OnDrawGizmosSelected ()
    {
        Gizmos.DrawLine (new Vector3 (this.transform.position.x, this.transform.position.y + offsetY, this.transform.position.z), this.transform.position + offsets[0]);
    }*/


    // Devuelve "true" si el personaje en cuestión está parado.
    public bool EstaParado ()
    {
        return (animator.GetCurrentAnimatorStateInfo(0).IsName ("Idle"));
    }


    // Le aplicamos gravedad al personaje y, si además está siendo movido por el jugador, lo movemos y rotamos adecuadamente hacia la dirección del movimiento.
    private void Mover (float horizontal, float vertical) 
    {
        if (characterCtr.isGrounded == false)
        {
            movimiento.y += gravedad;
        }

        if (horizontal != 0 || vertical != 0)
        {
            float angulo;
            Quaternion rotacion;

            Vector3 relativoCam = (camaraTrf.right * horizontal + camaraTrf.forward * vertical).normalized * movimientoVel;

            movimiento.x = relativoCam.x;
            movimiento.z = relativoCam.z;
            angulo = Mathf.Atan2 (movimiento.x, movimiento.z) * Mathf.Rad2Deg + 90;
            rotacion = Quaternion.Euler (this.transform.rotation.x, angulo, this.transform.rotation.z);
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacion, rotacionVel * Time.deltaTime);
        }
        movimiento += empuje * movimientoVel / 2;

        characterCtr.Move (movimiento * Time.deltaTime);

        if (characterCtr.isGrounded == true)
        {
            movimiento.y = -0.1f;
        }
    }


    // Si el personaje está en el suelo y se ha pulsado el botón de salto, haremos que salte.
    private void Saltar ()
    {
        movimiento.y = saltoVel;
    }


    // Gestiona las animaciones del personaje de acuerdo a su situación actual.
    private void Animar ()
    {
        animator.SetBool ("moviendose", movimiento.x != 0 || movimiento.z != 0);
        animator.SetBool ("tocandoSuelo", characterCtr.isGrounded);
        animator.SetFloat ("velocidadY", movimiento.y);
    }
}