
using UnityEngine;



public class SelectorDeTexto : MonoBehaviour
{
    [SerializeField] private string[] ranasVio, naifesVio, morenasVio, natillaVio, caidaVio, ranasAbe, naifesAbe, morenasAbe, natillaAbe, caidaAbe, caidaTwi;
    [SerializeField] private Hablar hablarScr;


    // Selecciona el diálogo que corresponda en función del avatar muerto y las circumstancias de su muerte.
    private void Start ()
    {
        SeleccionarDialogo (AlmacenDatos.instancia.avatarMue, AlmacenDatos.instancia.causaMue);
    }


    // Obtenemos primero el personaje derrotado y, si fuese necesario, examinamos también la causa de muerte del mismo para decidir que texto mostrar.
    private void SeleccionarDialogo (string personaje, string causa) 
    {
        if (personaje != "Twii") 
        {
            if (personaje == "Violeta") 
            {
                switch (causa) 
                {
                    case "Rana":
                        hablarScr.texto = ranasVio;

                        break;
                    case "Naife":
                        hablarScr.texto = naifesVio;

                        break;
                    case "Natilla":
                        hablarScr.texto = natillaVio;

                        break;
                    case "Caida":
                        hablarScr.texto = caidaVio;

                        break;
                    default:
                        hablarScr.texto = morenasVio;

                        break;
                }
            }
            else 
            {
                switch (causa)
                {
                    case "Rana":
                        hablarScr.texto = ranasAbe;

                        break;
                    case "Naife":
                        hablarScr.texto = naifesAbe;

                        break;
                    case "Natilla":
                        hablarScr.texto = natillaAbe;

                        break;
                    case "Caida":
                        hablarScr.texto = caidaAbe;

                        break;
                    default:
                        hablarScr.texto = morenasAbe;

                        break;
                }
            }
        }
        else 
        {
            hablarScr.texto = caidaTwi;
        }
    }
}