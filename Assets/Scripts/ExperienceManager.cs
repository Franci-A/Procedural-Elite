using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    private static ExperienceManager instance;
    public static ExperienceManager Instance { get { return instance; } }

    private int expLevel = 0;
    [SerializeField] private WeaponsHolder weaponsHolder;
    [SerializeField] private IntVariable playerExp;
    [SerializeField] private IntVariable playerLastLevel;
    [SerializeField] private IntVariable playerNextLevel;
    private int currentThresholdIndex = 0;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        playerExp.SetValue(0);
        playerLastLevel.SetValue(0);
        playerNextLevel.SetValue(1);
        expLevel = 0;
    }

    public void AddExp(int value)
    {
        expLevel += value;

        if(currentThresholdIndex > 0 && value < 0 && expLevel < weaponsHolder.GetWeapon().thresholds[currentThresholdIndex - 1].expNeeded)
        {
            currentThresholdIndex--;
            currentThresholdIndex = Mathf.Clamp(currentThresholdIndex, 0, weaponsHolder.GetWeapon().thresholds.Count -1);
        }

        playerExp.SetValue(expLevel);

        if (expLevel < weaponsHolder.GetWeapon().thresholds[currentThresholdIndex].expNeeded)
            return;
                
            playerLastLevel.SetValue((int)weaponsHolder.GetWeapon().thresholds[currentThresholdIndex].expNeeded + 1);
        currentThresholdIndex++;
        currentThresholdIndex = Mathf.Clamp(currentThresholdIndex, 0, weaponsHolder.GetWeapon().thresholds.Count -1);
            playerNextLevel.SetValue((int)weaponsHolder.GetWeapon().thresholds[currentThresholdIndex].expNeeded);


        playerExp.SetValue(expLevel);
    }
}