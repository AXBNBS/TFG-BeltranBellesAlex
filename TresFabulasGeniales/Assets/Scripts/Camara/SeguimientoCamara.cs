
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SeguimientoCamara : MonoBehaviour
{
    public bool input, centrar, transicionando;
    public Transform objetivo, detras;

    [SerializeField] private int movimientoVel, cambioPosVel, abajoLim, arribaLim, sensibilidad, centradoVel;
    private float ratonX, ratonY, stickX, stickY, finalX, finalY, rotacionX, rotacionY;
    private Quaternion rotacionObj, rotacionDlg;
    private bool dialogando;
    private Vector3 puntoMed;


    // Inicialización de variables.
    private void Start ()
    {
        // ACTIVAR AL FINAL
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        centrar = true;
        transicionando = false;
        dialogando = false;
    }


    // Si se ha de centrar la cámara, calculamos la diferencia entre la posición del objetivo y la que tendrá la cámara al centrarse respecto al mismo para determinar la rotación que esta ha de realizar. En el caso contrario, rotamos la cámara de
    //manera acorde con el input recibido por parte del usuario, ya sea mediande el ratón o el stick derecho del mando, teniendo el cuenta los límites superior e inferior de la cámara.
    private void Update ()
    {
        if (input == true && centrar == false && Input.GetButtonDown ("Centrar cámara") == true) 
        {
            centrar = true;
        }

        if (input == true)
        {
            if (centrar == true)
            {
                Vector3 diferencia = objetivo.position - detras.position;

                rotacionX = Mathf.Atan2 (diferencia.x, diferencia.z) * Mathf.Rad2Deg + 90;
                rotacionY = 0;
                rotacionObj = Quaternion.Euler (0, rotacionX, rotacionY);
                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionObj, Time.deltaTime * centradoVel);
                if (Quaternion.Angle (this.transform.rotation, rotacionObj) < 1)
                {
                    centrar = false;
                }
            }
            else
            {
                float suavizado = sensibilidad * Time.deltaTime;
                float ratX = Input.GetAxis ("Cámara X ratón");
                float ratY = Input.GetAxis ("Cámara Y ratón");
                float stkX = Input.GetAxis ("Cámara X joystick");
                float stkY = Input.GetAxis ("Cámara Y joystick");

                ratonX = Mathf.Abs (ratX) > 0.1f ? ratX : 0;
                ratonY = Mathf.Abs (ratY) > 0.1f ? ratY : 0;
                stickX = Mathf.Abs (stkX) > 0.1f ? stkX : 0;
                stickY = Mathf.Abs (stkY) > 0.1f ? stkY : 0;
                finalX = ratonX + stickX;
                finalY = ratonY + stickY;
                rotacionX += finalX * suavizado;
                rotacionY += finalY * suavizado;
                rotacionY = Mathf.Clamp (rotacionY, abajoLim, arribaLim);
                rotacionObj = Quaternion.Euler (0, rotacionX, rotacionY);
                this.transform.rotation = rotacionObj;
            }
        }
        else 
        {
            if (CambioDePersonajesYAgrupacion.instancia != null && this.transform.position == objetivo.position) 
            {
                CambioDePersonajesYAgrupacion.instancia.PermitirInput ();
            }

            if (dialogando == true) 
            {
                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionDlg, Time.deltaTime * centradoVel);
            }
        }
    }


    // Seguimiento del objetivo de la cámara.
    private void LateUpdate ()
    {
        if (transicionando == false) 
        {
            if (dialogando == false) 
            {
                this.transform.position = Vector3.MoveTowards (this.transform.position, objetivo.position, Time.deltaTime * movimientoVel);
            }
            else 
            {
                this.transform.position = Vector3.MoveTowards (this.transform.position, puntoMed, Time.deltaTime * cambioPosVel);
            }
        } 
    }


    // La típica.
    /*private void OnDrawGizmos ()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine (puntoMed, puntoMed + (objetivo.right - objetivo.forward).normalized * 5);
        Gizmos.DrawLine (puntoMed, puntoMed - (objetivo.right + objetivo.forward).normalized * 5);
    }*/


    // .
    public void PuntoMedioDialogo (bool activar, Vector3 personaje, Vector3 npc) 
    {
        dialogando = activar;
        if (activar == true)
        {
            puntoMed = (npc - personaje) / 2 + personaje;
            puntoMed.y = npc.y;
        }
    }


    // .
    public void CalcularGiro () 
    {
        Vector3 diferencia1 = puntoMed - (puntoMed + (objetivo.right - objetivo.forward).normalized * 5);
        Vector3 diferencia2 = puntoMed - (puntoMed - (objetivo.right + objetivo.forward).normalized * 5);
        float rotacion1 = Mathf.Atan2 (diferencia1.x, diferencia1.z) * Mathf.Rad2Deg + 90;
        float rotacion2 = Mathf.Atan2 (diferencia2.x, diferencia2.z) * Mathf.Rad2Deg + 90;

        rotacionX = Mathf.Abs (rotacion1 - this.transform.rotation.eulerAngles.y) < Mathf.Abs (rotacion2 - this.transform.rotation.eulerAngles.y) ? rotacion1 : rotacion2;
        rotacionY = 0;
        rotacionDlg = Quaternion.Euler (0, rotacionX, rotacionY);
    }
}