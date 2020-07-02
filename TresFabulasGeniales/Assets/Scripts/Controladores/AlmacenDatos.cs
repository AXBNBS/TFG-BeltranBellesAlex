
using UnityEngine;



public class AlmacenDatos : MonoBehaviour
{
    public static AlmacenDatos instancia;
    public bool muerte;
    public int regresarA;
    public string menuEsc;
    [HideInInspector] public string avatarMue, causaMue;


    // Creación del singleton e inicialización de variables.
    private void Awake ()
    {
        if (instancia != null)
        {
            GameObject.Destroy (this.gameObject);
        }
        else
        {
            instancia = this;
            regresarA = 1;

            GameObject.DontDestroyOnLoad (this.gameObject);
        }

        // CUIDAO CON ESTO AL FINAL
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 60;
    }
}