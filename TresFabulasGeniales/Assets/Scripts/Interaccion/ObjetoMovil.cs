
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjetoMovil : MonoBehaviour
{
    public bool caer;

    [SerializeField] private List<Collider> triggers;
    [SerializeField] private bool[] movimientoX;
    [SerializeField] private bool rueda;
    [SerializeField] private int rotacionVel;
    private CharacterController characterCtr;
    private int gravedad, indiceTrg, fuerzaY;
    private Empujar empujarScr;


    // Inicialización de variables.
    private void Start ()
    {
        caer = false;
        characterCtr = this.GetComponent<CharacterController> ();
        gravedad = -11;
        fuerzaY = 0;
        empujarScr = GameObject.FindObjectOfType<Empujar> ();
    }


    // Si el objeto ha de caer, le aplicamos gravedad y lo movemos de acuerdo a esto; en caso contrario simplemente nos aseguramos de que la fuerza en Y es 0.
    private void FixedUpdate ()
    {
        if (caer == true)
        {
            fuerzaY += gravedad;

            characterCtr.Move (new Vector3 (0, fuerzaY, 0) * Time.deltaTime);
        }
        else 
        {
            fuerzaY = 0;
        }
    }


    // Si el objeto entra en un trigger de caída, ponemos la variable que permite que caiga a "true".
    private void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("CaidaObjeto") == true) 
        {
            foreach (Collider t in triggers)
            {
                t.enabled = false;
            }
            caer = true;
            empujarScr.cercano = false;
        }
    }


    // Si el objeto sale de un trigger de caída, ponemos la variable que permite que caiga a "false".
    private void OnTriggerExit (Collider other)
    {
        if (other.CompareTag ("CaidaObjeto") == true)
        {
            caer = false;
        }
    }


    // Recibimos un trigger concreto del objeto y, tras localizarlo en el mismo, devolvemos una variable que indica si este objeto se puede mover en el eje X o en el Z de acuerdo con los parámetros que hemos establecido previamente.
    public bool EjeDeTrigger (Collider trigger) 
    {
        bool resultado = true;

        for (int t = 0; t < triggers.Count; t += 1) 
        {
            if (triggers[t] == trigger) 
            {
                resultado = movimientoX[t];
                indiceTrg = t;

                break;
            }
        }

        return resultado;
    }


    // Movemos el objeto de acuerdo con el empuje aplicado con el personaje, si este objeto ha de rotar al moverlo tenemos esto en cuenta y lo rotamos en el eje y sentido correcto.
    public void Mover (Vector3 movimiento) 
    {
        movimiento.y = 0;

        characterCtr.Move (movimiento);

        if (rueda == true && movimiento != Vector3.zero) 
        {
            float rotacion = rotacionVel * Time.deltaTime;

            if (movimientoX[indiceTrg] == true) 
            {
                this.transform.rotation = movimiento.x > 0 ? Quaternion.Euler (this.transform.rotation.eulerAngles.x - rotacion, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z) :
                    Quaternion.Euler (this.transform.rotation.eulerAngles.x + rotacion, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);
            }
            else 
            {
                this.transform.rotation = movimiento.z > 0 ? Quaternion.Euler (this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z - rotacion) :
                    Quaternion.Euler (this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z + rotacion);
            }
        }
    }
}