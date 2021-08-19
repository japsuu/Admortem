using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class StepPlayer : MonoBehaviour
{
    public StudioEventEmitter stepEmitter;
    
    public void PlayStep()
    {
        stepEmitter.Play();
    }
}
