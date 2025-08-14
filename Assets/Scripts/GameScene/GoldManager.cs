// GoldManager.cs
using TMPro;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [SerializeField] private int startingGold = 30;
    [SerializeField] private TextMeshProUGUI goldText;

    private int currentGold;
    public int GetCurrentGold() => currentGold;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentGold = startingGold;
        UpdateGoldUI();
    }

    public bool TrySpendGold(int amount)
    {
        if (currentGold < amount)
        {
            Debug.Log("❌ 골드 부족! 필요한 골드: " + amount);
            return false;
        }

        currentGold -= amount;
        UpdateGoldUI();
        return true;
    }
    public void SetGold(int amount)
    {
        currentGold = amount;
        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = currentGold.ToString();
    }
}
