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

    private void EnsureDefaultUnlocks()
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
