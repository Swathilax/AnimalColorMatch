using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private const string COINS_KEY = "PlayerCoins";

    [Header("Coin UI")]
    public TMP_Text[] coinTexts;

    private readonly List<TMP_Text> _dynamicCoinTexts = new List<TMP_Text>();

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

        LoadCoins();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        RefreshAndFindUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadCoins();
        RefreshAndFindUI();
    }

    public void LoadCoins()
    {
        Coins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    public void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, Coins);
        PlayerPrefs.Save();
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

    public void SetCoins(int amount)
    {
        Coins = Mathf.Max(0, amount);
        SaveCoins();
        UpdateCoinUI();
    }

    [ContextMenu("Reset Coins to 0")]
    public void ResetCoins()
    {
        Coins = 0;
        SaveCoins();
        UpdateCoinUI();
    }

    public void RegisterCoinText(TMP_Text text)
    {
        if (text == null)
            return;

        if (!_dynamicCoinTexts.Contains(text))
        {
            _dynamicCoinTexts.Add(text);
        }

        text.text = Coins.ToString();
    }

    public void UnregisterCoinText(TMP_Text text)
    {
        if (text == null)
            return;

        _dynamicCoinTexts.Remove(text);
    }

    public void RefreshAndFindUI()
    {
        _dynamicCoinTexts.RemoveAll(t => t == null);

        TMP_Text[] allTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text t in allTexts)
        {
            if (t == null) continue;

            string objName = t.gameObject.name.ToLower();
            string parentName = t.transform.parent != null ? t.transform.parent.gameObject.name.ToLower() : "";

            if (objName.Contains("coin") || parentName.Contains("coin"))
            {
                RegisterCoinText(t);
            }
        }

        UpdateCoinUI();
    }

    public void UpdateCoinUI()
    {
        if (coinTexts != null)
        {
            foreach (TMP_Text text in coinTexts)
            {
                if (text != null)
                    text.text = Coins.ToString();
            }
        }

        for (int i = _dynamicCoinTexts.Count - 1; i >= 0; i--)
        {
            if (_dynamicCoinTexts[i] != null)
            {
                _dynamicCoinTexts[i].text = Coins.ToString();
            }
            else
            {
                _dynamicCoinTexts.RemoveAt(i);
            }
        }
    }
}