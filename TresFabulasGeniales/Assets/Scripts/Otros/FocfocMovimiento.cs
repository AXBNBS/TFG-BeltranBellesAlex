
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FocfocMovimiento : MonoBehaviour
{
    [SerializeField] private int rotacionRan, velocidadRot, velocidadMov;
    [SerializeField] private Transform puntosTrf;
    private Quaternion extremoRot1, extremoRot2;
    private bool sentidoRot;
    private Vector3[] puntosMov;
    private int objetivoMov;


    // .
    private void Start ()
    {
        extremoRot1 = Quaternion.Euler (new Vector3 (0, +rotacionRan, 0));
        extremoRot2 = Quaternion.Euler (new Vector3 (0, -rotacionRan, 0));
        sentidoRot = true;
        puntosMov = new Vector3[puntosTrf.childCount];
        for (int p = 0; p < puntosMov.Length; p += 1) 
        {
            puntosMov[p] = puntosTrf.GetChild(p).position;
        }
        objetivoMov = 0;
    }


    // .
    private void Update ()
    {
        Rotar ();
        Mover ();
    }


    // .
    private void Rotar () 
    {
        this.transform.localRotation = Quaternion.Lerp (this.transform.localRotation, sentidoRot == true ? extremoRot1 : extremoRot2, Time.deltaTime * velocidadRot);
        if (Quaternion.Angle (this.transform.localRotation, sentidoRot == true ? extremoRot1 : extremoRot2) < 1)
        {
            sentidoRot = !sentidoRot;
        }
    }


    // .
    private void Mover () 
    {
        this.transform.position = Vector3.Lerp (this.transform.position, puntosMov[objetivoMov], Time.deltaTime * velocidadMov);
        if (Vector3.Distance (this.transform.position, puntosMov[objetivoMov]) < 0.1f) 
        {
            objetivoMov = objetivoMov != puntosMov.Length - 1 ? objetivoMov + 1 : 0;
        }
    }
}