
using UnityEngine;
using System.Collections.Generic;



public class MovimientoHistoria3 : MonoBehaviour
{
    public bool input, movimientoXEsc, movimientoXBal, escalarPos, enganchePer;
    public float limiteBal, cuerdaLimSup, cuerdaLimInf, escaladaXZ;
    public float[] limitesEsc;
    public Vector3 enganchePnt;
    public Quaternion rotacionEsc;
    public Balanceo balanceo;
    public Transform bolaSup;
    [HideInInspector] public Bola bolaScr;

    [SerializeField] private int movimientoVel, rotacionVel, saltoVel, escaladaVel, gravedad, balanceoVelMin, balanceoDstMin, longitudCueVel, interpolacionBalVel, deslizVel, muerteMinVelY;
    [SerializeField] private float pendienteRayLon, deslizFrc;
    [SerializeField] private Vector3 pendienteFrz;
    //[SerializeField] private Collider escenarioCol;
    private LayerMask capasSue;
    private Transform camaraTrf, modeloTrf, cuerdaIniTrf;
    private Vector3 direccionMov, previaPos, impulsoBal, impulsoCai, impulsoPar, offsetCenCtr, normal;
    private List<Vector3> normales;
    private CharacterController personajeCtr;
    private float radioEsfSue;
    private int verticalInp, horizontalInp;
    private bool saltoInp, engancharseInp, cuerdaLonInp, cuerdaLonInpUltFrm, saltado, impulsoMnt, sueleado, alteraCue, paradoBal, pendiente, deslizar;
    private LineRenderer renderizadorLin;
    private Quaternion[] rotacionesMod;
    private enum Estado { normal, trepando, rodando, balanceandose };
    private Estado estado;
    private Animator animador;
    //private bool[] adelanteBalXZ;


    // Inicialización de variables.
    private void Start ()
    {
        capasSue = LayerMask.GetMask ("Default");
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
        direccionMov = Vector3.zero;
        personajeCtr = this.GetComponent<CharacterController> ();
        previaPos = this.transform.localPosition;
        offsetCenCtr = personajeCtr.bounds.extents.y * Vector3.down;
        normales = new List<Vector3> ();
        radioEsfSue = personajeCtr.bounds.extents.x * 0.5f;
        saltado = false;
        renderizadorLin = this.GetComponent<LineRenderer> ();
        rotacionesMod = new Quaternion[] { Quaternion.identity, Quaternion.Euler (0, 0, 35) };
        estado = Estado.normal;
        animador = this.GetComponentInChildren<Animator> ();
        modeloTrf = animador.transform;
        cuerdaIniTrf = this.transform.GetChild (this.transform.childCount - 2);
    }


    // Determinamos el estado actual en el que se encuentra el personaje y actuamos en consecuencia, haciendo que siga caminando (afectado también por la gravedad), trepando, desplazándose con una bola o balanceándose. Recogemos aquí también el input.
    private void Update ()
    {
        direccionMov.x = 0;
        direccionMov.z = 0;
        if (input == true)
        {
            verticalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento vertical"));
            horizontalInp = Mathf.RoundToInt (Input.GetAxisRaw ("Movimiento horizontal"));
            saltoInp = (deslizar == false && estado != Estado.balanceandose) ? Input.GetButtonDown ("Salto") : false;
            engancharseInp = estado != Estado.trepando && estado != Estado.rodando ? Mathf.RoundToInt (Input.GetAxisRaw ("Engancharse")) != 0 : false;
            cuerdaLonInp = Estado.balanceandose != estado ? false : Mathf.RoundToInt (Input.GetAxisRaw ("Variar altura de cuerda")) != 0;
        }
        else 
        {
            verticalInp = 0;
            horizontalInp = 0;
            saltoInp = false;
            engancharseInp = false;
        }
        sueleado = Sueleado ();

        DeterminarEstado ();
        RotarModelo ();
        switch (estado)
        {
            case Estado.normal:
                pendiente = EnPendiente ();
                //print (pendiente);

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
                SubirOBajarPorCuerda ();

                break;
        }

        if (impulsoMnt == false) 
        {
            impulsoBal = Vector3.zero;
        }
        impulsoMnt = false;
        previaPos = this.transform.localPosition;
        cuerdaLonInpUltFrm = cuerdaLonInp;

        Animar ();
    }


