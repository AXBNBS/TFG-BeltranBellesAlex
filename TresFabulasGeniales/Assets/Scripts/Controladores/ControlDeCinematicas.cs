
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;



public class ControlDeCinematicas : MonoBehaviour
{
    [SerializeField] private int[] cuadros;
    [SerializeField] private float[] segundos;
    [SerializeField] private Animator[] actores;
    private PlayableDirector director;
    private bool seguir;
    private int cuadrosPas, indiceArr;


    // .
    private void Start ()
    {
        director = this.GetComponent<PlayableDirector> ();
        
    }


    // .
    private void Update ()
    {
        if (Input.GetKeyDown (KeyCode.E) == true) 
        {
            cuadrosPas += 1;
        }
        ControlarFlujo ();
    }


    // .
    private void ControlarFlujo () 
    {
        if (PlayState.Playing == director.state && cuadros[indiceArr] > cuadrosPas && segundos[indiceArr] < director.time)
        {
            director.Pause ();
        }
        if (PlayState.Paused == director.state && cuadros[indiceArr] <= cuadrosPas)
        {
            if (cuadros.Length - 1 != indiceArr)
            {
                indiceArr += 1;
            }

            director.Resume ();
        }
    }
}