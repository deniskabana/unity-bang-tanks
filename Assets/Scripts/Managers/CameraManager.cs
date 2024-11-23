using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    public bool debug = false;

    [Header("Settings")]
    [SerializeField] bool enableCameraAnimation = true;
    [SerializeField] float resolutionChangePollTime = 1f;
    [SerializeField] float cameraZoomedOutRatio = 0.86f; // Manually tested ratios
    [SerializeField] float cameraZoomedInRatio = 0.613f; // Manually tested ratios
    [SerializeField] float cameraSpeedZoom = 2f;
    [SerializeField] float cameraSpeedMove = 2f;
    [SerializeField] bool constrainCameraBoundaries = true;
    [SerializeField] Transform mainCamera;

    // Runtime variables
    // --------------------------------------------------

    bool initialized = false;
    float cameraZ; // Camera Z position is constant
    bool cameraZoomedIn = false;
    GameObject trackedObject;
    Camera mainCameraComponent;

    // Computed camera values
    float actualCamZoomedInSize; // Computed value for zoomed in on player
    float actualCamZoomedOutSize; // Computed value for zoomed out - default state
    // Resolution change detection
    float lastScreenWidth;
    float lastScreenHeight;
    Vector3 lastTargetPosition;

    [System.Serializable]
    public struct CameraBounds
    {
        public float minYZoomedIn;
        public float minYZoomedOut;
        public float minXZoomedIn;
        public float minXZoomedOut;
        public float maxXZoomedIn;
        public float maxXZoomedOut;
    }
    CameraBounds cameraBounds;

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
        CalculateCameraSizes(); // Sizes first
        CalculateCameraBounds(); // Bounds uses new sizes!
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        // Start resolution change detection
        StartCoroutine(CheckResolutionChange());
    }

    void HandleCameraZoom()
    {
        float currentSize = mainCameraComponent.orthographicSize;
        float newSize = cameraZoomedIn ? actualCamZoomedInSize : actualCamZoomedOutSize;

        // Cancel early if we're already at the desired size
        if (currentSize == newSize) return;

        // If the distance between the camera and the target is greater than the threshold, zoom the camera
        float threshold = 1 / 100f;
        if (Mathf.Abs(currentSize - newSize) > threshold && enableCameraAnimation)
        {
            newSize = Mathf.Lerp(currentSize, newSize, Time.deltaTime * cameraSpeedZoom);
        }

        mainCameraComponent.orthographicSize = Mathf.Round(newSize * 1000f) / 1000f;
    }

    void HandleCameraMovement()
    {
        if (trackedObject) lastTargetPosition = trackedObject.transform.position;
        Vector3 targetPosition = lastTargetPosition;

        if (constrainCameraBoundaries)
        {
            // Constrain camera to terrain bounds
            if (cameraZoomedIn)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, cameraBounds.minXZoomedIn, cameraBounds.maxXZoomedIn);
                targetPosition.y = Mathf.Clamp(targetPosition.y, cameraBounds.minYZoomedIn, float.MaxValue);
            }
            else
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, cameraBounds.minXZoomedOut, cameraBounds.maxXZoomedOut);
                targetPosition.y = Mathf.Clamp(targetPosition.y, cameraBounds.minYZoomedOut, float.MaxValue);
            }
        }

        // Cancel early if we're already at the desired position
        if (mainCamera.position.x == targetPosition.x && mainCamera.position.y == targetPosition.y) return;

        // float threshold = 1 / 1000f; // Also called deadzone
        float threshold = 0.5f;
        if (Mathf.Abs(mainCamera.position.x - targetPosition.x) > threshold || Mathf.Abs(mainCamera.position.y - targetPosition.y) > threshold)
        {
            if (enableCameraAnimation)
            {
                targetPosition = Vector3.Lerp(mainCamera.position, targetPosition, Time.deltaTime * cameraSpeedMove);
                mainCamera.position = targetPosition;
            }

            targetPosition.z = cameraZ; // Keep the camera Z position constant
            mainCamera.position = targetPosition;
        }

    }

    void CalculateCameraBounds()
    {
        if (!constrainCameraBoundaries) return;
        if (mainCameraComponent == null) mainCameraComponent = mainCamera.GetComponent<Camera>();

        Bounds terrainBounds = TerrainManager.Instance.GetTerrainRendererBounds();
        float threshold = 0.5f;
        float aspectRatio = mainCameraComponent.aspect;
        float minX = terrainBounds.min.x + threshold;
        float maxX = terrainBounds.max.x - threshold;
        float minY = terrainBounds.min.y + threshold;

        cameraBounds = new CameraBounds
        {
            minYZoomedIn = minY + actualCamZoomedInSize,
            minYZoomedOut = minY + actualCamZoomedOutSize,
            minXZoomedIn = minX + actualCamZoomedInSize * aspectRatio,
            minXZoomedOut = minX + actualCamZoomedOutSize * aspectRatio,
            maxXZoomedIn = maxX - actualCamZoomedInSize * aspectRatio,
            maxXZoomedOut = maxX - actualCamZoomedOutSize * aspectRatio
        };

        if (debug) Debug.Log("CameraManager: Calculate camera bounds");
    }

    void CalculateCameraSizes()
    {
        Bounds terrainBounds = TerrainManager.Instance.GetTerrainRendererBounds();
        float terrainWidth = terrainBounds.size.x;

        float aspectRatio = (float)Screen.width / Screen.height;

        actualCamZoomedOutSize = terrainWidth / aspectRatio / 2f * cameraZoomedOutRatio;
        actualCamZoomedInSize = terrainWidth / aspectRatio / 2f * cameraZoomedInRatio;

        if (debug) Debug.Log("CameraManager: New camera sizes: " + actualCamZoomedInSize + "; " + actualCamZoomedOutSize);
    }

    IEnumerator CheckResolutionChange()
    {
        while (true)
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                if (debug) Debug.Log("CameraManager: Resolution change detected");
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                CalculateCameraSizes(); // Sizes first
                CalculateCameraBounds(); // Bounds uses new sizes!
            }

            yield return new WaitForSeconds(resolutionChangePollTime);
        }
    }


    // Static methods
    // --------------------------------------------------

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
