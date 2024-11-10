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
    [SerializeField] private float cameraZoomedInSize = 4.75f; // Zoom in on player
    [SerializeField] private float cameraZoomedOutSize = 7.4f; // Default state
    [SerializeField] private float cameraSpeedZoom = 2.2f;
    [SerializeField] private float cameraSpeedMove = 5f;
    [SerializeField] private bool constrainCameraBoundaries = true;
    [SerializeField] private Transform mainCamera;

    // Runtime variables
    // --------------------------------------------------

    bool initialized = false;
    GameObject trackedObject;
    Camera mainCameraComponent;
    float cameraSize;
    float cameraZ;
    Vector3 centerPoint = new Vector3(0, 0, 0);

    // Built-in methods
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!Instance) Instance = this;
        cameraSize = cameraZoomedOutSize;
        mainCameraComponent = mainCamera.GetComponent<Camera>();
        cameraZ = mainCamera.transform.position.z;
    }

    void LateUpdate()
    {
        if (!initialized) return;
        HandleCameraMovement();
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
        float currentSize = mainCameraComponent.orthographicSize;
        float newSize = cameraSize;

        // Cancel early if we're already at the desired size
        if (currentSize == cameraSize) return;

        // If the distance between the camera and the target is greater than the threshold, zoom the camera
        float threshold = 1 / 100f;
        if (Math.Abs(currentSize - cameraSize) > threshold && enableCameraAnimation)
        {
            newSize = Mathf.Lerp(currentSize, cameraSize, Time.deltaTime * cameraSpeedZoom);
        }

        mainCameraComponent.orthographicSize = Mathf.Round(newSize * 1000f) / 1000f;
    }

    void HandleCameraMovement()
    {
        Vector3 targetPosition = trackedObject ? trackedObject.transform.position : centerPoint;
        targetPosition.z = cameraZ;

        // Cancel early if we're already at the desired position
        if (mainCamera.position == targetPosition) return;
        Vector3 newPosition = mainCamera.position;

        // If the distance between the camera and the target is greater than the threshold, move the camera
        float threshold = 1 / 1000f;
        if (Math.Abs(mainCamera.position.x - targetPosition.x) > threshold && Math.Abs(mainCamera.position.y - targetPosition.y) > threshold)
        {
            if (enableCameraAnimation) newPosition = Vector3.Lerp(mainCamera.position, targetPosition, Time.deltaTime * cameraSpeedMove);
        }

        mainCamera.position = newPosition;

        if (!constrainCameraBoundaries) return;

        // TODO: This works but is super slow; perform calculations minimum viable amount of times
        // Camera boundaries based on terrain size and orthographic size
        float cameraHalfWidth = mainCameraComponent.orthographicSize * mainCameraComponent.aspect;
        float cameraHalfHeight = mainCameraComponent.orthographicSize;
        Bounds terrainBounds = TerrainManager.Instance.GetTerrainRendererBounds();
        float minX = terrainBounds.min.x + cameraHalfWidth;
        float maxX = terrainBounds.max.x - cameraHalfWidth;
        float minY = terrainBounds.min.y + cameraHalfHeight;
        float maxY = terrainBounds.max.y - cameraHalfHeight;
        mainCamera.position = new Vector3(
            Mathf.Clamp(mainCamera.position.x, minX, maxX),
            Mathf.Clamp(mainCamera.position.y, minY, maxY),
            mainCamera.position.z
        );
    }

    public static void SetSceneCenter(Vector3 center)
    {
        if (Instance.debug) Debug.Log("Set scene center to " + center);
        Instance.centerPoint = center;
    }

    public static void TrackObject(GameObject obj, bool zoomIn = true)
    {
        if (Instance.debug) Debug.Log("Track object " + obj.name);
        Instance.trackedObject = obj;
        Instance.cameraSize = zoomIn ? Instance.cameraZoomedInSize : Instance.cameraZoomedOutSize;
    }

    public static void StopTracking(bool zoomOut = true)
    {
        if (Instance.debug) Debug.Log("Stop tracking");
        Instance.trackedObject = null;
        Instance.cameraSize = zoomOut ? Instance.cameraZoomedOutSize : Instance.cameraZoomedInSize;
    }
}
