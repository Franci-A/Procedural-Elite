using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ProceduralData")]
public class ProceduralData : ScriptableObject
{
    [SerializeField] public bool generateGoldenPath = true;
    /// <summary>
    /// Number of Rooms used for the Golden Path
    /// </summary>
    [SerializeField, ShowIf(nameof(generateGoldenPath)), MinMaxSlider(0, 50)] public Vector2Int goldenPathRoomRange;
    /// <summary>
    /// Number of Rooms used for a Side Path
    /// </summary>
    [SerializeField, ShowIf(nameof(generateGoldenPath)), MinMaxSlider(1, 50)] public Vector2Int sidePathRoomRange;
    [SerializeField, ShowIf(nameof(generateGoldenPath))] public int sidePathsNumber;

    [SerializeField, HideIf(nameof(generateGoldenPath)), MinMaxSlider(1, 50)] public Vector2Int pathRoomRange;
}
