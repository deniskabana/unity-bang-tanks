using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelectionManager : MonoBehaviour
{
    public static WeaponSelectionManager Instance;

    public bool debug = false;

    [Header("Weapons data")]
    [SerializeField] private List<WeaponDetail> weaponsData;

    // Built-in methods
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (!Instance) Instance = this;
    }

    // Custom methods
    // --------------------------------------------------

    public static WeaponDetail GetWeaponDetail(int index)
    {
        if (Instance.debug) Debug.Log("WeaponSelectionManager - GetWeaponDetail(" + index + ")");
        return Instance.weaponsData[Mathf.Clamp(index, 0, Instance.weaponsData.Count - 1)];
    }
}
