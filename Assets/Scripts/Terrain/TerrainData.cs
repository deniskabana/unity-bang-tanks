using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TerrainData
{
	public float PixelsPerUnit;
	public Texture2D HeightmapTexture;
	public int TextureWidth;
	public int TextureHeight;
	public float[] Heightmap;
	public bool EnableTerrainChunking;
	public int ChunkSize;
	public bool Initialized;
}