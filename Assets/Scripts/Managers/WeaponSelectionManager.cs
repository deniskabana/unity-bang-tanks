using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelectionManager : MonoBehaviour
{
    public static WeaponSelectionManager Instance;

    public bool debug = false;

    [Header("Weapons data")]
    [SerializeField] private List<WeaponDetail> weaponsData;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
