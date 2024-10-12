using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    [SerializeField, Range(3f, 100f)] private float terrainWidth = 25;
    [SerializeField, Range(3f, 100f)] private float terrainHeight = 8;
    [SerializeField, Range(0.01f, 1)] private float minHeightFactor = 0.15f;
    [SerializeField, Range(0.01f, 1)] private float maxHeightFactor = 0.9f;
    [SerializeField, Range(0, 6)] private int hillCount = 3;
    [SerializeField, Range(1, 80)] private int resolution = 50;
    [Header("References")]
    [SerializeField] private SpriteShapeController spriteShapeController;
    [Header("Debug")]
    [SerializeField] private bool showAdvancedGizmos = false;

    // Private variables
    // --------------------------------------------------

    private struct Explosion
    {
        public float radius;
        public float x;
        public float y;

        public Explosion(float radius, float x, float y)
        {
            this.radius = radius;
            this.x = x;
            this.y = y;
        }
    }
    private List<Explosion> explosions = new List<Explosion>();

    // Built-in methods
    // --------------------------------------------------

    private void OnValidate()
    {
        GenerateTerrain();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HandleExplosion(0.8f, mousePosition.x, mousePosition.y);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showAdvancedGizmos) return;
        if (spriteShapeController == null) return;

        Spline spline = spriteShapeController.spline;
        int pointCount = spline.GetPointCount();

        // Set Gizmos color to uncolored (white)
        Gizmos.color = Color.white;

        // Get the position offset of the Sprite Shape
        Vector3 offset = spriteShapeController.transform.position;

        // Draw a small circle at each point's position
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 pointPosition = spline.GetPosition(i) + offset;
            Gizmos.DrawWireSphere(pointPosition, 0.1f); // Adjust the radius (0.1f) as needed
        }

        // Draw lines between the points to visualize the spline
        Gizmos.color = Color.white;
        for (int i = 0; i < pointCount - 1; i++)
        {
            Vector3 startPoint = spline.GetPosition(i) + offset;
            Vector3 endPoint = spline.GetPosition(i + 1) + offset;
            Gizmos.DrawLine(startPoint, endPoint);
        }

        // Draw a Gizmo circle for each explosion in the list
        Gizmos.color = Color.red;
        foreach (var explosion in explosions)
        {
            Gizmos.DrawWireSphere(new Vector3(explosion.x, explosion.y, 0), explosion.radius);
        }

        // Draw Gizmos for explosion affected points
        if (explosions.Count == 0) return;

        foreach (var explosion in explosions)
        {
            Vector3 explosionPosition = new Vector3(explosion.x, explosion.y, 0);
            // Vector3 terrainPosition = transform.position;
            // Vector3 explosionLocalPosition = new Vector3(explosion.x, explosion.y, 0) - transform.position;


            for (int i = 0; i < pointCount; i++)
            {
                Vector3 point = spriteShapeController.transform.TransformPoint(spline.GetPosition(i));
                float distanceToExplosion = Vector2.Distance(new Vector2(point.x, point.y), new Vector2(explosionPosition.x, explosionPosition.y));

                if (distanceToExplosion <= explosion.radius + 0.01f)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(point, 0.1f);
                }
            }
        }
    }

    // Custom methods
    // --------------------------------------------------

    public void HandleExplosion(float explosionRadius, float explosionX, float explosionY)
    {
        // Step 1: Store the explosion data
        explosions.Add(new Explosion(explosionRadius, explosionX, explosionY));
        Debug.Log($"Explosion added at ({explosionX}, {explosionY}) with radius {explosionRadius}");
    }

    public void GenerateTerrain()
    {
        // Clear existing terrain points
        Spline spline = spriteShapeController.spline;
        spline.Clear();

        // Calculate noise scale
        float noiseScale = hillCount / (float)terrainWidth; // Ensures smooth hills

        // Adjusted height range
        float minHeight = terrainHeight * minHeightFactor; // Minimum height (adjust as needed)
        float maxHeight = terrainHeight * maxHeightFactor; // Maximum height (adjust as needed)

        // Generate terrain points
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution; // Normalized value from 0 to 1
            float x = t * terrainWidth; // X position across the entire width

            // Use Perlin noise to generate smooth Y values for hills
            float noiseValue = Mathf.PerlinNoise(x * noiseScale, 0f);
            float y = Mathf.Lerp(minHeight, maxHeight, noiseValue);

            // Insert point into the Sprite Shape spline
            spline.InsertPointAt(i, new Vector3(x, y, 0));
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }

        // Properly close the terrain at both ends
        spline.InsertPointAt(spline.GetPointCount(), new Vector3(terrainWidth, 0, 0)); // Right bottom edge
        spline.InsertPointAt(spline.GetPointCount(), new Vector3(0, 0, 0)); // Left bottom edge

        // Refresh the Sprite Shape to apply the changes
        spriteShapeController.RefreshSpriteShape();
    }
}
