using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variable/FloatVariable")]
public class FloatVariable : ScriptableObject
{
    [SerializeField] private float value;
    public float Value => value;

    public void SetValue(float newValue)
    {
        value = newValue;
    }

    public void AddValue(float _value)
    {
        value += _value;
    }
}
