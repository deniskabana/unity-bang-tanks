using UnityEngine;

public class PrintAwake : MonoBehaviour
{
  public bool debug = false;
  [SerializeField] Transform GameManagerObject;

  void OnValidate()
  {
    // Only run when the game is not playing
    if (Application.isPlaying) return;

    LevelManager lm = GameManagerObject.GetComponent<LevelManager>();
    TerrainSettings terrainSettings = lm.terrainSettings;
    TerrainManager tm = GameManagerObject.GetComponent<TerrainManager>();
    tm.Initialize(terrainSettings, true);

    if (debug)
    {
      Debug.Log("Initializing terrain preview for editor. FEEL FREE TO CLEAR CONSOLE");
    }

    Debug.ClearDeveloperConsole();
  }
}