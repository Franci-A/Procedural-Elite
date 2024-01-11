using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Threshold")]
public class ExperienceThresholdSO : ScriptableObject
{
    public List<threshold> thresholds;
    public IntVariable playerLevel;

    public threshold GetCurrentThreshold()
    {
        return thresholds[playerLevel.Value];
    }
}

[Serializable]
public struct threshold
{
    public float expNeeded;
    public float attackDamage;
    public float attackCooldown;
}
