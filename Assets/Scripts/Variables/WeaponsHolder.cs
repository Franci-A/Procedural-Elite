using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName ="Weapon Holder")]
public class WeaponsHolder : ScriptableObject
{
    public List<WeaponObject> weapons;
    private int selectedIndex;
    public IntVariable playerExp;
    public UnityEvent OnClassChanged;

    public GameObject GetWeaponPrefab()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].index == selectedIndex)
                return weapons[i].weapon;
        }

        return null;
    }
    
    public WeaponObject GetWeapon()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].index == selectedIndex)
                return weapons[i];
        }

        return null;
    }
    
    public void SetSelectedWeapons(int i)
    {
        selectedIndex = i;
        OnClassChanged?.Invoke();
    }

    public threshold GetCurrentThreshold()
    {
        int index = 0;
        WeaponObject weapon = GetWeapon();
        for (int i = 0; i < weapon.thresholds.Count; i++)
        {
            if (weapon.thresholds[i].expNeeded <= playerExp.Value)
            {
                index = i;
            }
            else break;
        }
        return weapon.thresholds[index];
    }

    public Sprite GetPlayerSprite()
    {
        return weapons[selectedIndex].playerImage;
    }

    public void SwitchWeapon()
    {
        selectedIndex++;
        selectedIndex = Mathf.FloorToInt(Mathf.Repeat(selectedIndex, weapons.Count));
        OnClassChanged?.Invoke();
    }
}

[Serializable]
public class WeaponObject
{
    public int index;
    public GameObject weapon;
    public bool canDash = false;
    public Sprite playerImage;

    public List<threshold> thresholds;
}


[Serializable]
public struct threshold
{
    public float expNeeded;
    public float attackDamage;
    public float attackCooldown;
}
