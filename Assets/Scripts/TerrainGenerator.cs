using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private SpriteShapeController spriteShapeController;
    [SerializeField, Range(3f, 100f)] private int levelLength = 50;
    [SerializeField, Range(1f, 50f)] private int xMultiplier = 2;
    [SerializeField, Range(1f, 50f)] private int yMultiplier = 2;
    [SerializeField, Range(0f, 1f)] private float curveSmoothness = 0.5f;
    [SerializeField] private float noiseStep = 0.5f;
    [SerializeField] private float bottom = 10f;

    private Vector3 lastPosition;

    private void OnValidate()
    {
        noiseStep = Random.Range(0.1f, 3f);
        spriteShapeController.spline.Clear();

        for (int i = 0; i < levelLength; i++)
        {
            lastPosition = transform.position + new Vector3(i * xMultiplier, Mathf.PerlinNoise(transform.position.y, transform.position.y + i * noiseStep) * yMultiplier);
            spriteShapeController.spline.InsertPointAt(i, lastPosition);

            if (i > 0)
            {
                spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                spriteShapeController.spline.SetLeftTangent(i, Vector3.left * xMultiplier * curveSmoothness);
                spriteShapeController.spline.SetRightTangent(i, Vector3.right * xMultiplier * curveSmoothness);
            }
        }

        spriteShapeController.spline.InsertPointAt(levelLength, new Vector3(lastPosition.x, transform.position.y - bottom));
        spriteShapeController.spline.InsertPointAt(levelLength + 1, new Vector3(transform.position.x, transform.position.y - bottom));
    }

    public void Generate()
    {
        OnValidate();
    }

    public void CreateHole(float radius, float xPosition)
    {
        int pointCount = 20; // Number of points to approximate the circle
        float angleStep = 360f / pointCount;
        List<Vector3> newPoints = new List<Vector3>();

        // Find the y position at the given x position
        float yPosition = 0f;
        for (int i = 0; i < spriteShapeController.spline.GetPointCount(); i++)
        {
            Vector3 point = spriteShapeController.spline.GetPosition(i);
            if (Mathf.Approximately(point.x, xPosition))
            {
                yPosition = point.y;
                break;
            }
        }

        // Generate points for the circular hole
        for (int i = 0; i < pointCount; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            float x = xPosition + Mathf.Cos(angle) * radius;
            float y = yPosition + Mathf.Sin(angle) * radius;
            newPoints.Add(new Vector3(x, y));
        }

        // Insert new points into the spline
        foreach (var newPoint in newPoints)
        {
            spriteShapeController.spline.InsertPointAt(spriteShapeController.spline.GetPointCount(), newPoint);
        }

        // Ensure the tangents are set correctly for smoothness
        for (int i = 0; i < spriteShapeController.spline.GetPointCount(); i++)
        {
            spriteShapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            spriteShapeController.spline.SetLeftTangent(i, Vector3.left * xMultiplier * curveSmoothness);
            spriteShapeController.spline.SetRightTangent(i, Vector3.right * xMultiplier * curveSmoothness);
        }
    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse down detected!");
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D collider = Physics2D.OverlapPoint(mousePosition);
            if (collider != null && collider.gameObject == gameObject)
            {
                Debug.Log("Mouse down detected on terrain!");
            }
        }
    }
}
