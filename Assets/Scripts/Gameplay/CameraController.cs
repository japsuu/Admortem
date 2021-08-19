using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    [Header("Camera settings")]
    public float moveSmoothness = 0.3f;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 20f;
    [Range(0f, 1f)]
    public float zoomSmoothness = .015f;
    public Transform target = null;
    public bool fixedUpdate = false;
    [Space]
    [Header("Amount of shake")]
    [Range(0f, 5f)]
    public float shakeMagnitude = 0.7f;
    [Header("How quickly the shake evaporates")]
    [Range(0f, 5f)]
    public float dampingSpeed = 1.0f;

    private Camera cam;
    private float savedZoom;
    private float zoom;
    private static float shakeDuration = 0f;
    private Vector3 initialPos = Vector3.zero;
    private Vector3 lastSavedPos = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private void OnEnable()
    {
        initialPos = transform.position;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        cam = GetComponent<Camera>();
        savedZoom = cam.orthographicSize;
        zoom = savedZoom;
    }

    void Update()
    {
        DoScreenshake();
        HandleZooming();
        if (!fixedUpdate)
        {
            if (target != null)
            {
                MoveCamera();

                lastSavedPos = target.position;
            }
            else
            {
                if (lastSavedPos != Vector3.zero)
                {
                    transform.parent.position = lastSavedPos;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (fixedUpdate)
        {
            if (target != null)
            {
                MoveCamera();

                lastSavedPos = target.position;
            }
            else
            {
                if (lastSavedPos != Vector3.zero)
                {
                    transform.parent.position = lastSavedPos;
                }
            }
        }
    }

    void MoveCamera()
    {
        Vector3 goalPos = target.position;
        goalPos.z = -10;
        transform.parent.position = Vector3.SmoothDamp(transform.parent.position, goalPos, ref velocity, moveSmoothness);
    }

    void DoScreenshake()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = initialPos + Random.insideUnitSphere * shakeMagnitude;

            shakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = initialPos;
        }
    }

    void HandleZooming()
    {
        if (Input.GetKey(Settings.CameraZoomInKey))
            zoom -= zoomSpeed * Time.deltaTime;
        if (Input.GetKey(Settings.CameraZoomOutKey))
            zoom += zoomSpeed * Time.deltaTime;

        if (Input.mouseScrollDelta.y > 0)
            zoom -= zoomSpeed * Time.deltaTime * 20f + (zoom / 16);
        if (Input.mouseScrollDelta.y < 0)
            zoom += zoomSpeed * Time.deltaTime * 20f + (zoom / 16);

        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

        if (Input.GetKey(Settings.CameraZoomResetKey))
            zoom = savedZoom;

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoom, zoomSmoothness / 2);
    }

    public static void Shake(float duration)
    {
        shakeDuration = duration;
    }
}
