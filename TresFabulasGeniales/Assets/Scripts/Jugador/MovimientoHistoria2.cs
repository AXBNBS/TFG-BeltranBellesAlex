
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class MovimientoHistoria2 : MonoBehaviour
{
    public bool input;
    public Vector3 movimiento;
    public int saltoVel;
    [HideInInspector] public float offsetY, offsetXZ;

    [SerializeField] private int movimientoVel, rotacionVel;
    [SerializeField] private LayerMask capas;
    [SerializeField] private MovimientoHistoria2 companyeroMov;
    private int gravedad, empujeVel;
    public bool saltarInp, seguir, yendo, sueleado, empujando, limitadoX;
    private CharacterController characterCtr;
    private float horizontalInp, verticalInp, angulo;
    private Quaternion rotacion;
    private Transform camaraTrf, objetivoSeg, companyeroTrf;
    private Animator animator;
    private Vector3 empuje;
    private NavMeshAgent mallaAgtNav;
    private ObjetoMovil empujado;


    // Inicialización de variables.
    private void Start ()
    {
        gravedad = -11;
        empujeVel = movimientoVel / 3;
        seguir = false;
        yendo = false;
        sueleado = false;
        empujando = false;
        characterCtr = this.GetComponent<CharacterController> ();
        offsetY = this.transform.localScale.y * characterCtr.height;
        offsetXZ = this.transform.localScale.x * characterCtr.radius * 3;
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
        //objetivoSeg = companyeroMov.transform.GetChild (1);
        //companyeroTrf = companyeroMov.transform;
        animator = this.GetComponentInChildren<Animator> ();
        mallaAgtNav = this.GetComponent<NavMeshAgent> ();
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
            if (empujando == false) 
            {
                saltarInp = Input.GetButtonDown ("Salto");
            }
        }
        movimiento.x = 0;
        movimiento.z = 0;

        if (saltarInp == true && characterCtr.isGrounded == true)
        {
            Saltar ();
        }
        if (seguir == false)
        {
            Mover (horizontalInp, verticalInp);
        }
        else
        {
            Seguir ();
            DesagruparSiEso ();
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
                if (this.transform.position.y > tocado.position.y) 
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

                    if (Physics.Raycast (centroSup, -tocado.forward, offsetXZ, capas, QueryTriggerInteraction.Ignore) == false)
                    {
                        empuje = -tocado.forward;

                        return;
                    }
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
        
    }


    // Pone "sueleado" a "true" para evitar que se reproduzca la animación de estar en el aire cuando realmente no lo está.
    private void OnTriggerStay (Collider other)
    {
        if (other.tag == "Sueleador")
        {
            sueleado = true;
        }
    }


    // Pone "sueleado" a "false" para permitir que se reproduzca la animación de estar en el aire una vez se haya salido del rango que ocupa el trigger.
    private void OnTriggerExit (Collider other)
    {
        if (other.tag == "Sueleador") 
        {
            sueleado = false;
        }
    }


    // Se pone "sueleado" a "true" durante 0.1 segundos ya que si no puede detectar que está en el aire durante un momento y reproducir la animación equivocada.
    public void GestionarCambio ()
    {
        sueleado = true;

        this.Invoke ("CambiarSueleado", 0.1f);
    }


    // Devuelve "true" si el personaje en cuestión está parado.
    public bool EstaEnElAire ()
    {
        return (animator.GetCurrentAnimatorStateInfo(0).IsTag ("Aire"));
    }


    // Hacemos que la variable que hace que se siga al compañero sea verdadera y activamos el "NavMeshAgent" para poder seguir al otro personaje.
    public void GestionarSeguimiento (bool comenzar)
    {
        seguir = comenzar;
        mallaAgtNav.enabled = comenzar;
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


    // Si el personaje está en el suelo y se ha pulsado el botón de salto, haremos que salte.
    private void Saltar ()
    {
        movimiento.y = saltoVel;
    }


    // Le aplicamos gravedad al personaje y, si además está siendo movido por el jugador, lo movemos y rotamos adecuadamente hacia la dirección del movimiento, teniendo en cuenta además la posición de la cámara para que el movimiento sea relativo a la
    //misma. Movemos también el objeto empujado en caso de que haya alguno.
    private void Mover (float horizontal, float vertical) 
    {
        if (horizontal != 0 || vertical != 0)
        {
            Vector3 relativoCam;

            if (empujando == false)
            {
                relativoCam = (camaraTrf.right * horizontal + camaraTrf.forward * vertical).normalized * movimientoVel;
                movimiento.x = relativoCam.x;
                movimiento.z = relativoCam.z;
            }
            else 
            {
                relativoCam = (camaraTrf.right * horizontal + camaraTrf.forward * vertical);
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
            angulo = Mathf.Atan2 (movimiento.x, movimiento.z) * Mathf.Rad2Deg;
            rotacion = Quaternion.Euler (this.transform.rotation.x, angulo, this.transform.rotation.z);
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacion, rotacionVel * Time.deltaTime);
        }
        movimiento += empuje * movimientoVel / 2;

        characterCtr.Move (movimiento * Time.deltaTime);
        if (empujando == true) 
        {
            empujado.Mover (movimiento * Time.deltaTime);
        }
    }


    // Es que si no flota.
    private void AplicarGravedad () 
    {
        if (characterCtr.isGrounded == false)
        {
            movimiento.y += gravedad;
        }
        else
        {
            movimiento.y = -0.1f;
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
        
        yendo = (this.transform.position - posicionPnt).magnitude > mallaAgtNav.stoppingDistance && Physics.Raycast (rayo, diferencia.magnitude, capas, QueryTriggerInteraction.Ignore) == false;

        if (yendo == false)
        {
            mallaAgtNav.SetDestination (this.transform.position);

            mirarDir = (companyeroTrf.position - this.transform.position).normalized;
            objetivoRot = Quaternion.LookRotation(mirarDir).eulerAngles;
            rotacionY = Quaternion.Euler (this.transform.rotation.eulerAngles.x, objetivoRot.y, this.transform.rotation.eulerAngles.z);
            this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionY, rotacionVel * Time.deltaTime);
        }
        else 
        {
            mallaAgtNav.SetDestination (posicionPnt);

            mirarDir = (posicionPnt - this.transform.position).normalized;
            objetivoRot = Quaternion.LookRotation(mirarDir).eulerAngles;
            rotacionY = Quaternion.Euler (this.transform.rotation.eulerAngles.x, objetivoRot.y, this.transform.rotation.eulerAngles.z);
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
        if (seguir == false)
        {
            animator.SetBool ("moviendose", movimiento.x != 0 || movimiento.z != 0);
            animator.SetBool ("tocandoSuelo", characterCtr.isGrounded || sueleado);
            animator.SetFloat ("velocidadY", movimiento.y);
        }
        else
        {
            animator.SetBool ("moviendose", yendo);
            animator.SetBool ("tocandoSuelo", true);
        }
    }


    // Para evitar que reproduzca la animación de estar en el aire.
    private void CambiarSueleado () 
    {
        sueleado = false;
    }
}