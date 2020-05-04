
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Morena : MonoBehaviour
{
    public enum Estado { saliendo, entrando, escondida };
    public Estado estado;

    [SerializeField] private int distanciaObj;
    [SerializeField] private Vector3 inicioPos;
    [SerializeField] private float escondidaTmp;
    private float velocidad, saliendoTmp, pasadoTmp;
    private CapsuleCollider capsulaCol;
    private Vector3 desplazamiento, offsetPos;


    // Inicialización de variables.
    private void Start ()
    {
        saliendoTmp = escondidaTmp / 2;
        velocidad = distanciaObj / saliendoTmp;
        capsulaCol = this.GetComponentInChildren<CapsuleCollider> ();
        desplazamiento = capsulaCol.transform.TransformDirection(Vector3.up).normalized;
        offsetPos = this.transform.parent.position;
        this.transform.position = offsetPos + inicioPos;
    }


    // Si ha pasado el suficiente tiempo y nos encontramos en el estado escondido, cambiamos el estado de la morena. En cambio, si nos encontramos en alguno de los otros 2 estados, aplicamos el desplazamiento que corresponda antes de comprobar 
    //también si es necesario cambiar de estado.
    private void Update ()
    {
        pasadoTmp += Time.deltaTime;
        if (Estado.escondida == estado) 
        {
            if (pasadoTmp > escondidaTmp)
            {
                CambiarEstado ();
            }
        }
        else 
        {
            this.transform.Translate ((Estado.saliendo == estado ? desplazamiento : -desplazamiento) * Time.deltaTime * velocidad, Space.World);
            if (pasadoTmp > saliendoTmp) 
            {
                CambiarEstado ();
            }
        }
    }


    // Pal debug.
    private void OnDrawGizmosSelected ()
    {
        //if (Vector3.zero != desplazamiento) 
        //{
            Gizmos.color = Color.red;

            Gizmos.DrawRay (inicioPos, this.GetComponentInChildren<CapsuleCollider>().transform.TransformDirection(Vector3.up).normalized * distanciaObj);
        //}
    }


    // Cuando la morena toca al jugador, este recibe daños y las colisiones con el mismo se ignoran durante un tiempo.
    private void OnCollisionEnter (Collision collision)
    {
        if (collision.transform.CompareTag ("Jugador") == true) 
        {
            Physics.IgnoreCollision (collision.collider, capsulaCol, true);
            collision.transform.GetComponent<Salud>().RecibirDanyo (Vector3.zero);
            this.StartCoroutine ("ColisionarDeNuevo", collision.collider);
        }
    }


    // Cada vez que pasa un cierto tiempo, cambiamos de un estado al siguiente. En el caso de pasar al estado en el que la morena se esconde, reseteamos la posición de la misma.
    private void CambiarEstado () 
    {
        pasadoTmp = 0;
        switch (estado) 
        {
            case Estado.escondida:
                estado = Estado.saliendo;

                break;
            case Estado.saliendo:
                estado = Estado.entrando;

                break;
            default:
                estado = Estado.escondida;
                this.transform.position = offsetPos + inicioPos;

                break;
        }
    }


    // Un poco después de haber dañado al jugador, volvemos a permitir las colisiones con el mismo.
    private IEnumerator ColisionarDeNuevo (Collider avatarCol)
    {
        yield return new WaitForSeconds (1);

        Physics.IgnoreCollision (capsulaCol, avatarCol, false);
    }
}