using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private const string COINS_KEY = "PlayerCoins";

    [Header("Coin UI")]
    public TMP_Text[] coinTexts;

    public int Coins { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Coins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    private void Start()
    {
        UpdateCoinUI();
    }

    public void AddCoins(int amount)
    {
        Coins += amount;

        SaveCoins();
        UpdateCoinUI();
    }

    public bool SpendCoins(int amount)
    {
        if (Coins < amount)
            return false;

        Coins -= amount;

        SaveCoins();
        UpdateCoinUI();

        return true;
    }

    public bool CanAfford(int amount)
    {
        return Coins >= amount;
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, Coins);
        PlayerPrefs.Save();
    }

    public void UpdateCoinUI()
    {
        if (coinTexts == null)
            return;

        foreach (TMP_Text text in coinTexts)
        {
            if (text != null)
                text.text = Coins.ToString();
        }
    }
}