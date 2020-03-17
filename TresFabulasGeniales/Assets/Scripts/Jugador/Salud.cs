
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Salud : MonoBehaviour
{
    public bool aturdido;

    [SerializeField] private float salud, invulnerabilidadTmp;
    private bool devolverInp, invulnerable;
    private Animator animador;
    private MovimientoHistoria2 movimientoScr;
    private Ataque ataqueScr;
    private Empujar empujeScr;

    
    // Inicialización de variables.
    private void Start ()
    {
        aturdido = false;
        devolverInp = false;
        invulnerable = false;
        animador = this.GetComponentInChildren<Animator> ();
        movimientoScr = this.GetComponent<MovimientoHistoria2> ();
        ataqueScr = this.GetComponent<Ataque> ();
        empujeScr = this.GetComponent<Empujar> ();
    }


    // Si hemos de devolver el input al jugador y este ha salido del estado de aturdimiento, llamamos a la función que se encarga de devolver dicho input.
    private void Update ()
    {
        if (devolverInp == true) 
        {
            if (animador.GetCurrentAnimatorStateInfo(0).IsTag ("Aturdimiento") == false && animador.GetAnimatorTransitionInfo(0).IsName ("AnyState -> RecibirDaño") == false)
            {
                invulnerable = true;
                aturdido = false;

                this.Invoke ("InvulnerabilidadPerdida", invulnerabilidadTmp);
                ControlarInput (true);
            }
        }
    }


    // Si el jugador no está aturdido, pierde salud si está siendo controlado y no puede moverse durante unos pocos segundos debido al aturdimiento, también activamos una animación que indica que ha recibido daño.
    public void RecibirDanyo () 
    {
        if (invulnerable == false && animador.GetCurrentAnimatorStateInfo(0).IsTag ("Aturdimiento") == false)
        {
            if (movimientoScr.input == true)
            {
                salud -= 1;
            }
            aturdido = true;

            ControlarInput (false);
            Animar (true);
            this.Invoke ("PararAnimacion", 0.5f);
        }
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


    // Usada como corutina para desactivar la invulnerabilidad tras poco tiempo.
    private void InvulnerabilidadPerdida () 
    {
        invulnerable = false;
    }


    // El personaje deja de mostrar la animación de estar aturdido.
    private void PararAnimacion () 
    {
        Animar (false);
    }
}