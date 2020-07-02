
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;



public class Menu : MonoBehaviour
{
    public static Menu instancia;

    [SerializeField] private string tituloAlt;
    private Fundido fundidoScr;


    // Inicialización de variables y desactivación del menú y cambios en sus textos si corresponde.
    private void Start ()
    {
        if (instancia == null) 
        {
            instancia = this;
        }
        fundidoScr = Fundido.instancia;
        if (AlmacenDatos.instancia.menuEsc != fundidoScr.escena.name) 
        {
            instancia.transform.GetChild(0).GetComponent<Text>().text = tituloAlt;

            instancia.MenuVisible (false);
        }
    }


    // Vamos a la siguiente escena o realizamos un fundido para salir del menú de pausa, dependiendo de si estamos en la escena inicial u otra.
    public void Jugar () 
    {
        if (Fundido.instancia.animando == false) 
        {
            if (AlmacenDatos.instancia.menuEsc != fundidoScr.escena.name)
            {
                fundidoScr.FundidoPausa ();
            }
            else
            {
                fundidoScr.FundidoAEscena (fundidoScr.escena.buildIndex + 1);
            }
        }
    }


    // Se cierra el juego.
    public void Salir () 
    {
        Application.Quit ();
    }


    // Se hace visible el menú o no según corresponda, también reseteamos el sistema de eventos para asegurarnos de que el primer botón del menú sea siempre el seleccionado por defecto.
    public void MenuVisible (bool visible) 
    {
        this.gameObject.SetActive (visible);
        if (visible == false) 
        {
            EventSystem.current.SetSelectedGameObject (null);
        }
        else 
        {
            EventSystem.current.SetSelectedGameObject (this.GetComponentInChildren<Button>().gameObject, null);
        }
    }
}