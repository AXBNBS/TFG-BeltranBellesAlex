
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SeguimientoCamara : MonoBehaviour
{
    public bool input, centrar;
    [HideInInspector] public Transform objetivo, detras;

    [SerializeField] private int movimientoVel, abajoLim, arribaLim, sensibilidad;
    private float ratonX, ratonY, stickX, stickY, finalX, finalY, rotacionX, rotacionY;
    private Quaternion rotacionLoc;


    // Inicialización de variables.
    private void Start ()
    {
        // ACTIVAR AL FINAL
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        centrar = false;
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

                rotacionX = Mathf.Atan2(diferencia.x, diferencia.z) * Mathf.Rad2Deg + 90;
                rotacionY = 0;
                rotacionLoc = Quaternion.Euler (rotacionX, rotacionY, 0);
                this.transform.rotation = Quaternion.Lerp (this.transform.rotation, rotacionLoc, Time.deltaTime);
                centrar = false;
            }
            else 
            {
                float suavizado = sensibilidad * Time.deltaTime;

                ratonX = Mathf.Abs (Input.GetAxis ("Cámara X ratón")) > 0.1f ? Input.GetAxis ("Cámara X ratón") : 0;
                ratonY = Mathf.Abs (Input.GetAxis ("Cámara Y ratón")) > 0.1f ? Input.GetAxis ("Cámara Y ratón") : 0;
                stickX = Mathf.Abs (Input.GetAxis ("Cámara X joystick")) > 0.1f ? Input.GetAxis ("Cámara X joystick") : 0;
                stickY = Mathf.Abs (Input.GetAxis ("Cámara Y joystick")) > 0.1f ? Input.GetAxis ("Cámara Y joystick") : 0;
                finalX = ratonX + stickX;
                finalY = ratonY + stickY;
                rotacionX += finalX * suavizado;
                rotacionY += finalY * suavizado;
                rotacionY = Mathf.Clamp (rotacionY, abajoLim, arribaLim);
                rotacionLoc = Quaternion.Euler (0, rotacionX, rotacionY);
                this.transform.rotation = rotacionLoc;
            }
        }
    }


    // Seguimiento del objetivo de la cámara.
    private void LateUpdate ()
    {
        this.transform.position = Vector3.MoveTowards (this.transform.position, objetivo.position, movimientoVel * Time.deltaTime);
    }
}