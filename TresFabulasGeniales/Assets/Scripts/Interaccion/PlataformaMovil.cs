
using UnityEngine;



public class PlataformaMovil : MonoBehaviour
{
    public Vector3 movimiento;

    [SerializeField] private int ejeLib;
    private CharacterController avatarPerCtr;
    private MovimientoHistoria2 avatarMovScr;
    private Rigidbody cuerpoRig;


    // .
    private void Start ()
    {
        cuerpoRig = this.GetComponent<Rigidbody> ();

        //this.Invoke ("LiberarEje", 1);
    }


    // Si un avatar está sobre la plataforma, obtenemos la referencia a su script de movimiento.
    private void OnTriggerEnter (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true)
        {
            //avatarPerCtr = other.GetComponent<CharacterController> ();
            avatarMovScr = other.GetComponent<MovimientoHistoria2> ();
            other.transform.parent = this.transform;
            avatarMovScr.plataformaAbj = true;
            print ("Gato Sánchez.");
        }
    }


    // Siempre que un avatar se mantenga dentro del trigger de la plataforma, este recibirá el movimiento aplicado a la misma.
    /*private void OnTriggerStay (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true) 
        {
            avatarMovScr.plataformaAbj = true;
        }
    }*/


    // Al salir del trigger de la plataforma, nos aseguramos de que no se aplique movimiento alguno por parte de la misma al avatar.
    private void OnTriggerExit (Collider other)
    {
        if (other.isTrigger == false && other.CompareTag ("Jugador") == true)
        {
            //avatarMov.plataformaMov = Vector3.zero;
            //avatarMovScr.plataformaAbj = false;
            other.transform.parent = null;
            avatarMovScr.plataformaAbj = false;
            print ("Perro Sánchez.");
        }
    }


    // .
    private void LiberarEje () 
    {
        switch (ejeLib)
        {
            case 1:
                cuerpoRig.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

                break;
            case 0:
                cuerpoRig.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

                break;
            default:
                cuerpoRig.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

                break;
        }
    }
}