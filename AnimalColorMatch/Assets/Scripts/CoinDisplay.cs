using UnityEngine;
using TMPro;

public class CoinDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    private void Awake()
    {
        if (coinText == null)
        {
            coinText = GetComponent<TMP_Text>();
            if (coinText == null)
                coinText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void OnEnable()
    {
        if (coinText == null)
        {
            coinText = GetComponent<TMP_Text>();
            if (coinText == null)
                coinText = GetComponentInChildren<TMP_Text>(true);
        }

        RefreshDisplay();

        if (CoinManager.Instance != null && coinText != null)
        {
            CoinManager.Instance.RegisterCoinText(coinText);
        }
    }

    private void Start()
    {
        RefreshDisplay();
    }

    private void OnDisable()
    {
        if (CoinManager.Instance != null && coinText != null)
        {
            CoinManager.Instance.UnregisterCoinText(coinText);
        }
    }

    public void RefreshDisplay()
    {
        if (coinText == null)
            return;

        int currentCoins = CoinManager.Instance != null 
            ? CoinManager.Instance.Coins 
            : PlayerPrefs.GetInt("PlayerCoins", 0);

        coinText.text = currentCoins.ToString();
    }
}
