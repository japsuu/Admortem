using UnityEngine;

public class FmodPlayer : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string eventName;
    
    public void Play()
    {
        FMODUnity.RuntimeManager.PlayOneShot(eventName, transform.position);
    }
}
