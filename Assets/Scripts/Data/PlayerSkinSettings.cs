using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerSkin
{
  public string name;
  public Sprite body1;
  public Sprite body2;
  public bool isLocked;
}

[System.Serializable]
public class PlayerSkinSettings
{
  [SerializeField] public List<PlayerSkin> playerSkins = new();
}