using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cadena : MonoBehaviour
{
    [SerializeField] private int balanceoFrz;
    [SerializeField] private Rigidbody cuerdaFin;
    private Transform camaraTrf;

    
    // .
    private void Start ()
    {
        camaraTrf = GameObject.FindGameObjectWithTag("CamaraPrincipal").transform;
    }


    // .
    private void Update ()
    {
        float horizontalInp = Input.GetAxis ("Movimiento horizontal") * balanceoFrz;
        float verticalInp = Input.GetAxis ("Movimiento vertical") * balanceoFrz;

        cuerdaFin.AddForce (verticalInp * camaraTrf.forward, ForceMode.Acceleration);
        cuerdaFin.AddForce (horizontalInp * camaraTrf.right, ForceMode.Acceleration);
    }
}