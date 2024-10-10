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

        // DEBUG
        Spline spline = spriteShapeController.spline;
        int pointCount = spline.GetPointCount();
        Debug.Log($"Spline has {pointCount} points.");

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 point = spline.GetPosition(i);
            Debug.Log($"Point {i}: (X: {point.x}, Y: {point.y})");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse down detected!");
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
            bool foundAffectedPoints = false;
            Vector3 explosionPosition = new Vector3(explosion.x, explosion.y, 0);
            Vector3 terrainPosition = transform.position;
            Vector3 explosionLocalPosition = new Vector3(explosion.x, explosion.y, 0) - transform.position;


            // Log the adjusted positions
            Debug.Log($"Explosion (world): {explosionPosition}, Explosion (local): {explosionLocalPosition}");

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 point = spriteShapeController.transform.TransformPoint(spline.GetPosition(i));
                float distanceToExplosion = Vector2.Distance(new Vector2(point.x, point.y), new Vector2(explosionPosition.x, explosionPosition.y));

                if (distanceToExplosion <= explosion.radius + 0.01f)
                {
                    foundAffectedPoints = true;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(point, 0.1f);
                }
            }

            float leftEdge = explosionLocalPosition.x - explosion.radius;
            float rightEdge = explosionLocalPosition.x + explosion.radius;
            Debug.Log($"Adjusted Explosion Local Position: {explosionLocalPosition}, Left edge X: {leftEdge}, Right edge X: {rightEdge}");
            float leftY = GetLinearInterpolatedY(spline, leftEdge);
            float rightY = GetLinearInterpolatedY(spline, rightEdge);
            Debug.Log($"Interpolated Y for left edge: {leftY}, right edge: {rightY}");

            Vector3 leftPointWorld = new Vector3(leftEdge, leftY, 0) + transform.position;
            Vector3 rightPointWorld = new Vector3(rightEdge, rightY, 0) + transform.position;


            // Draw blue Gizmos at the positions where the new points would be added
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(leftPointWorld, 0.1f);
            Gizmos.DrawSphere(rightPointWorld, 0.1f);

            if (!foundAffectedPoints)
            {
                // TODO: if necessary
            }
        }
    }

    // Custom methods
    // --------------------------------------------------

    private float GetLinearInterpolatedY(Spline spline, float xPosition)
    {
        int pointCount = spline.GetPointCount();

        // Find the two points surrounding the xPosition
        for (int i = 0; i < pointCount - 1; i++)
        {
            Vector3 p1 = spline.GetPosition(i);
            Vector3 p2 = spline.GetPosition(i + 1);

            if (p1.x <= xPosition && p2.x >= xPosition)
            {
                // Perform linear interpolation between p1 and p2
                float t = (xPosition - p1.x) / (p2.x - p1.x);
                float interpolatedY = Mathf.Lerp(p1.y, p2.y, t);
                Debug.Log($"Linear interpolation between ({p1.x}, {p1.y}) and ({p2.x}, {p2.y}), result: {interpolatedY}");
                return interpolatedY;
            }
        }

        // If xPosition is outside the range of the spline, return the nearest endpoint's Y
        if (xPosition < spline.GetPosition(0).x)
        {
            Debug.LogWarning($"xPosition {xPosition} is before the first point. Using Y: {spline.GetPosition(0).y}");
            return spline.GetPosition(0).y;
        }
        else if (xPosition > spline.GetPosition(pointCount - 1).x)
        {
            Debug.LogWarning($"xPosition {xPosition} is after the last point. Using Y: {spline.GetPosition(pointCount - 1).y}");
            return spline.GetPosition(pointCount - 1).y;
        }

        // In case something goes wrong (shouldn't happen)
        Debug.LogError("Failed to interpolate Y value.");
        return 0;
    }

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
