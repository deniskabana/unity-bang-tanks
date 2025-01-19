using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.U2D;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    public bool debug = false;

    [Header("Settings")]
    [SerializeField] bool enableCameraAnimation = true;
    [SerializeField] float resolutionChangePollTime = 0.1f; // 50ms
    [SerializeField] float cameraZoomedOutRatio = 0.86f; // Manually tested ratios
    [SerializeField] float cameraZoomedInRatio = 0.613f; // Manually tested ratios
    [SerializeField] float cameraSpeedZoom = 2f;
    [SerializeField] float cameraSpeedMove = 2f;
    [SerializeField] bool constrainCameraBoundaries = true;
    [SerializeField] Transform mainVirtualCameraObject;

    // Runtime variables
    // --------------------------------------------------

    bool initialized = false;
    bool cameraZoomedIn = false;
    CinemachineVirtualCamera mainVirtualCamera;

    // Built-in methods
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
        mainVirtualCamera = mainVirtualCameraObject.GetComponent<CinemachineVirtualCamera>();
    }

    void Start()
    {
        if (!Instance) Instance = this;
        mainVirtualCamera = mainVirtualCameraObject.GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        if (!initialized) return;
        HandleCameraZoom();
    }

    // Custom methods
    // --------------------------------------------------

    public void Initialize()
    {
        initialized = true;
    }

    void HandleCameraZoom()
    {
        float currentSize = mainVirtualCamera.m_Lens.OrthographicSize;
        float newSize = cameraZoomedIn ? 12 : 17;

        // Cancel early if we're already at the desired size
        if (currentSize == newSize) return;

        // If the distance between the camera and the target is greater than the threshold, zoom the camera
        float threshold = 1 / 10f; // 10% zoom threshold for the most expensive calculations
        if (Mathf.Abs(currentSize - newSize) > threshold && enableCameraAnimation)
        {
            newSize = Mathf.Lerp(currentSize, newSize, Time.deltaTime * cameraSpeedZoom);
        }
        else
        {
            newSize = currentSize;
        }

        mainVirtualCamera.m_Lens.OrthographicSize = Mathf.Round(newSize * 10000f) / 10000f;
    }


    // Static methods
    // --------------------------------------------------

    public static void TrackObject(GameObject obj)
    {
        if (Instance.debug) Debug.Log("CameraManager: Track object " + obj.name);
        Instance.mainVirtualCamera.Follow = obj.transform;
        Instance.mainVirtualCamera.LookAt = obj.transform;
    }

    public static void Zoom(bool zoomIn = true)
    {
        if (Instance.debug) Debug.Log("CameraManager: Zoom " + (zoomIn ? "in" : "out"));
        Instance.cameraZoomedIn = zoomIn;
    }
}
