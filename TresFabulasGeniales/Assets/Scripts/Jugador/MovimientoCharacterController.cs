
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MovimientoCharacterController : MonoBehaviour
{
    public bool input;

    [SerializeField] private int movimientoVel, rotacionVel, saltoVel;
    private int gravedad;
    private float horizontalInp, verticalInp;
    private bool saltarInp, sueleadoUltFrm;
    private Vector3 movimiento;
    private CharacterController characterCtr;
    private Transform camaraTrf;
    private Animator animator;

    
    // Inicialización de variables.
    private void Start ()
    {
        gravedad = -600;
        sueleadoUltFrm = true;
        characterCtr = this.GetComponent<CharacterController> ();
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
        if (sueleadoUltFrm == true && characterCtr.isGrounded == false)
        {
            movimiento.y = 0;
        }

        if (characterCtr.isGrounded == true && saltarInp == true)
        {
            Saltar ();
        }
        Mover (horizontalInp, verticalInp);
        Animar ();

        sueleadoUltFrm = characterCtr.isGrounded;
    }


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
            movimiento.y += gravedad * Time.deltaTime;
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

        characterCtr.Move (movimiento * Time.deltaTime);
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