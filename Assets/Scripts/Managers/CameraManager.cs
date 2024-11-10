using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    public bool debug = false;

    [Header("Settings")]
    [SerializeField] private float cameraSpeedZoom = 1.75f;
    [SerializeField] private float cameraSpeedMove = 5f;
    [SerializeField] private float cameraZoomedInSize = 4.75f; // Zoom in on player
    [SerializeField] private float cameraZoomedOutSize = 7.4f; // Default state
    [SerializeField] private Transform mainCamera;

    // Runtime variables
    // --------------------------------------------------

    bool initialized = false;
    GameObject trackedObject;
    Camera mainCameraComponent;
    float cameraSize;
    float cameraZ;

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

    void Update()
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
        mainCameraComponent.orthographicSize = Mathf.Lerp(mainCameraComponent.orthographicSize, cameraSize, Time.deltaTime * cameraSpeedZoom);
    }

    void HandleCameraMovement()
    {
        if (trackedObject)
        {
            Vector3 targetPosition = trackedObject.transform.position;
            targetPosition.z = cameraZ; // Make sure we don't change the Z position
            mainCamera.position = Vector3.Lerp(mainCamera.position, targetPosition, Time.deltaTime * cameraSpeedMove);
        }
    }

    public static void SetSceneCenter(Vector3 center)
    {
        if (Instance.debug) Debug.Log("Set scene center to " + center);
        Instance.mainCamera.transform.position = new Vector3(center.x, center.y, -10);
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
