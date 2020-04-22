
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SeguimientoCamara : MonoBehaviour
{
    public bool input, centrar, transicionando, desplazandose, cambioCmp;
    public Transform objetivo, detras;

    [SerializeField] private int movimientoVel, cambioPosVel, abajoLim, arribaLim, sensibilidad, centradoVel;
    [SerializeField] private LayerMask dialogoCap;
    private float ratonX, ratonY, stickX, stickY, finalX, finalY, rotacionX, rotacionY;
    private Quaternion rotacionObj, rotacionDlg;
    private bool dialogando;
    private Vector3 puntoMed;
    private Transform hijoTrf;


    // Inicialización de variables.
    private void Start ()
    {
        // ACTIVAR AL FINAL
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        centrar = true;
        transicionando = false;
        cambioCmp = true;
        dialogando = false;
        hijoTrf = this.transform.GetChild (0);
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
            //print ("Rotando alrededor del personaje.");
        }
        else 
        {
            if (dialogando == false) 
            {
                if (CambioDePersonajesYAgrupacion.instancia != null && this.transform.position == objetivo.position)
                {
                    cambioCmp = true;
                    desplazandose = false;

                    CambioDePersonajesYAgrupacion.instancia.PermitirInput ();
                    CambioDePersonajesYAgrupacion.instancia.ActivarIACombate ();
                    objetivo.parent.GetComponent<Salud>().InvulnerabilidadTemporal ();
                }
            }
            else
            {
                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionDlg, Time.deltaTime * centradoVel);
                //print ("Adaptándose a la conversación.");
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
                if (desplazandose == false) 
                {
                    this.transform.position = objetivo.position;
                    //print ("Siguiendo a mi objetivo.");
                }
                else 
                {
                    this.transform.position = Vector3.MoveTowards (this.transform.position, objetivo.position, Time.deltaTime * movimientoVel);
                    //print ("Yendo a por el otro personaje.");
                }
            }
            else 
            {
                this.transform.position = Vector3.MoveTowards (this.transform.position, puntoMed, Time.deltaTime * cambioPosVel);
                //print ("Yendo a por el punto medio: " + puntoMed);
            }
        } 
    }


    // La típica.
    /*private void OnDrawGizmos ()
    {
        if (objetivo != null) 
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine (puntoMed, puntoMed + (objetivo.right + objetivo.forward).normalized * 100);
            Gizmos.DrawLine (puntoMed, puntoMed + (objetivo.right - objetivo.forward).normalized * 100);
        }
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
    public void CalcularGiro (Vector3 npcPos) 
    {
        bool rayo1, rayo2;
        RaycastHit rayoDat1, rayoDat2;

        Vector3 diferencia1 = puntoMed - (puntoMed + (objetivo.right + objetivo.forward).normalized * 100);
        Vector3 diferencia2 = puntoMed - (puntoMed + (objetivo.right - objetivo.forward).normalized * 100);
        Vector3 hijoPosAct = hijoTrf.position;
        float rotacionX1 = Mathf.Atan2 (diferencia1.x, diferencia1.z) * Mathf.Rad2Deg + 90;
        float rotacionX2 = Mathf.Atan2 (diferencia2.x, diferencia2.z) * Mathf.Rad2Deg + 90;
        Quaternion rotacionAct = this.transform.rotation;
        //Transform clon = GameObject.Instantiate(this.gameObject).transform;
        //Transform clonHij = clon.GetChild (0);

        //print (hijoTrf.position);
        this.transform.rotation = Quaternion.Euler (0, rotacionX1, 0);
        //print (hijoTrf.position);
        rayo1 = Physics.Linecast (npcPos, hijoTrf.position, out rayoDat1, dialogoCap, QueryTriggerInteraction.Ignore);
        this.transform.rotation = Quaternion.Euler (0, rotacionX2, 0);
        //print (hijoTrf.position);
        rayo2 = Physics.Linecast (npcPos, hijoTrf.position, out rayoDat2, dialogoCap, QueryTriggerInteraction.Ignore);
        this.transform.rotation = rotacionAct;
        if (rayo1 == false && rayo2 == false)
        {
            rotacionX = Mathf.Abs (rotacionX1 - this.transform.rotation.eulerAngles.y) < Mathf.Abs (rotacionX2 - this.transform.rotation.eulerAngles.y) ? rotacionX1 : rotacionX2;
        }
        else 
        {
            if (rayo1 != rayo2)
            {
                rotacionX = rayo1 == false ? rotacionX1 : rotacionX2;
            }
            else 
            {
                rotacionX = rayoDat1.distance > rayoDat2.distance ? rotacionX1 : rotacionX2;
            }
        }
        rotacionY = 0;
        rotacionDlg = Quaternion.Euler (0, rotacionX, rotacionY);
    }
}