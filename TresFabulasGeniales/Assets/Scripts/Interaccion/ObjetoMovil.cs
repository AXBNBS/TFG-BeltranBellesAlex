
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjetoMovil : MonoBehaviour
{
    public bool caer, bloqueado;

    [SerializeField] private List<Collider> triggers;
    [SerializeField] private bool[] movimientoX;
    [SerializeField] private bool rueda;
    private CharacterController characterCtr;
    private int gravedad, indiceTrg, fuerzaY, rotacionVel;
    private Empujar empujarScr;
    private Transform[] sueleadoresTrf;
    private Vector3[] sueleadoresOff;
    private Quaternion[] sueleadoresRot;


    // Inicialización de variables.
    private void Start ()
    {
        caer = false;
        characterCtr = this.GetComponent<CharacterController> ();
        gravedad = -11;
        fuerzaY = 0;
        rotacionVel = 50;
        empujarScr = GameObject.FindObjectOfType<Empujar> ();
        sueleadoresTrf = this.transform.GetChild(0).GetComponentsInChildren<Transform> ();
        sueleadoresOff = new Vector3[sueleadoresTrf.Length];
        sueleadoresOff[0] = sueleadoresTrf[0].position - this.transform.position;
        sueleadoresOff[1] = sueleadoresTrf[1].position - this.transform.position;
        sueleadoresRot = new Quaternion[sueleadoresTrf.Length];
        sueleadoresRot[0] = sueleadoresTrf[0].rotation;
        sueleadoresRot[1] = sueleadoresTrf[1].rotation;
    }


    // A cada frame, nos aseguramos de que los sueleadores sigan en la posición correcta respecto al tronco.
    private void Update ()
    {
        sueleadoresTrf[0].position = this.transform.position + sueleadoresOff[0];
        sueleadoresTrf[1].position = this.transform.position + sueleadoresOff[1];
        sueleadoresTrf[0].rotation = sueleadoresRot[0];
        sueleadoresTrf[1].rotation = sueleadoresRot[1];
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
        switch (other.tag) 
        {
            case "CaidaObjeto":
                foreach (Collider t in triggers)
                {
                    t.enabled = false;
                }
                caer = true;
                empujarScr.cercano = false;

                break;
            case "Jugador":
                if (other.GetComponent<Empujar> () == null) 
                {
                    bloqueado = true;
                }

                break;
        }
    }


    // Si el objeto sale de un trigger de caída, ponemos la variable que permite que caiga a "false".
    private void OnTriggerExit (Collider other)
    {
        switch (other.tag)
        {
            case "CaidaObjeto":
                caer = false;

                break;
            case "Jugador":
                if (other.GetComponent<Empujar> () == null)
                {
                    bloqueado = false;
                }

                break;
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
        characterCtr.Move (Time.deltaTime * movimiento);
        if (rueda == true && movimiento != Vector3.zero) 
        {
            if (movimientoX[indiceTrg] == true) 
            {
                this.transform.Rotate (movimiento.x > 0 ? new Vector3 (-rotacionVel, 0, 0) * Time.deltaTime : new Vector3 (+rotacionVel, 0, 0) * Time.deltaTime);
            }
            else 
            {
                this.transform.Rotate (movimiento.z > 0 ? new Vector3 (0, 0, -rotacionVel) * Time.deltaTime : new Vector3 (0, 0, +rotacionVel) * Time.deltaTime);
            }
        }
    }
}