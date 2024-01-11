using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    private static ExperienceManager instance;
    public static ExperienceManager Instance { get { return instance; } }

    private int expLevel = 0;
    [SerializeField] private ExperienceThresholdSO expThresholds;
    [SerializeField] private IntVariable expThreshold;
    private int currentThresholdIndex = 0;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        expThreshold.SetValue(0);
        expLevel = 0;
    }

    public void AddExp(int value)
    {
        expLevel += value;
        Debug.Log("exp : " + expLevel);
        if (expLevel < expThresholds.thresholds[currentThresholdIndex].expNeeded)
            return;
                
        currentThresholdIndex++;
        currentThresholdIndex = Mathf.Clamp(currentThresholdIndex, 0, expThresholds.thresholds.Count -1);
        expThreshold.SetValue(currentThresholdIndex);
    }
}