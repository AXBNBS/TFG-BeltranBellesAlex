
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MovimientoCharacterController : MonoBehaviour
{
    public bool input;
    [HideInInspector] public float offsetY, offsetXZ;

    [SerializeField] private int movimientoVel, rotacionVel, saltoVel;
    [SerializeField] private LayerMask capas;
    [SerializeField] private MovimientoCharacterController companyero;
    private int gravedad;
    private bool saltarInp, seguir;
    private CharacterController characterCtr;
    private float horizontalInp, verticalInp, angulo;
    private Quaternion rotacion;
    private Transform camaraTrf;
    private Animator animator;
    private Vector3 movimiento, empuje, puntoIni, objetivoSeg;
    private List<Collider> detrasCol;
    private SphereCollider esferaCol;


    // Inicialización de variables.
    private void Start ()
    {
        gravedad = -10;
        seguir = false;
        characterCtr = this.GetComponent<CharacterController> ();
        offsetY = this.transform.localScale.y * characterCtr.height;
        offsetXZ = this.transform.localScale.x * characterCtr.radius * 3;
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
        animator = this.GetComponent<Animator> ();
        detrasCol = new List<Collider> ();
        esferaCol = this.GetComponent<SphereCollider> ();
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

        if (saltarInp == true && characterCtr.isGrounded == true)
        {
            Saltar ();
        }
        Mover (horizontalInp, verticalInp);
        if (seguir == true)
        {
            Seguir ();
            DesagruparSiEso ();
        }
        Animar ();
    }


    // Añadimos el objeto que haya entrado a la lista de colliders traseros.
    private void OnTriggerEnter (Collider other)
    {
        if (other.isTrigger == false)
        {
            detrasCol.Add (other);
        }
    }


    // Eliminamos el objeto que haya entrado de la lista de colliders traseros.
    private void OnTriggerExit (Collider other)
    {
        if (other.isTrigger == false)
        {
            detrasCol.Remove (other);
        }   
    }


    // Si el personaje ha caído sobre otro, empujarlo hacia el primer lado de este que se encuentre libre.
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if (hit.transform.tag == this.tag && this.transform.position.y > hit.transform.position.y)
        {
            Vector3 centroSup = new Vector3 (hit.transform.position.x, hit.transform.position.y + offsetY, hit.transform.position.z);

            if (Physics.Raycast (centroSup, hit.transform.right, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
            {
                empuje = hit.transform.right;

                return;
            }

            if (Physics.Raycast (centroSup, hit.transform.forward, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
            {
                empuje = hit.transform.forward;

                return;
            }

            if (Physics.Raycast (centroSup, -hit.transform.forward, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
            {
                empuje = -hit.transform.forward;

                return;
            }

            if (Physics.Raycast (centroSup, -hit.transform.right, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
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


    // .
    private void OnDrawGizmosSelected ()
    {
        
    }


    // Devuelve "true" si el personaje en cuestión está parado.
    public bool EstaEnElAire ()
    {
        return (animator.GetCurrentAnimatorStateInfo(0).IsTag ("Aire"));
    }


    // Hacemos que la variable que hace que se siga al compañero sea verdadera.
    public void GestionarSeguimiento (bool comenzar)
    {
        seguir = comenzar;

        if (seguir == false)
        {
            esferaCol.enabled = false;
        }
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


    //
    private void Seguir ()
    {
        puntoIni = new Vector3 (this.transform.position.x, this.transform.position.y + offsetY / 5, this.transform.position.z);

        Vector3 puntoIni2 = puntoIni + this.transform.localScale.x * characterCtr.radius * this.transform.forward;
        Vector3 puntoIni3 = puntoIni - this.transform.localScale.x * characterCtr.radius * this.transform.forward;

        objetivoSeg = new Vector3 (companyero.transform.position.x, companyero.transform.position.y + offsetY / 5, companyero.transform.position.z) + companyero.transform.right * 15;

        Vector3 direccion = puntoIni - objetivoSeg;
        float distancia = Vector3.Distance (this.transform.position, objetivoSeg) + offsetXZ / 3;
        if (direccion != Vector3.zero && companyero.ColliderChungoAEspaldas () == false && 
            Physics.Raycast (puntoIni, direccion, distancia, capas, QueryTriggerInteraction.Ignore) == false && Physics.Raycast (puntoIni2, direccion, distancia, capas, QueryTriggerInteraction.Ignore) == false 
            && Physics.Raycast (puntoIni3, direccion, distancia, capas, QueryTriggerInteraction.Ignore) == false)
        {
            Vector3 offset = objetivoSeg - puntoIni;

            if (offset.magnitude > 1)
            {
                angulo = Mathf.Atan2 (offset.x, offset.z) * Mathf.Rad2Deg + 90;
                rotacion = Quaternion.Euler (this.transform.rotation.x, angulo, this.transform.rotation.z);
                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacion, rotacionVel * 2 * Time.deltaTime);

                characterCtr.Move (offset.normalized * movimientoVel * Time.deltaTime);
            }
            else
            {
                this.transform.position = new Vector3 (objetivoSeg.x, this.transform.position.y, objetivoSeg.z);
                esferaCol.enabled = true;
            }
        }
    }


    //
    private void DesagruparSiEso ()
    {
        if (Mathf.Abs (this.transform.position.y - companyero.transform.position.y) > 5)
        {
            GestionarSeguimiento (false);

            CambioDePersonajesYAgrupacion.instancia.juntos = false;
        }
    }


    // Gestiona las animaciones del personaje de acuerdo a su situación actual.
    private void Animar ()
    {
        if (seguir == false)
        {
            animator.SetBool ("moviendose", movimiento.x != 0 || movimiento.z != 0);
            animator.SetBool ("tocandoSuelo", characterCtr.isGrounded);
            animator.SetFloat ("velocidadY", movimiento.y);
        }
        else
        {
            animator.SetBool ("moviendose", puntoIni != objetivoSeg);
            animator.SetBool ("tocandoSuelo", true);
        }
    }


    // Devuelve "true" si hay algun collider no ligado a los avatares jugables.
    private bool ColliderChungoAEspaldas ()
    {
        for (int c = detrasCol.Count - 1; c > -1; c -= 1)
        {
            if (detrasCol[c].tag != this.tag)
            {
                return true;
            }
        }
        return false;
    }
}