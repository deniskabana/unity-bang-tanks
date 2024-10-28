using System.Collections.Generic;
using UnityEngine;

// TODO: Could possibly be a strict singleton class
// once I figure out how to properly implement it
[System.Serializable]
public struct TerrainData
{
	public bool Initialized;
	// Terrain settings
	public int TextureWidth;
	public int TextureHeight;
	// Advanced terrain settings
	public bool EnableTerrainChunking;
	public int ChunkSize;
	// Graphics settings
	public float PixelsPerUnit;
	// Generated at run-time
	public float[] Heightmap;
	public Texture2D HeightmapTexture;
}