    // Aplicamos la gravedad a intervalos de tiempo fijos para que la velocidad del PC no afecte a la jugabilidad.
    private void FixedUpdate ()
    {
        AplicarGravedad ();
    }


    // Cada vez que el "character controller" colisiona con una superfície, añadimos su normal a la lista y reducimos el empuje a 0 por el momento.
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        normales.Add (hit.normal);
    }


    // Debug.
    private void OnDrawGizmos ()
    {
        if (personajeCtr != null) 
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere (personajeCtr.bounds.center + offsetCenCtr, radioEsfSue);
            Gizmos.DrawRay (personajeCtr.bounds.center + offsetCenCtr, pendienteRayLon * Vector3.down);
        }
    }


    // Como su propio nombre indica, realiza las operaciones necesarias para obtener un vector representando una cierta dirección en base a un ángulo dado.
    private Vector3 DireccionDeAngulo (float gradosAng)
    {
        return new Vector3 (Mathf.Sin (gradosAng * Mathf.Deg2Rad), 0, Mathf.Cos (gradosAng * Mathf.Deg2Rad));
    }


    // Dependiendo del estado del personaje, sabremos si está tocando el suelo o no. En el caso de que esté en su estado normal y el movimiento en Y sea negativo, será necesario utilizar un "CheckSphere" para aclararlo.
    private bool Sueleado ()
    {
        switch (estado) 
        {
            case Estado.normal:
                return direccionMov.y < 0 ? Physics.CheckSphere (personajeCtr.bounds.center + offsetCenCtr, radioEsfSue, capasSue, QueryTriggerInteraction.Ignore) : false;
            case Estado.trepando:
            case Estado.balanceandose:
                return false;
            default:
                return true;

        }
    }


    // Definimos, para cada posible estado, las condiciones bajo las cuáles podemos permitir que se cambie a otro.
    private void DeterminarEstado ()
    {
        switch (estado) 
        {
            case Estado.normal:
                if (escalarPos == true && sueleado == false) 
                {
                    estado = Estado.trepando;
                    impulsoCai = Vector3.zero;
                    deslizar = false;

                    //Physics.IgnoreCollision (characterCtr, escenarioCol, true);

                    break;
                }

                if (bolaSup != null) 
                {
                    estado = Estado.rodando;
                    this.transform.parent = bolaSup;
                    deslizar = false;

                    break;
                }

                if (engancharseInp == true && enganchePer == true && this.transform.position.y < limiteBal && sueleado == false && Physics.Raycast (this.transform.position, enganchePnt - this.transform.position, 
                    Vector3.Distance (this.transform.position, enganchePnt), capasSue, QueryTriggerInteraction.Ignore) == false) 
                {
                    //Vector3 direccion;

                    estado = Estado.balanceandose;
                    balanceo.twii.velocidad = impulsoBal / 2;
                    renderizadorLin.enabled = true;
                    deslizar = false;
                    //direccion = DireccionDeAngulo (this.transform.rotation.eulerAngles.y);
                    //adelanteBalXZ = new bool[] { Mathf.Sign (direccion.x) == 1, Mathf.Sign (direccion.z) == 1 };

                    balanceo.CambiarEnganche (enganchePnt);

                    break;
                }

                break;
            case Estado.trepando:
                if (escalarPos == false) 
                {
                    //Physics.IgnoreCollision (characterCtr, escenarioCol, false);

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
                    bolaScr.input = false;
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
                    direccionMov.y = alteraCue == false ? balanceo.twii.velocidad.y : 0;
                    alteraCue = false;
                }

                break;
        }
    }


    // Se le permite al jugador influir en el balanceo usando el input para impulsar al personaje (aunque a partir de cierta altura no es posible ya que sino el movimiento deja de ser realista). Además, rotamos al personaje de manera que su rotación se
    //alinee con el movimiento.
    private void Balancear ()
    {
        float engancheDstXZ = Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (enganchePnt.x, enganchePnt.z));

        if (cuerdaLonInp == false) 
        {
            //Vector2 balanceoAct;

            if (cuerdaLonInp != cuerdaLonInpUltFrm) 
            {
                balanceo.twii.velocidad.y = 0;
            }
            if (this.transform.position.y < limiteBal && (verticalInp != 0 || horizontalInp != 0)) 
            {
                Vector3 relativoCam = (camaraTrf.forward * verticalInp + camaraTrf.right * horizontalInp).normalized;

                balanceo.twii.velocidad += relativoCam;
                this.transform.rotation = Quaternion.Slerp (this.transform.rotation, Quaternion.Euler (0, Mathf.Atan2 (relativoCam.x, relativoCam.z) * Mathf.Rad2Deg + 90, 0), Time.deltaTime * rotacionVel);
            }

            //DecidirRotacionBalanceo (new Vector2 (balanceo.twii.velocidad.x, balanceo.twii.velocidad.z));
            /*balanceoAct = new Vector2 (balanceo.twii.velocidad.x, balanceo.twii.velocidad.z);
            print (balanceoAct.normalized);
            if (balanceoAct != Vector2.zero) 
            {
                if (Vector2.Angle (balanceoAct, balanceoVelUltFrm) < 170)
                {
                    this.transform.rotation = Quaternion.Slerp (this.transform.rotation, Quaternion.Euler (0, Mathf.Atan2(balanceo.twii.velocidad.x, balanceo.twii.velocidad.z) * Mathf.Rad2Deg + 90, 0), Time.deltaTime * rotacionVel);
                }
                balanceoVelUltFrm = balanceoAct;
            }*/
        }
        else 
        {
            if (paradoBal == false) 
            {
                Vector2 velocidadXZ = Vector2.ClampMagnitude (new Vector2 (balanceo.twii.velocidad.x, balanceo.twii.velocidad.z), engancheDstXZ * 2);

                balanceo.twii.velocidad = new Vector3 (velocidadXZ.x, balanceo.twii.velocidad.y, velocidadXZ.y);
            }
            else 
            {
                balanceo.twii.velocidad = new Vector3 (0, balanceo.twii.velocidad.y, 0);
                this.transform.position = Vector3.Lerp (this.transform.position, new Vector3 (enganchePnt.x, this.transform.position.y, enganchePnt.z), Time.deltaTime * interpolacionBalVel);
            }
        }
        if (alteraCue == false) 
        {
            this.transform.localPosition = balanceo.Mover (this.transform.localPosition, previaPos, Time.deltaTime);
            if (engancheDstXZ < balanceoDstMin) 
            {
                paradoBal = balanceo.twii.velocidad.magnitude < balanceoVelMin;
            }
        }
        else 
        {
            this.transform.localPosition = new Vector3 (this.transform.localPosition.x, balanceo.Mover(this.transform.localPosition, previaPos, Time.deltaTime).y, this.transform.localPosition.z);
            paradoBal = true;
        }
        
        /*if (cuerdaLonInp == false && this.transform.position.y < limiteBal && (verticalInp != 0 || horizontalInp != 0))
        {
            print("Fithos");
            balanceo.twii.velocidad += (camaraTrf.forward * verticalInp + camaraTrf.right * horizontalInp).normalized;
            paradoBal = false;

            Vector3 balanceoDir = new Vector3 (balanceo.twii.velocidad.x, 0, balanceo.twii.velocidad.z);
            Vector3 camaraDir = new Vector3 (this.transform.position.x - camaraTrf.position.x, 0, this.transform.position.z - camaraTrf.position.z);
            float anguloY1 = Quaternion.FromToRotation(balanceoDir, camaraDir).eulerAngles.y - 90;
            float anguloY2 = Quaternion.FromToRotation(-balanceoDir, camaraDir).eulerAngles.y - 90;

            rotacionBal = Quaternion.Euler (0, Mathf.Atan2 (balanceo.twii.velocidad.x, balanceo.twii.velocidad.z) * Mathf.Rad2Deg + 90, 0);
        }
        else 
        {
            if (alteraCue == false && Vector2.Distance (new Vector2 (this.transform.position.x, this.transform.position.z), new Vector2 (enganchePnt.x, enganchePnt.z)) < balanceoDstMin) 
            {
                paradoBal = balanceo.twii.velocidad.magnitude < balanceoVelMin;
            }
        }*/
        //Vector3 diferenciaCam = new Vector3 ();
        //this.transform.rotation = Quaternion.Slerp (this.transform.rotation, Quaternion.FromToRotation (new Vector3 (this.transform.position.x - camaraTrf.position.z, 0, this.transform.position.z - camaraTrf.position.z), -this.transform.right), 
        //Time.deltaTime * rotacionVel);
        /*print (balanceo.twii.velocidad.normalized == -direccionBalPrv.normalized);
        if (balanceo.twii.velocidad != Vector3.zero) 
        {
            direccionBalPrv = balanceo.twii.velocidad;
        }*/
    }


    // Movemos al personaje mientras camina o cae y lo rotamos acorde al movimiento, también tenemos en cuenta el impulso que puede estar recibiendo mientras cae tras balancearse.
    private void Caminar ()
    {
        CollisionFlags banderitas;

        balanceo.twii.velocidad = Vector3.zero;

        if ((verticalInp != 0 || horizontalInp != 0) && impulsoPar == Vector3.zero) 
        {
            Vector3 relativoCam = (camaraTrf.right * horizontalInp + camaraTrf.forward * verticalInp).normalized * movimientoVel;

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

        if (deslizar == true)
        {
            float inputX = direccionMov.x;
            float inputZ = direccionMov.z;

            direccionMov.x = (1 - normal.y) * normal.x * (1 - deslizFrc) * deslizVel;
            direccionMov.z = (1 - normal.y) * normal.z * (1 - deslizFrc) * deslizVel;
            if (Mathf.Sign (direccionMov.x) == Mathf.Sign (inputX))
            {
                direccionMov.x += inputX;
            }
            if (Mathf.Sign (direccionMov.z) == Mathf.Sign (inputZ))
            {
                direccionMov.z += inputZ;
            }
        }
        banderitas = personajeCtr.Move ((direccionMov + (pendiente == true ? pendienteFrz : Vector3.zero)) * Time.deltaTime);
        deslizar = ObtenerMenorPendiente () > personajeCtr.slopeLimit || (sueleado == false && banderitas != CollisionFlags.None && banderitas != CollisionFlags.Sides);

        if (personajeCtr.isGrounded == true) 
        {
            impulsoCai = Vector3.zero;
        }

        normales.Clear ();
    }


    // Hacemos que el personaje rote de acuerdo con la dirección hacia donde se mueve.
    private void Rotar () 
    {
        this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler (0, Mathf.Atan2 (direccionMov.x, direccionMov.z) * Mathf.Rad2Deg + 90, 0), rotacionVel * Time.deltaTime);
    }


    // Si el personaje está enganchado o trepando, no se aplica gravedad. En caso contrario, se le aplica una gravedad muy pequeña para mantenerlo cerca del suelo si lo está tocando. La gravedad se incrementa constantemente si, en cambio, el personaje
    //está en el aire.
    private void AplicarGravedad () 
    {
        if (estado != Estado.normal && estado != Estado.rodando)
        {
            direccionMov.y = 0;
        }
        else 
        {
            if (sueleado == true && saltado == false)
            {
                direccionMov.y = -10;
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
        if (saltoInp == true && sueleado == true) 
        {
            direccionMov.y = saltoVel;
            saltado = true;

            this.Invoke ("FinImpulso", 0.1f);
        }
    }


    // Usamos está función para poner la variable que indica que hemos saltado a "false" tras un pequeño intervalo de tiempo.
    private void FinImpulso () 
    {
        saltado = false;
    }


    // Se llama si el personaje se está balanceando y permite mostrar la cuerda que le mantiene colgado de manera correcta.
    private void CambiarPosicionCuerda () 
    {
        renderizadorLin.SetPosition (0, enganchePnt);
        renderizadorLin.SetPosition (1, cuerdaIniTrf.position);
    }


    // Twii salta hacia arriba y en la dirección opuesta a la pared por la que está trepando. 
    private void SaltarPared () 
    {
        if (saltoInp == true) 
        {
            impulsoPar = this.transform.right * saltoVel;
            impulsoPar.y = saltoVel;
        }
    }


    // Según el tipo de superficie escalable, el input en horizontal se usará para movernos en un eje u otro y el vertical será siempre en la misma dirección. En base a esto movemos al personaje correctamente para simular que trepa por la superfície.
    private void Trepar () 
    {
        Vector3 centro = personajeCtr.bounds.center;

        float diferencia;
        if (verticalInp != 0 || horizontalInp != 0)
        {
            Vector3 relativoCam;

            if (movimientoXEsc == true) 
            {
                relativoCam = (camaraTrf.right * horizontalInp + camaraTrf.up * verticalInp);
                if (Mathf.Abs (relativoCam.y) > Mathf.Abs (relativoCam.x)) 
                {
                    direccionMov.x = 0;
                    direccionMov.y = Mathf.Sign (relativoCam.y) * escaladaVel;
                    if ((direccionMov.y > 0 && centro.y > limitesEsc[2]) || (direccionMov.y < 0 && centro.y < limitesEsc[3]))
                    {
                        direccionMov.y = 0;
                    }
                }
                else 
                {
                    direccionMov.x = Mathf.Sign (relativoCam.x) / 2 * escaladaVel;
                    if ((direccionMov.x > 0 && centro.x > limitesEsc[0]) || (direccionMov.x < 0 && centro.x < limitesEsc[1]))
                    {
                        direccionMov.x = 0;
                    }
                    direccionMov.y = 0;
                }
            }
            else 
            {
                relativoCam = (camaraTrf.right * horizontalInp + camaraTrf.up * verticalInp);
                if (Mathf.Abs (relativoCam.y) > Mathf.Abs (relativoCam.z))
                {
                    direccionMov.y = Mathf.Sign (relativoCam.y) * escaladaVel;
                    if ((direccionMov.y > 0 && centro.y > limitesEsc[2]) || (direccionMov.y < 0 && centro.y < limitesEsc[3]))
                    {
                        direccionMov.y = 0;
                    }
                    direccionMov.z = 0;
                }
                else
                {
                    direccionMov.y = 0;
                    direccionMov.z = Mathf.Sign (relativoCam.z) / 2 * escaladaVel;
                    if ((direccionMov.z > 0 && centro.z > limitesEsc[0]) || (direccionMov.z < 0 && centro.z < limitesEsc[1]))
                    {
                        direccionMov.z = 0;
                    }
                }
            }
            /*if (Physics.CheckSphere (characterCtr.bounds.center + this.transform.right * 1.5f + direccionMov, characterCtr.bounds.extents.x, capasTrp, QueryTriggerInteraction.Collide) == false) 
            {
                direccionMov = Vector3.zero;
            }*/
        }
        if (movimientoXEsc == true) 
        {
            diferencia = escaladaXZ - centro.z;
            direccionMov.z = diferencia > 1.5f ? diferencia : 0;
        }
        else 
        {
            diferencia = escaladaXZ - centro.x;
            direccionMov.x = diferencia > 1.5f ? diferencia : 0;
        }
        direccionMov += impulsoPar;
        this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionEsc, Time.deltaTime * rotacionVel);

        personajeCtr.Move (direccionMov * Time.deltaTime);
    }


    // Miramos la posición hacia la que se mueve la bola para rotar al jugador de manera acorde, y también nos aseguramos de que su posición se corresponda con la parte superior de la bola.
    private void MantenerEquilibrio () 
    {
        if (verticalInp != 0 || horizontalInp != 0)
        {
            Vector3 relativoCam = (camaraTrf.right * horizontalInp + camaraTrf.forward * verticalInp);

            direccionMov.x = relativoCam.x;
            direccionMov.z = relativoCam.z;

            Rotar ();
        }

        this.transform.position = Vector3.Lerp (this.transform.position, bolaSup.position, movimientoVel * Time.deltaTime);
    }


    // Animamos al personaje de acuerdo con su situación actual.
    private void Animar () 
    {
        switch (estado) 
        {
            case Estado.normal:
                animador.SetBool ("escalando", false);
                animador.SetBool ("balanceandose", false);
                animador.SetBool ("moviendose", direccionMov.x != 0 || direccionMov.z != 0);
                animador.SetBool ("tocandoSuelo", sueleado == true && deslizar == false && saltado == false);

                break;
            case Estado.trepando:
                animador.SetBool ("escalando", true);
                animador.SetBool ("escaladaVertical", direccionMov.y != 0);
                animador.SetInteger ("escaladaHorizontal", (int) ((movimientoXEsc == true ? direccionMov.x : direccionMov.z) * 2));

                break;
            case Estado.balanceandose:
                animador.SetBool ("balanceandose", true);
                animador.SetBool ("moviendose", !paradoBal);
                animador.SetBool ("ajustarCuerda", alteraCue == true && verticalInp != 0);

                break;
            default:
                animador.SetBool ("escalando", false);
                animador.SetBool ("balanceandose", false);
                animador.SetBool ("moviendose", direccionMov.x != 0 || direccionMov.z != 0);
                animador.SetBool ("tocandoSuelo", true);

                break;
        }
    }


    // El modelo rota en Z de manera que esta rotación se corresponda con la animación actual.
    private void RotarModelo () 
    {
        modeloTrf.localRotation = Quaternion.Slerp (modeloTrf.localRotation, (sueleado == true || Estado.normal != estado) ? rotacionesMod[0] : rotacionesMod[1], Time.deltaTime * rotacionVel);
    }


    // Si el twii está colgado pero parado, se está pulsando el botón que permite variar la longitud de la cuerda y el input en vertical no es nulo, podemos hacer que este suba o baje por la misma.
    private void SubirOBajarPorCuerda () 
    {
        if (paradoBal == true && cuerdaLonInp == true) 
        {
            if (verticalInp != 0) 
            {
                float subida = Time.deltaTime * verticalInp * longitudCueVel;

                if (verticalInp == 1 ? (this.transform.position.y + subida < cuerdaLimSup) : (this.transform.position.y + subida > cuerdaLimInf)) 
                {
                    balanceo.cuerda.longitud -= subida;
                }
                else 
                {
                    verticalInp = 0;
                }
            }
            alteraCue = true;
        }
        else 
        {
            alteraCue = false;
        }
    }


    // .
    private bool EnPendiente ()
    {
        if (direccionMov.y > 0)
        {
            return false;
        }

        if (Physics.Raycast (personajeCtr.bounds.center + offsetCenCtr, Vector3.down, out RaycastHit datosRay, pendienteRayLon, capasSue, QueryTriggerInteraction.Ignore) == true)
        {
            normales.Add (datosRay.normal);

            return datosRay.normal != Vector3.up;
        }
        else
        {
            return false;
        }
    }


    // .
    private float ObtenerMenorPendiente () 
    {
        if (sueleado == true)
        {
            List<Vector3> ignorar = new List<Vector3> ();
            foreach (Vector3 v in normales)
            {
                if (Mathf.Abs (Vector3.Angle (Vector3.up, v) - 90) < 1)
                {
                    ignorar.Add (v);
                }
            }
            foreach (Vector3 v in ignorar)
            {
                normales.Remove (v);
            }
        }

        if (normales.Count != 0)
        {
            float angulo;

            float inclinacion = 90;

            foreach (Vector3 n in normales)
            {
                angulo = Vector3.Angle (Vector3.up, n);
                if (angulo < inclinacion)
                {
                    inclinacion = angulo;
                    normal = n;
                }
            }

            return inclinacion;
        }
        else
        {
            return 0;
        }
    }


    // .
    /*private void DecidirRotacionBalanceo (Vector2 velocidadXZ) 
    {
        if ((Mathf.Abs (velocidadXZ.x) > Mathf.Abs (velocidadXZ.y) && (Mathf.Sign (velocidadXZ.x) == 1) == adelanteBalXZ[0]) || (Mathf.Abs (velocidadXZ.y) > Mathf.Abs (velocidadXZ.x) && (Mathf.Sign (velocidadXZ.y) == 1) == adelanteBalXZ[1])) 
        {
            this.transform.rotation = Quaternion.Slerp (this.transform.rotation, Quaternion.Euler (0, Mathf.Atan2 (velocidadXZ.x, velocidadXZ.y) * Mathf.Rad2Deg + 90, 0), Time.deltaTime * rotacionVel);
        }
    }*/
}