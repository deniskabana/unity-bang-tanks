using UnityEngine;

[System.Serializable]
public class TerrainSettings
{
  [Header("Terrain Settings")]
  public int textureWidth = 1920;  // The width of the terrain (in pixels)
  public int textureHeight = 1080; // The max height of the terrain (in pixels)
  public float noiseScale = 0.0022f; // Controls the frequency of hills and valleys
  public float heightMultiplier = 600f; // Controls the maximum height

  [Header("Advanced terrain settings")]
  public bool enableTerrainChunking = true;
  public float pixelsPerUnit = 100f;
  public float terrainScale = 1f;
  public float minRandomOffset = 0; // The maximum random offset for Perlin noise
  public float maxRandomOffset = 1000f; // The maximum random offset for Perlin noise
  [Range(2, 2048)] public int chunkSize = 256; // Size in pixels by which we will subdivide the terrain
}