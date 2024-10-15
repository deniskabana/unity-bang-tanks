using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField] private int numberOfPoints = 50;
    [SerializeField] private float terrainWidth = 20f;
    [SerializeField] private float maxHeight = 5f;
    [SerializeField] private float noiseScale = 0.2f;
    [SerializeField] private float noiseScaleRandomFactor = 2f;

    [Header("Debug")]
    [SerializeField] private bool showAdvancedGizmos = false;

    // Private variables
    // --------------------------------------------------

    private List<Vector3> splinePoints = new List<Vector3>();
    private float randomizedNoiseScale;
    private float xOffset;

    // Built-in methods
    // --------------------------------------------------

    void Start()
    {
        Initialize();
    }

    void OnValidate()
    {
        Initialize();
    }

    void OnDrawGizmos()
    {
        if (!showAdvancedGizmos) return;
        if (splinePoints == null || splinePoints.Count == 0) return;

        Gizmos.color = Color.white;

        // Draw a sphere at each spline point
        foreach (var point in splinePoints)
        {
            Gizmos.DrawWireSphere(point, 0.15f);
        }

        // Draw connecting lines between the points
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            Gizmos.DrawLine(splinePoints[i], splinePoints[i + 1]);
        }

    }

    // Custom methods
    // --------------------------------------------------

    void Initialize()
    {
        // Randomize the noise scale to create different terrain each time
        randomizedNoiseScale = Random.Range(noiseScale / noiseScaleRandomFactor, noiseScale * noiseScaleRandomFactor);
        xOffset = Random.Range(0f, 100f); // Random offset to avoid repetitive starting terrain behavior

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        // Debug check the assigned sprite
        Debug.Log("Assigned Sprite: " + spriteRenderer.sprite.name);

        // Debug check the assigned material
        Debug.Log("Assigned Material: " + spriteRenderer.material.name);

        GenerateTerrainSpline();
    }

    void GenerateTerrainSpline()
    {
        splinePoints.Clear();
        float spacing = terrainWidth / (numberOfPoints - 1);

        for (int i = 0; i < numberOfPoints; i++)
        {
            float xPos = i * spacing;

            // Use Perlin noise with randomization and offset to generate terrain heights
            float yPos = Mathf.PerlinNoise((xPos * randomizedNoiseScale) + xOffset, 0) * maxHeight;

            splinePoints.Add(new Vector3(xPos, yPos, 0));
        }

        // Use Catmull-Rom smoothing for a better curve
        splinePoints = GenerateCatmullRomSpline(splinePoints);
    }

    // Catmull-Rom spline interpolation for smoother curves
    private List<Vector3> GenerateCatmullRomSpline(List<Vector3> points)
    {
        List<Vector3> smoothPoints = new List<Vector3>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = (i == 0) ? points[i] : points[i - 1]; // Handle boundary case
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = (i + 2 < points.Count) ? points[i + 2] : points[i + 1]; // Handle boundary case

            // Interpolate between points p1 and p2 with tangents
            smoothPoints.Add(p1); // Add the first point directly
            for (int t = 1; t <= 10; t++) // Increase 10 for more smoothness
            {
                float tNorm = t / 10f;
                smoothPoints.Add(CatmullRom(p0, p1, p2, p3, tNorm));
            }
        }

        smoothPoints.Add(points[points.Count - 1]); // Add the last point directly
        return smoothPoints;
    }

    // Catmull-Rom spline interpolation formula
    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        // Catmull-Rom spline calculation using 4 control points
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
    }
}
