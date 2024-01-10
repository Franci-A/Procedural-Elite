using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Weapon Holder")]
public class WeaponsHolder : ScriptableObject
{
    public List<WeaponObject> weapons;
    private int selectedIndex;

    public GameObject GetWeaponPrefab()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].index == selectedIndex)
                return weapons[i].weapon;
        }

        return null;
    }
    
    public void SetSelectedWeapons(int i)
    {
        selectedIndex = i;
    }
}

[Serializable]
public struct WeaponObject
{
    public int index;
    public GameObject weapon;
}
