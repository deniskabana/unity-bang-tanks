using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    public bool debug = false;

    [Header("Settings")]
    [SerializeField] bool enableCameraAnimation = true;
    [SerializeField] float resolutionChangePollTime = 1f;
    [SerializeField] float cameraZoomedOutRatio = 0.9f; // Manually tested ratios
    [SerializeField] float cameraZoomedInRatio = 0.563f; // Manually tested ratios
    [SerializeField] float cameraSpeedZoom = 2.2f;
    [SerializeField] float cameraSpeedMove = 5f;
    [SerializeField] bool constrainCameraBoundaries = true;
    [SerializeField] Transform mainCamera;

    // Runtime variables
    // --------------------------------------------------

    bool initialized = false;
    float cameraZ; // Camera Z position is constant
    bool cameraZoomedIn = false;
    GameObject trackedObject;
    Camera mainCameraComponent;

    Vector3 centerPoint = new Vector3(0, 0, 0);

    // Computed camera values
    float actualCamZoomedInSize; // Computed value for zoomed in on player
    float actualCamZoomedOutSize; // Computed value for zoomed out - default state
    // Resolution change detection
    float lastScreenWidth;
    float lastScreenHeight;

    // Built-in methods
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!Instance) Instance = this;
        mainCameraComponent = mainCamera.GetComponent<Camera>();
        cameraZ = mainCamera.transform.position.z;
    }

    void LateUpdate()
    {
        if (!initialized) return;
        HandleCameraMovement();
        HandleCameraZoom();

        if (!trackedObject && cameraZoomedIn) cameraZoomedIn = false;
    }

    // Custom methods
    // --------------------------------------------------

    public void Initialize()
    {
        initialized = true;
        StartCoroutine(CheckResolutionChange());
    }

    IEnumerator CheckResolutionChange()
    {
        while (true)
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                if (debug) Debug.Log("CameraManager: Resolution change detected");
                // CalculateCameraBounds();
                CalculateCameraSizes();
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
            }

            yield return new WaitForSeconds(resolutionChangePollTime);
        }
    }

    void HandleCameraZoom()
    {
        float currentSize = mainCameraComponent.orthographicSize;
        float newSize = cameraZoomedIn ? actualCamZoomedInSize : actualCamZoomedOutSize;

        // Cancel early if we're already at the desired size
        if (currentSize == newSize) return;

        // If the distance between the camera and the target is greater than the threshold, zoom the camera
        float threshold = 1 / 100f;
        if (Math.Abs(currentSize - newSize) > threshold && enableCameraAnimation)
        {
            newSize = Mathf.Lerp(currentSize, newSize, Time.deltaTime * cameraSpeedZoom);
        }

        mainCameraComponent.orthographicSize = Mathf.Round(newSize * 1000f) / 1000f;
    }

    void HandleCameraMovement()
    {
        if (!trackedObject) return;
        Vector3 targetPosition = trackedObject ? trackedObject.transform.position : centerPoint;
        targetPosition.z = cameraZ;

        // Cancel early if we're already at the desired position
        if (mainCamera.position.x == targetPosition.x && mainCamera.position.y == targetPosition.y) return;
        Vector3 newPosition = mainCamera.position;

        // If the distance between the camera and the target is greater than the threshold, move the camera
        float threshold = 1 / 400f;
        if (Math.Abs(mainCamera.position.x - targetPosition.x) > threshold && Math.Abs(mainCamera.position.y - targetPosition.y) > threshold)
        {
            if (enableCameraAnimation) newPosition = Vector3.Lerp(mainCamera.position, targetPosition, Time.deltaTime * cameraSpeedMove);
        }

        mainCamera.position = newPosition;
    }

    void CalculateCameraBounds()
    {
        if (debug) Debug.Log("CameraManager: Calculate camera bounds");
        if (mainCameraComponent == null) mainCameraComponent = mainCamera.GetComponent<Camera>();
        Bounds terrainBounds = TerrainManager.Instance.GetTerrainRendererBounds();

        // TODO: implement
    }

    void CalculateCameraSizes()
    {
        Bounds terrainBounds = TerrainManager.Instance.GetTerrainRendererBounds();
        float terrainWidth = terrainBounds.size.x;

        float aspectRatio = (float)Screen.width / Screen.height;

        actualCamZoomedOutSize = terrainWidth / aspectRatio / 2f * cameraZoomedOutRatio;
        actualCamZoomedInSize = actualCamZoomedOutSize * cameraZoomedInRatio;

        if (debug) Debug.Log("CameraManager: Camera sizes: " + actualCamZoomedInSize + " / " + actualCamZoomedOutSize);
    }
    public static void SetSceneCenter(Vector3 center)
    {
        if (Instance.debug) Debug.Log("CameraManager: Set scene center to " + center);
        Instance.centerPoint = center;
    }

    public static void TrackObject(GameObject obj, bool zoomIn = true)
    {
        if (Instance.debug) Debug.Log("CameraManager: Track object " + obj.name);
        Instance.trackedObject = obj;
    }

    public static void Zoom(bool zoomIn = true)
    {
        if (Instance.debug) Debug.Log("CameraManager: Zoom " + (zoomIn ? "in" : "out"));
        Instance.cameraZoomedIn = zoomIn;
    }
}
