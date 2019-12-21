
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MovimientoCharacterController : MonoBehaviour
{
    public bool input;

    [SerializeField] private int movimientoVel, rotacionVel;
    private int gravedad;
    private float horizontalInp, verticalInp;
    private Vector3 movimiento;
    private CharacterController characterCtr;
    private Transform camaraTrf;

    
    // Inicialización de variables.
    private void Start ()
    {
        input = true;
        gravedad = -8;
        characterCtr = this.GetComponent<CharacterController> ();
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
    }


    // En el caso de que el input esté permitido, obtendremos el relativo al movimiento de las teclas/joysticks correspondiente y moveremos al personaje en consecuencia.
    private void Update ()
    {
        if (input == false)
        {
            horizontalInp = 0;
            verticalInp = 0;
        }
        else 
        {
            horizontalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Horizontal"));
            verticalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Vertical"));
        }

        Mover (horizontalInp, verticalInp);
    }


    // Le aplicamos gravedad al personaje y, si además está siendo movido por el jugador, lo movemos y rotamos adecuadamente hacia la dirección del movimiento.
    private void Mover (float horizontal, float vertical) 
    {
        movimiento = new Vector3 (0, gravedad, 0);

        if (horizontal != 0 || vertical != 0)
        {
            float angulo;
            Quaternion rotacion;

            Vector3 relativoCam = (camaraTrf.right * horizontal + camaraTrf.forward * vertical).normalized;

            movimiento += relativoCam * movimientoVel;
            angulo = Mathf.Atan2 (movimiento.x, movimiento.z) * Mathf.Rad2Deg + 90;
            rotacion = Quaternion.Euler (this.transform.rotation.x, angulo, this.transform.rotation.z);
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacion, rotacionVel * Time.deltaTime);
        }

        characterCtr.Move (movimiento * Time.deltaTime);
    }
}