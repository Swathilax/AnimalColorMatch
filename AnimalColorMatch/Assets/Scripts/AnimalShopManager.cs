using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalShopManager : MonoBehaviour
{
    public static AnimalShopManager Instance { get; private set; }

    public const string ANIMAL_BEAR = "Bear";
    public const string ANIMAL_FOX = "Fox";
    public const string ANIMAL_ELEPHANT = "Elephant";
    public const string ANIMAL_TIGER = "Tiger";
    public const string ANIMAL_LION = "Lion";

    public const string UNLOCKED_PREF_PREFIX = "AnimalUnlocked_";
    public const string BUILD_GUID_KEY = "Build_Instance_GUID";

    public static readonly Dictionary<string, int> AnimalPrices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { ANIMAL_BEAR, 0 },
        { ANIMAL_FOX, 500 },
        { ANIMAL_ELEPHANT, 750 },
        { ANIMAL_TIGER, 1000 },
        { ANIMAL_LION, 1500 }
    };

    public static event Action<string> OnAnimalUnlocked;
    public static event Action OnShopUpdated;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void CheckBuildInitialization()
    {
        string currentBuildId = Application.buildGUID;
        if (string.IsNullOrEmpty(currentBuildId))
        {
            currentBuildId = "DEV_BUILD_" + Application.version;
        }

        string savedBuildId = PlayerPrefs.GetString(BUILD_GUID_KEY, "");

        if (!PlayerPrefs.HasKey(BUILD_GUID_KEY) || savedBuildId != currentBuildId)
        {
            Debug.Log("[AnimalShopManager] New build detected (ID: " + currentBuildId + "). Initializing fresh player state: 0 coins, Bear default.");
            ResetToFreshPlayerState();
            PlayerPrefs.SetString(BUILD_GUID_KEY, currentBuildId);
            PlayerPrefs.Save();
        }
        else
        {
            EnsureDefaultUnlocks();
        }
    }

    [ContextMenu("Reset to Fresh Player State")]
    public void ContextMenuResetPlayerState()
    {
        ResetToFreshPlayerState();
    }

    public static void ResetToFreshPlayerState()
    {
        // 1. Reset Coins to 0
        PlayerPrefs.SetInt("PlayerCoins", 0);
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoins();
        }

        // 2. Lock all animals except Bear
        string[] lockableAnimals = { ANIMAL_FOX, ANIMAL_ELEPHANT, ANIMAL_TIGER, ANIMAL_LION };
        foreach (string animal in lockableAnimals)
        {
            PlayerPrefs.DeleteKey(UNLOCKED_PREF_PREFIX + animal);
            PlayerPrefs.SetInt(UNLOCKED_PREF_PREFIX + animal, 0);
        }

        // 3. Ensure Bear is default unlocked
        PlayerPrefs.SetInt(UNLOCKED_PREF_PREFIX + ANIMAL_BEAR, 1);
        PlayerPrefs.Save();

        Debug.Log("[AnimalShopManager] Player state reset complete: Coins = 0, Active Default Animal = Bear, Locked Animals = Fox, Elephant, Tiger, Lion.");

        OnShopUpdated?.Invoke();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureDefaultUnlocks();
    }

    private static void EnsureDefaultUnlocks()
    {
        if (!PlayerPrefs.HasKey(UNLOCKED_PREF_PREFIX + ANIMAL_BEAR))
        {
            PlayerPrefs.SetInt(UNLOCKED_PREF_PREFIX + ANIMAL_BEAR, 1);
            PlayerPrefs.Save();
        }
    }

    public static bool IsAnimalUnlocked(string animalName)
    {
        if (string.IsNullOrEmpty(animalName))
            return false;

        string normalized = NormalizeAnimalName(animalName);

        if (normalized.Equals(ANIMAL_BEAR, StringComparison.OrdinalIgnoreCase))
            return true;

        return PlayerPrefs.GetInt(UNLOCKED_PREF_PREFIX + normalized, 0) == 1;
    }

    public static bool BuyAnimal(string animalName)
    {
        if (string.IsNullOrEmpty(animalName))
            return false;

        string normalized = NormalizeAnimalName(animalName);

        if (IsAnimalUnlocked(normalized))
        {
            return true;
        }

        int price = GetAnimalPrice(normalized);

        if (CoinManager.Instance != null)
        {
            if (!CoinManager.Instance.SpendCoins(price))
            {
                return false;
            }
        }
        else
        {
            int currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
            if (currentCoins < price)
            {
                return false;
            }

            PlayerPrefs.SetInt("PlayerCoins", currentCoins - price);
            PlayerPrefs.Save();
        }

        PlayerPrefs.SetInt(UNLOCKED_PREF_PREFIX + normalized, 1);
        PlayerPrefs.Save();

        OnAnimalUnlocked?.Invoke(normalized);
        OnShopUpdated?.Invoke();

        return true;
    }

    public static void UnlockAnimalDirectly(string animalName)
    {
        if (string.IsNullOrEmpty(animalName))
            return;

        string normalized = NormalizeAnimalName(animalName);
        PlayerPrefs.SetInt(UNLOCKED_PREF_PREFIX + normalized, 1);
        PlayerPrefs.Save();

        OnAnimalUnlocked?.Invoke(normalized);
        OnShopUpdated?.Invoke();
    }

    public static int GetAnimalPrice(string animalName)
    {
        string normalized = NormalizeAnimalName(animalName);
        if (AnimalPrices.TryGetValue(normalized, out int price))
        {
            return price;
        }

        return 500;
    }

    public static List<string> GetAllAnimals()
    {
        return new List<string>
        {
            ANIMAL_BEAR,
            ANIMAL_FOX,
            ANIMAL_ELEPHANT,
            ANIMAL_TIGER,
            ANIMAL_LION
        };
    }

    public static List<string> GetUnlockedAnimals()
    {
        List<string> unlocked = new List<string>();
        foreach (string animal in GetAllAnimals())
        {
            if (IsAnimalUnlocked(animal))
            {
                unlocked.Add(animal);
            }
        }
        return unlocked;
    }

    public static string GetLatestUnlockedAnimal()
    {
        List<string> all = GetAllAnimals();
        string latest = ANIMAL_BEAR;
        for (int i = all.Count - 1; i >= 0; i--)
        {
            if (IsAnimalUnlocked(all[i]))
            {
                return all[i];
            }
        }
        return latest;
    }

    public static List<string> GetPreviousUnlockedAnimals()
    {
        List<string> all = GetAllAnimals();
        string latest = GetLatestUnlockedAnimal();
        List<string> previous = new List<string>();

        foreach (string animal in all)
        {
            if (animal.Equals(latest, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (IsAnimalUnlocked(animal))
            {
                previous.Add(animal);
            }
        }

        return previous;
    }

    public static string NormalizeAnimalName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ANIMAL_BEAR;

        string lower = name.Trim().ToLower();

        if (lower.Contains("bear")) return ANIMAL_BEAR;
        if (lower.Contains("fox")) return ANIMAL_FOX;
        if (lower.Contains("elephant")) return ANIMAL_ELEPHANT;
        if (lower.Contains("tiger")) return ANIMAL_TIGER;
        if (lower.Contains("lion")) return ANIMAL_LION;

        return name.Trim();
    }
}
