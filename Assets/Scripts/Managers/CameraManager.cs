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
    [SerializeField] private float cameraSpeedZoom = 1.75f;
    [SerializeField] private float cameraSpeedMove = 5f;
    [SerializeField] private bool checkBoundaries = true;
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
        mainCameraComponent.orthographicSize = enableCameraAnimation ? Mathf.Lerp(mainCameraComponent.orthographicSize, cameraSize, Time.deltaTime * cameraSpeedZoom) : cameraSize;
    }

    // TODO: Problematic to have this method in Update, should be called only when necessary and cancel early if not needed
    void HandleCameraMovement()
    {
        Vector3 targetPosition = trackedObject ? trackedObject.transform.position : centerPoint;
        targetPosition.z = cameraZ; // Make sure we don't change the Z position

        // TODO: Lerping here is slow, needs a threshold to stop lerping close-enough to spare calculations
        if (enableCameraAnimation) mainCamera.position = Vector3.Lerp(mainCamera.position, targetPosition, Time.deltaTime * cameraSpeedMove);
        else mainCamera.position = targetPosition;

        if (!checkBoundaries) return;
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
