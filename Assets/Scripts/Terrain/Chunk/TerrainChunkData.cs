using System.Collections.Generic;
using UnityEngine;

public struct TerrainChunkData
{
  public float PixelsPerUnit { get; set; }
  public bool[,] ChunkHeightmapGrid { get; set; }
  public int TerrainGridCellSize { get; set; }
  public Texture2D ChunkHeightmapTexture { get; set; }

  public TerrainChunkData(float pixelsPerUnit, bool[,] chunkHeightmapGrid, int terrainGridCellSize, Texture2D chunkHeightmapTexture)
  {
    this.PixelsPerUnit = pixelsPerUnit;
    this.ChunkHeightmapGrid = chunkHeightmapGrid;
    this.TerrainGridCellSize = terrainGridCellSize;
    this.ChunkHeightmapTexture = chunkHeightmapTexture;
  }
}