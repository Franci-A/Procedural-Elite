using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpSliderUI : MonoBehaviour
{
    [SerializeField] private IntVariable playerExp;
    [SerializeField] private IntVariable playerLastLevel;
    [SerializeField] private IntVariable playerNextLevel;
    [SerializeField] private Slider slider;
    [SerializeField] private WeaponsHolder weaponsHolder;

    private void Start()
    {
        float maxValue = 0;
        foreach (var weapon in weaponsHolder.GetWeapon().thresholds)
        {
            if (weapon.expNeeded > maxValue)
                maxValue = weapon.expNeeded;
        }
        slider.maxValue = maxValue;
        playerExp.OnValueChanged.AddListener(UpdateSlider);
    }

    private void UpdateSlider()
    {
        slider.value = playerExp.Value;

        slider.minValue = playerLastLevel.Value;
        slider.maxValue = playerNextLevel.Value;

        Debug.Log($"Last {playerLastLevel.Value}, current {playerExp.Value}, next {playerNextLevel.Value}");
    }

}
