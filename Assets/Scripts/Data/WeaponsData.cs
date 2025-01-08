using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
  Cannonball,
  Grenade,
}

[System.Serializable]
public struct WeaponDetail
{
  public bool isEnabled;
  public string slug;
  public GameObject projectilePrefab;
  public Sprite hudIcon;
  public ProjectileType type;
  public int baseDamage;
  public int projectileSpawnCount;
  public float maxProjectileLifeTime;
  public float projectileMass;
  public bool explodeOnImpact;
}