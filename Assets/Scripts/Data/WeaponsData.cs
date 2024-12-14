using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
  Cannonball,
}

[System.Serializable]
public struct WeaponDetail
{
  public bool isEnabled;
  public string slug;
  public GameObject projectilePrefab;
  public ProjectileType type;
  public int projectileSpawnCount;
  public float maxProjectileLifeTime;
  public float projectileMass;
  public bool explodeOnImpact;
}

[System.Serializable]
public class WeaponsData
{
  [SerializeField] public List<WeaponDetail> weapons = new();
}