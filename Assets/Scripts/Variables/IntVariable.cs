using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variable/IntVariable")]
public class IntVariable : ScriptableObject
{
    [SerializeField] private int value;
    public int Value => value;

    public void SetValue(int newValue)
    {
        value = newValue;
    }

    public void AddValue(int _value)
    {
        value += _value;
    }
}
