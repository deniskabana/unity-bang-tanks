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
    [SerializeField] private bool enableCameraAnimation = true;
    [SerializeField] private float resolutionChangePollTime = 3f;
    [SerializeField] private float defaultCameraZoomedInSize = 4.75f; // Zoom in on player
    [SerializeField] private float defaultCameraZoomedOutSize = 7.4f; // Default state
    [SerializeField] private float cameraSpeedZoom = 2.2f;
    [SerializeField] private float cameraSpeedMove = 5f;
    [SerializeField] private bool constrainCameraBoundaries = true;
    [SerializeField] private Transform mainCamera;

    // Runtime variables
    // --------------------------------------------------

    bool initialized = false;

    GameObject trackedObject; // Object being tracked - optional
    Camera mainCameraComponent;
    bool cameraZoomedIn = false; // State - if camera is zoomed in on player
    float cameraZ; // Camera Z position is constant
    Vector3 centerPoint = new Vector3(0, 0, 0);
    // Computed camera values
    float actualCamZoomedInSize; // Computed value for zoomed in on player
    float actualCamZoomedOutSize; // Computed value for zoomed out - default state
    float cameraHalfWidth;
    float cameraHalfHeight;
    float minX;
    float maxX;
    float minY;
    float maxY;

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
                CalculateCameraBounds();
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
        cameraHalfWidth = mainCameraComponent.orthographicSize * mainCameraComponent.aspect;
        cameraHalfHeight = mainCameraComponent.orthographicSize;
        minX = terrainBounds.min.x + cameraHalfWidth;
        maxX = terrainBounds.max.x - cameraHalfWidth;
        minY = terrainBounds.min.y + cameraHalfHeight;
        maxY = terrainBounds.max.y - cameraHalfHeight;
    }

    void CalculateCameraSizes()
    {
        if (debug) Debug.Log("CameraManager: Calculate camera sizes (should run after calculating camera bounds)");
        // Calculate if the sizes are sufficient for the terrain size and if the camera width would be bigger than terrain, zoom in more
        Bounds terrainBounds = TerrainManager.Instance.GetTerrainRendererBounds();

        float terrainWidth = terrainBounds.size.x;
        float terrainHeight = terrainBounds.size.y;
        float cameraAspect = mainCameraComponent.aspect;
        float terrainAspect = terrainWidth / terrainHeight;

        if (terrainAspect > cameraAspect)
        {
            // Terrain is wider than camera aspect ratio
            actualCamZoomedInSize = defaultCameraZoomedInSize * (terrainAspect / cameraAspect);
            actualCamZoomedOutSize = defaultCameraZoomedOutSize * (terrainAspect / cameraAspect);
        }
        else
        {
            // Terrain is taller than camera aspect ratio
            actualCamZoomedInSize = defaultCameraZoomedInSize * (cameraAspect / terrainAspect);
            actualCamZoomedOutSize = defaultCameraZoomedOutSize * (cameraAspect / terrainAspect);
        }

        // Ensure the zoomed out size is smaller or equal to the terrain width
        // actualCamZoomedOutSize = Mathf.Min(cameraZoomedOutSize, terrainWidth / (2 * mainCameraComponent.aspect));

        // Ensure the zoomed in size is 33% more zoomed in
        // actualCamZoomedInSize = Mathf.Max(cameraZoomedInSize, actualCamZoomedOutSize / 1.33f);

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
