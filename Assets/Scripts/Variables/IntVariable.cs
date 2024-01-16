using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Variable/IntVariable")]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int value;
    public int Value => value;
    public UnityEvent OnValueChanged;

    public void SetValue(int newValue)
    {
        value = newValue;
        OnValueChanged?.Invoke();
    }

    public void AddValue(int _value)
    {
        value += _value;
        OnValueChanged?.Invoke();
    }
}
