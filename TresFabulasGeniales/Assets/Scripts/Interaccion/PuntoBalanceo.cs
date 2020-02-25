﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PuntoBalanceo : MonoBehaviour
{
    [SerializeField] private int offsetY;
    [SerializeField] private bool movimientoX;
    private float limiteY;
    private MovimientoHistoria3 jugador;

    
    // Inicializamos una unidad de variable.
    private void Start ()
    {
        limiteY = this.transform.position.y + offsetY;
        jugador = GameObject.FindObjectOfType<MovimientoHistoria3> ();
    }


    // Para ver mejor a donde llega el límite.
    private void OnDrawGizmos ()
    {
        Gizmos.DrawLine (this.transform.position, this.transform.position + Vector3.up * offsetY);
    }


    // El jugador recibe información sobre dónde está el punto al que engancharse, en que eje puede balancearse y a partir de que límite en Y no puede impulsarse más.
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            jugador.enganchePnt = this.transform.position;
            jugador.movimientoXBal = movimientoX;
            jugador.limiteBal = limiteY;
        }
    }


    // El punto de enganche más cercano pasa a estar en una posición no permitida, y por tanto el jugador no puede colgarse.
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("Jugador") == true) 
        {
            jugador.enganchePnt = Vector3.zero;
        }
    }
}