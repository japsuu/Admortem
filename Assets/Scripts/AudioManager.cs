using System.Collections;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Music")]
    
    [SerializeField] [EventRef] private string nightMusicEvent;
    [SerializeField] [ParamRef] private string clipIndexParameter;
    [SerializeField] private uint clipCount;
    
    [Header("SFX")]
    [EventRef] public string pickupEvent;
    [EventRef] public string genericPlacedEvent;
    [EventRef] public string genericMinedEvent;
    [EventRef] public string genericBrokenEvent;
    
    [Space]
    
    [SerializeField] [ReadOnly] private uint currentlyPlayingClipIndex;

    private EventInstance nightInstance;
    private uint[] clipIndexes;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        
        DontDestroyOnLoad(gameObject);

        clipIndexes = new uint[clipCount];
        
        nightInstance = RuntimeManager.CreateInstance(nightMusicEvent);
    }

    private void Start()
    {
        // Shuffle the clips
        for (int i = 0; i < clipIndexes.Length; i++)
        {
            clipIndexes[i] = (uint)i;
        }
        
        System.Random rnd = new System.Random();
        clipIndexes = clipIndexes.OrderBy(x => rnd.Next()).ToArray(); 
        
        currentlyPlayingClipIndex = GetNextClipIndex();
        
        nightInstance.start();
        nightInstance.release();
    }

    private void Update()
    {
        if (IsFinishedPlaying(nightInstance))
        {
            currentlyPlayingClipIndex = GetNextClipIndex();
        }
        
        RuntimeManager.StudioSystem.setParameterByName(clipIndexParameter, currentlyPlayingClipIndex);
    }

    public void StartPlayback()
    {
        nightInstance.start();
        nightInstance.release();
    }

    public void StopPlayback()
    {
        nightInstance.stop(STOP_MODE.ALLOWFADEOUT);
    }

    public IEnumerator Next()
    {
        nightInstance.stop(STOP_MODE.ALLOWFADEOUT);

        yield return new WaitForSecondsRealtime(3);
        
        currentlyPlayingClipIndex = GetNextClipIndex();

        nightInstance.start();
        nightInstance.release();
        
        yield return null;
    }

    public IEnumerator Previous()
    {
        nightInstance.stop(STOP_MODE.ALLOWFADEOUT);

        yield return new WaitForSecondsRealtime(3);
        
        currentlyPlayingClipIndex = GetPreviousClipIndex();

        nightInstance.start();
        nightInstance.release();
        
        yield return null;
    }

    /// <summary>
    /// Set the volume in the range 0 to 1.
    /// </summary>
    /// <param name="volume"></param>
    public void SetVolume(float volume)
    {
        nightInstance.setVolume(volume);
    }

    private int currentArrayIndex;
    private uint GetNextClipIndex() //BUG: FIX CLIPPING WHEN CHANGING SONGS
    {
        currentArrayIndex++;
        
        if (currentArrayIndex > clipIndexes.Length - 1)
        {
            currentArrayIndex = 0;
        }
        
        return clipIndexes[currentArrayIndex];
    }
    
    private uint GetPreviousClipIndex()
    {
        currentArrayIndex--;
        
        if (currentArrayIndex < 0)
        {
            currentArrayIndex = clipIndexes.Length - 1;
        }
        
        Debug.Log("Selecting music clip " + currentArrayIndex);
        
        return clipIndexes[currentArrayIndex];
    }

    private static bool IsFinishedPlaying(EventInstance instance)
    {
        instance.getPlaybackState(out PLAYBACK_STATE state);
        
        return state == PLAYBACK_STATE.SUSTAINING;
    }
}
