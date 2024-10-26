using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TerrainChunkData
{
  public float PixelsPerUnit;
  public int ChunkSize;
  public Texture2D HeightmapTexture;
  public float[] Heightmap;
}