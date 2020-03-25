
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Salud : MonoBehaviour
{
    public bool aturdido;

    [SerializeField] private float salud, invulnerabilidadTmp;
    [SerializeField] private int aturdimientoVelY;
    private bool devolverInp, invulnerable;
    private float movimientoY, aturdimientoY; 
    private int gravedad;
    private Animator animador;
    private MovimientoHistoria2 movimientoScr;
    private Ataque ataqueScr;
    private Empujar empujeScr;
    private SeguimientoCamara camaraScr;
    private CharacterController personajeCtr;

    
    // Inicialización de variables.
    private void Start ()
    {
        aturdido = false;
        devolverInp = false;
        invulnerable = false;
        gravedad = -10;
        animador = this.GetComponentInChildren<Animator> ();
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        ataqueScr = this.GetComponent<Ataque> ();
        empujeScr = this.GetComponent<Empujar> ();
        camaraScr = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform.parent.GetComponent<SeguimientoCamara> ();
        personajeCtr = this.GetComponent<CharacterController> ();
    }


    // Si hemos de devolver el input al jugador y este ha salido del estado de aturdimiento, llamamos a la función que se encarga de devolver dicho input.
    private void Update ()
    {
        if (devolverInp == true) 
        {
            if (animador.GetCurrentAnimatorStateInfo(0).IsTag ("Aturdimiento") == false && animador.GetAnimatorTransitionInfo(0).IsName ("AnyState -> RecibirDaño") == false)
            {
                aturdido = false;

                if (CambioDePersonajesYAgrupacion.instancia.ActivarInputAutorizado (this) == true)
                {
                    ControlarInput (true);
                }
                InvulnerabilidadTemporal ();
            }
        }
    }


    // .
    private void OnControllerColliderHit (ControllerColliderHit hit)
    {
        /*if (this.name == "Abedul" && hit.transform.CompareTag ("Enemigo") == true) 
        {
            print (hit.transform.name);
        }*/
        /*if (aturdido == true && hit.transform.CompareTag ("Enemigo") == true) 
        {
            hit.transform.Translate (new Vector3(-hit.normal.x, 0, -hit.normal.z).normalized);
        }*/
    }


    // Si el jugador no está aturdido, pierde salud si está siendo controlado y no puede moverse durante unos pocos segundos debido al aturdimiento, también activamos una animación que indica que ha recibido daño.
    public void RecibirDanyo () 
    {
        if (invulnerable == false && camaraScr.cambioCmp == true && animador.GetCurrentAnimatorStateInfo(0).IsTag ("Aturdimiento") == false)
        {
            if (movimientoScr.input == true)
            {
                MoverEnY ();

                salud -= 1;
            }
            aturdido = true;

            ControlarInput (false);
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


    // Se activa la animación de haber sido dañado.
    private void Animar (bool aturdido) 
    {
        animador.SetBool ("aturdido", aturdido);
    }


    // Activamos o desactivamos todo el input del jugador.
    private void ControlarInput (bool activar) 
    {
        movimientoScr.input = activar;
        ataqueScr.input = activar;
        if (empujeScr != null)
        {
            empujeScr.input = activar;
        }
        devolverInp = !activar;
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
    }


    // Si el personaje está aturdido, lo mueve ligeramente en el eje Y.
    private void MoverEnY () 
    {
        movimientoScr.movimiento.y = aturdimientoVelY;

        personajeCtr.Move (new Vector3 (0, aturdimientoVelY, 0) * Time.deltaTime);
    }
}