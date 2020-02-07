
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


    // .
    private void Start ()
    {
        caer = false;
        characterCtr = this.GetComponent<CharacterController> ();
        gravedad = -11;
        fuerzaY = 0;
    }


    // .
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


    // .
    private void OnTriggerEnter (Collider other)
    {
        if (other.tag == "CaidaObjeto") 
        {
            caer = true;
        }
    }


    // .
    private void OnTriggerExit (Collider other)
    {
        if (other.tag == "CaidaObjeto")
        {
            caer = false;
        }
    }


    // .
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


    // .
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