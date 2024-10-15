using UnityEngine;
using System.IO;

public class TextureFromJpeg : MonoBehaviour
{
    [SerializeField] string jpegFilePath = "Assets/YourJPEGFile.jpg";

    public Texture2D loadedTexture;

    void Start()
    {
        loadedTexture = LoadTextureFromFile(jpegFilePath);
    }

    void OnValidate()
    {
        loadedTexture = LoadTextureFromFile(jpegFilePath);
    }

    Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData)) //LoadImage will auto-resize the texture dimensions
        {
            return tex;
        }
        else
        {
            Debug.LogError("Failed to load texture from file: " + filePath);
            return null;
        }
    }
}