using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Threshold")]
public class ExperienceThresholdSO : ScriptableObject
{
    public List<threshold> thresholds;
}

[Serializable]
public struct threshold
{
    public float expNeeded;
    public float attackDamage;
    public float attackSpeed;
}
