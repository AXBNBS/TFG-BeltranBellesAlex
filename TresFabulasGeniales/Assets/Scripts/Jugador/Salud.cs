
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class Salud : MonoBehaviour
{
    public bool aturdido;

    [SerializeField] private float salud, invulnerabilidadTmp;
    [SerializeField] private int aturdimientoVelY;
    private bool devolverInp, invulnerable;
    private Animator animador;
    private MovimientoHistoria2 movimientoScr;
    private Ataque ataqueScr;
    private Empujar empujeScr;
    private SeguimientoCamara camaraScr;
    private CharacterController personajeCtr;
    private Transform saludPanTrf;

    
    // Inicialización de variables.
    private void Start ()
    {
        aturdido = false;
        devolverInp = false;
        invulnerable = false;
        animador = this.transform.GetChild(6).GetComponent<Animator> ();
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        ataqueScr = this.GetComponent<Ataque> ();
        empujeScr = this.GetComponent<Empujar> ();
        camaraScr = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform.parent.GetComponent<SeguimientoCamara> ();
        personajeCtr = this.GetComponent<CharacterController> ();
        saludPanTrf = Fundido.instancia.saludPan.transform;

        saludPanTrf.gameObject.SetActive (true);
        ActualizarHUD ();
    }


    // Si hemos de devolver el input al jugador y este ha salido del estado de aturdimiento, llamamos a la función que se encarga de devolver dicho input.
    private void Update ()
    {
        if (devolverInp == true) 
        {
            aturdido = false;
            devolverInp = false;

            if (CambioDePersonajesYAgrupacion.instancia.ActivarInputAutorizado (this) == true)
            {
                ControlarInput (true);
            }
            InvulnerabilidadTemporal ();
        }
    }


    // Más debug.
    /*private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        if (this.name == "Abedul" && hit.transform.CompareTag ("Enemigo") == true) 
        {
            print (hit.transform.name);
        }
        if (aturdido == true && hit.transform.CompareTag ("Enemigo") == true) 
        {
            hit.transform.Translate (new Vector3(-hit.normal.x, 0, -hit.normal.z).normalized);
        }
    }*/


    // Al entrar en un trigger asociado a la natilla o a una caída, el personaje muere directamente.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Natilla") == true || other.CompareTag ("Caida") == true) 
        {
            this.StartCoroutine (Muerte (other.tag));
        }
    }


    // Si el jugador no está aturdido, pierde salud si está siendo controlado y no puede moverse durante unos pocos segundos debido al aturdimiento, también activamos una animación que indica que ha recibido daño.
    public void RecibirDanyo (Vector3 impulso, string causa) 
    {
        if (invulnerable == false && aturdido == false && camaraScr.cambioCmp == true && animador.GetCurrentAnimatorStateInfo(0).IsTag ("Aturdimiento") == false)
        {
            //print (this.name + ": IMPACTO");
            if (movimientoScr.input == true)
            {
                MoverEnY ();
                ControlarInput (false);

                salud -= 1;

                ActualizarHUD ();
                if (salud < 0) 
                {
                    this.StartCoroutine (Muerte (causa));
                }
            }
            aturdido = true;
            movimientoScr.aturdimientoImp = (movimientoScr.input == true && movimientoScr.sueleado == true) ? new Vector3(impulso.x, 0, impulso.z).normalized : Vector3.zero;

            Animar (true);
            this.Invoke ("PararAnimacion", 0.3f);
        }
    }


    // El personaje es invulnerable durante un pequeño periodo de tiempo.
    public void InvulnerabilidadTemporal () 
    {
        invulnerable = true;

        this.Invoke ("InvulnerabilidadPerdida", invulnerabilidadTmp);
    }


    // Actualizamos las barras de salud del HUD de acuerdo a la situación actual del personaje.
    public void ActualizarHUD ()
    {
        for (int b = 0; b < saludPanTrf.childCount; b += 1)
        {
            saludPanTrf.GetChild(b).GetComponentInChildren<Image>().enabled = salud > b;
        }
    }


    // Se activa la animación de haber sido dañado.
    private void Animar (bool controlador) 
    {
        animador.SetBool ("aturdido", controlador);
    }


    // Activamos o desactivamos todo el input del jugador.
    private void ControlarInput (bool activar) 
    {
        CambioDePersonajesYAgrupacion.instancia.input = activar;
        ataqueScr.input = activar;
        if (empujeScr != null)
        {
            empujeScr.input = activar;
        }
    }


    // Invocada para desactivar la invulnerabilidad tras poco tiempo.
    private void InvulnerabilidadPerdida () 
    {
        invulnerable = false;
    }


    // El personaje deja de mostrar la animación de estar aturdido.
    private void PararAnimacion () 
    {
        Animar (false);

        devolverInp = true;
    }


    // Si el personaje está aturdido, lo mueve ligeramente en el eje Y.
    private void MoverEnY () 
    {
        movimientoScr.movimiento.y = aturdimientoVelY;

        personajeCtr.Move (new Vector3 (0, aturdimientoVelY, 0) * Time.deltaTime);
    }


    // Si el avatar controlado acaba de ser derrotado, esperamos brevemente, paramos el tiempo y lo mandamos a la escena de muerte. Almacenaremos también datos como la escena a la que se regresará, el nombre del avatar y la causa de su muerte.
    private IEnumerator Muerte (string causa) 
    {
        yield return new WaitForSeconds (0.2f);

        Time.timeScale = 0;
        AlmacenDatos.instancia.muerte = true;
        AlmacenDatos.instancia.regresarA = SceneManager.GetActiveScene().buildIndex;
        AlmacenDatos.instancia.avatarMue = this.name;
        AlmacenDatos.instancia.causaMue = causa;

        Fundido.instancia.FundidoAEscena (0);
    }
}