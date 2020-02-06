
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class MovimientoHistoria2 : MonoBehaviour
{
    public bool input;
    [HideInInspector] public float offsetY, offsetXZ;

    [SerializeField] private int movimientoVel, rotacionVel, saltoVel;
    [SerializeField] private LayerMask capas;
    [SerializeField] private MovimientoHistoria2 companyeroMov;
    private int gravedad, empujeVel;
    private bool saltarInp, seguir, yendo, sueleado, empujando, limitadoX;
    private CharacterController characterCtr;
    private float horizontalInp, verticalInp, angulo;
    private Quaternion rotacion;
    private Transform camaraTrf, objetivoSeg, companyeroTrf;
    private Animator animator;
    private Vector3 movimiento, empuje;
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
        objetivoSeg = companyeroMov.transform.GetChild (1);
        companyeroTrf = companyeroMov.transform;
        animator = this.GetComponentInChildren<Animator> ();
        mallaAgtNav = this.GetComponent<NavMeshAgent> ();
    }


    // En el caso de que el input esté permitido, obtendremos el relativo al movimiento de las teclas/botones correspondiente y moveremos y animaremos al personaje en consecuencia.
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
        if (characterCtr.isGrounded == false)
        {
            AplicarGravedad ();
        }
    }


    // Si el personaje ha caído sobre otro, empujarlo hacia el primer lado de este que se encuentre libre.
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        Transform tocado = hit.transform;
        if (tocado.tag == this.tag && this.transform.position.y > tocado.position.y)
        {
            Vector3 centroSup = new Vector3(tocado.position.x, tocado.position.y + offsetY, tocado.position.z);

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
        else 
        {
            empuje = Vector3.zero;
        }
    }


    // Pal debug y tal.
    private void OnDrawGizmosSelected ()
    {
        
    }


    // Pone "sueleado" a "true" para evitar que se reproduzca la animación de estar en el aire.
    private void OnTriggerEnter (Collider other)
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


    // Hacemos que la variable que hace que se siga al compañero sea verdadera.
    public void GestionarSeguimiento (bool comenzar)
    {
        seguir = comenzar;
        mallaAgtNav.enabled = comenzar;
    }


    // .
    public void ComenzarEmpuje (bool enEjeX, ObjetoMovil objeto) 
    {
        empujando = true;
        limitadoX = enEjeX;
        empujado = objeto;
    }


    // .
    public void PararEmpuje () 
    {
        empujando = false;
    }


    // Le aplicamos gravedad al personaje y, si además está siendo movido por el jugador, lo movemos y rotamos adecuadamente hacia la dirección del movimiento.
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

        if (characterCtr.isGrounded == true)
        {
            movimiento.y = -0.1f;
        }
    }


    // Es que si no flota.
    private void AplicarGravedad () 
    {
        movimiento.y += gravedad;
    }


    // Si el personaje está en el suelo y se ha pulsado el botón de salto, haremos que salte.
    private void Saltar ()
    {
        movimiento.y = saltoVel;
    }


    // Si no hay colliders chungos detrás del personaje seguido, la distancia entre la posición actual y la del punto a seguir es distinta a cero y no hay obstáculos entre el personaje y ese punto, nos movemos hacia ahí.
    private void Seguir ()
    {
        Vector3 mirarDir, objetivoRot;
        Quaternion rotacionY;

        Vector3 posicionPnt = objetivoSeg.position;
        Vector3 diferencia = posicionPnt - objetivoSeg.parent.position;
        Ray rayo = new Ray (objetivoSeg.parent.position, diferencia);
        //NavMeshPath caminito = new NavMeshPath ();
        //print (mallaAgtNav.CalculatePath (posicionPnt, caminito));
        
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
        /*puntoIni = new Vector3 (this.transform.position.x, this.transform.position.y + offsetY / 5, this.transform.position.z);

        Vector3 puntoIni2 = puntoIni + this.transform.localScale.x * characterCtr.radius * this.transform.forward;
        Vector3 puntoIni3 = puntoIni - this.transform.localScale.x * characterCtr.radius * this.transform.forward;

        objetivoSeg = new Vector3 (companyero.transform.position.x, companyero.transform.position.y + offsetY / 5, companyero.transform.position.z) + companyero.transform.right * 15;

        Vector3 direccion = puntoIni - objetivoSeg;
        print (direccion);
        float distancia = direccion.magnitude + offsetXZ / 3;
        if (direccion != Vector3.zero && companyero.ColliderChungoAEspaldas () == false && Physics.Raycast (puntoIni, direccion, distancia, capas, QueryTriggerInteraction.Ignore) == false && 
            Physics.Raycast (puntoIni2, direccion, distancia, capas, QueryTriggerInteraction.Ignore) == false && Physics.Raycast (puntoIni3, direccion, distancia, capas, QueryTriggerInteraction.Ignore) == false)
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
            }
        }*/
    }


    // Si la distancia en y es mayor a 5, automáticamente desagrupamos a los personajes.
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
        if (input == true)
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