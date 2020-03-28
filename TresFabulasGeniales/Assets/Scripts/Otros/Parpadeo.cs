
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Parpadeo : MonoBehaviour
{
    [SerializeField] private int[] cambiar;
    [SerializeField] private Material materialAlt;
    private Renderer renderer;
    private Material[] materialesIni, materialesAlt;
    private bool parpadeando;
    private float tiempoPas, tiempoMax;
    private int parpadeoDob;


    // Inicialización de variables.
    private void Start ()
    {
        renderer = this.GetComponentInChildren<Renderer> ();
        materialesIni = new Material[renderer.materials.Length];
        materialesAlt = new Material[materialesIni.Length];
        parpadeando = false;
        tiempoPas = 0;
        tiempoMax = Random.Range (3f, 5.5f);
        parpadeoDob = 0;
        for (int m = 0; materialesIni.Length > m; m += 1) 
        {
            materialesIni[m] = renderer.materials[m];
        }
        for (int m = 0; materialesAlt.Length > m; m += 1)
        {
            materialesAlt[m] = materialesIni[m];
        }
        foreach (int i in cambiar) 
        {
            materialesAlt[i] = materialAlt;
        }
    }


    // Cuando ha pasado un cierto tiempo con los ojos abiertos o los párpados bajados, llamamos a la función para cambiar los materiales, cambiamos el valor de la variable que indica si está parpadeando, reseteamos el tiempo pasado y definimos el que
    //pasará hasta que vuelva a bajar los párpados o subirlos (según la situación), también contemplamos la posibilidad de que pueda realizar un parpadeo doble para añadir variedad.
    private void Update ()
    {
        tiempoPas += Time.deltaTime;
        if (tiempoPas > tiempoMax) 
        {
            Parpadear ();

            if (parpadeoDob == 0)
            {
                tiempoMax = parpadeando == false ? Random.Range (0.1f, 0.15f) : Random.Range (3f, 5.5f);
                if (parpadeando == false) 
                {
                    parpadeoDob = Random.Range (0f, 1f) > 0.3f ? 0 : 2;
                }
            }
            else 
            {
                parpadeoDob -= 1;
            }
            parpadeando = !parpadeando;
            tiempoPas = 0;
        }
    }


    // Cambiamos los materiales de los ojos por otros para dar la sensación de que el personaje está parpadeando.
    private void Parpadear ()
    {
        renderer.materials = parpadeando == false ? materialesAlt : materialesIni;
    }
}