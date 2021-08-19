using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public Gradient lightGradient;
    public Light2D sun;
    public Transform radialGraphic;
    public Transform backgroundParallaxLayer1;
    public Transform backgroundParallaxLayer2;
    public Transform backgroundParallaxLayer3;
    public int dayLength = 600;
    [Range(0, 23)]
    public int dayStart = 9;
    public bool stopCycle = false;

    float time = 400f;
    [ReadOnly] public string timeOfDay = "";

    int days;
    public int Days => days;
    bool canChangeDay = true;

    /// <summary>
    /// Called every time a day has passed.
    /// </summary>
    //public delegate void OnDayChanged();

    //public OnDayChanged DayChanged;

    private void Start()
    {
        time = 24 / dayLength * dayStart;
    }

    private void Update()
    {
        if (stopCycle)
            return;

        if(time > dayLength)
        {
            time = 0;
        }

        if((int)time == dayLength / 2 && canChangeDay)
        {
            canChangeDay = false;
            //DayChanged();
            Debug.Log("Day changed");
            days++;
        }

        if((int)time == dayLength / 2 + 5)
        {
            canChangeDay = true;
        }

        time += Time.deltaTime;

        sun.color = lightGradient.Evaluate(time * 1 / dayLength);

        // First normalize to 0-1 range, then multiply by 24
        float normalized = time / dayLength;
        float timeSpan = 24 * normalized;

        int hours = (int)(timeSpan + 12);
        if(hours >= 24)
        {
            hours -= 24;
        }

        int minutes = (int)(60 * (timeSpan % 1));

        timeOfDay = hours + " : " + minutes;

        if(radialGraphic != null)
        {
            radialGraphic.rotation = Quaternion.Euler(0, 0, 360 * normalized);
        }
    }

    private void FixedUpdate()
    {
        float newX = CameraController.Instance.transform.position.x;
        radialGraphic.position = new Vector2(newX / 4, radialGraphic.position.y);

        backgroundParallaxLayer1.position = new Vector2(newX / 2, backgroundParallaxLayer1.position.y);
        backgroundParallaxLayer2.position = new Vector2(newX / 4, backgroundParallaxLayer2.position.y);
        backgroundParallaxLayer3.position = new Vector2(newX / 8, backgroundParallaxLayer3.position.y);
    }
}
