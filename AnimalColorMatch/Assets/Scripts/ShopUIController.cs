using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ShopUIController : MonoBehaviour
{
    [System.Serializable]
    public class ShopAnimalItem
    {
        public string animalName;
        public Button buyButton;
        public TMP_Text priceText;
        public GameObject ownedIndicator;
    }

    [Header("Shop Panel & Buttons")]
    public GameObject shopPanel;
    public Button openShopButton;
    public Button closeShopButton;

    [Header("Animal Shop Items")]
    public List<ShopAnimalItem> animalItems = new List<ShopAnimalItem>();

    [Header("Owned Styling")]
    public float ownedScaleMultiplier = 1.08f;

    private readonly Dictionary<GameObject, Vector3> _initialScales = new Dictionary<GameObject, Vector3>();

    private void Awake()
    {
        RebindSceneElements();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        AnimalShopManager.OnShopUpdated += RefreshShopUI;
        RebindSceneElements();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        AnimalShopManager.OnShopUpdated -= RefreshShopUI;
    }

    private void Start()
    {
        RebindSceneElements();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindSceneElements();
    }

    public void RebindSceneElements()
    {
        _initialScales.Clear();

        if (shopPanel == null)
        {
            shopPanel = null;
        }

        if (openShopButton == null)
        {
            openShopButton = null;
        }

        if (closeShopButton == null)
        {
            closeShopButton = null;
        }

        AutoWireElements();
        RefreshShopUI();
    }

    private void CacheInitialScales()
    {
        if (animalItems == null) return;

        foreach (ShopAnimalItem item in animalItems)
        {
            if (item != null && item.ownedIndicator != null)
            {
                if (!_initialScales.ContainsKey(item.ownedIndicator))
                {
                    _initialScales[item.ownedIndicator] = item.ownedIndicator.transform.localScale;
                }
            }
        }
    }

    private Vector3 GetInitialScale(GameObject obj)
    {
        if (obj == null) return Vector3.one;

        if (!_initialScales.TryGetValue(obj, out Vector3 scale))
        {
            scale = obj.transform.localScale;
            _initialScales[obj] = scale;
        }

        return scale;
    }

    public void AutoWireElements()
    {
        if (shopPanel == null)
        {
            GameObject foundPanel = GameObject.Find("ShopPanel");
            if (foundPanel == null) foundPanel = GameObject.Find("Shop Panel");

            if (foundPanel == null)
            {
                Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (Transform t in allTransforms)
                {
                    if (t != null && (t.gameObject.name == "ShopPanel" || t.gameObject.name == "Shop Panel") && t.GetComponent<RectTransform>() != null)
                    {
                        if (t.gameObject.scene.isLoaded)
                        {
                            shopPanel = t.gameObject;
                            break;
                        }
                    }
                }
            }
            else
            {
                shopPanel = foundPanel;
            }
        }

        if (openShopButton == null)
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Button b in allButtons)
            {
                if (b == null) continue;
                string bName = b.gameObject.name.ToLower();
                if (bName == "shop" || bName == "shop btn" || bName == "shop button")
                {
                    if (shopPanel == null || !b.transform.IsChildOf(shopPanel.transform))
                    {
                        openShopButton = b;
                        break;
                    }
                }
            }
        }

        if (openShopButton != null)
        {
            openShopButton.onClick.RemoveListener(OpenShop);
            openShopButton.onClick.AddListener(OpenShop);
        }

        if (shopPanel != null && closeShopButton == null)
        {
            Button[] panelButtons = shopPanel.GetComponentsInChildren<Button>(true);
            foreach (Button b in panelButtons)
            {
                if (b == null) continue;
                string bName = b.gameObject.name.ToLower();
                if (bName.Contains("close") || bName.Contains("exit") || bName.Contains("cancel"))
                {
                    closeShopButton = b;
                    break;
                }
            }
        }

        if (closeShopButton != null)
        {
            closeShopButton.onClick.RemoveListener(CloseShop);
            closeShopButton.onClick.AddListener(CloseShop);
        }

        bool needsRebuild = (animalItems == null || animalItems.Count == 0);
        if (!needsRebuild)
        {
            foreach (ShopAnimalItem item in animalItems)
            {
                if (item == null || item.buyButton == null || item.ownedIndicator == null)
                {
                    needsRebuild = true;
                    break;
                }
            }
        }

        if (needsRebuild && shopPanel != null)
        {
            animalItems = new List<ShopAnimalItem>();
            string[] animalNames = { AnimalShopManager.ANIMAL_FOX, AnimalShopManager.ANIMAL_ELEPHANT, AnimalShopManager.ANIMAL_TIGER, AnimalShopManager.ANIMAL_LION };

            foreach (string animalName in animalNames)
            {
                ShopAnimalItem item = FindAnimalItemInPanel(animalName);
                if (item != null)
                {
                    animalItems.Add(item);
                }
            }
        }

        if (animalItems != null)
        {
            foreach (ShopAnimalItem item in animalItems)
            {
                if (item == null) continue;

                if (item.ownedIndicator != null)
                {
                    GetInitialScale(item.ownedIndicator);
                }

                if (item.buyButton != null)
                {
                    string targetAnimal = item.animalName;
                    item.buyButton.onClick.RemoveAllListeners();
                    item.buyButton.onClick.AddListener(() => OnBuyClicked(targetAnimal));
                }
            }
        }
    }

    private ShopAnimalItem FindAnimalItemInPanel(string animalName)
    {
        if (shopPanel == null) return null;

        Transform[] allChildren = shopPanel.GetComponentsInChildren<Transform>(true);
        Transform animalTransform = null;

        foreach (Transform t in allChildren)
        {
            if (t != null && t.gameObject.name.Equals(animalName, StringComparison.OrdinalIgnoreCase))
            {
                animalTransform = t;
                break;
            }
        }

        if (animalTransform == null) return null;

        ShopAnimalItem item = new ShopAnimalItem();
        item.animalName = animalName;
        item.ownedIndicator = animalTransform.gameObject;

        Button btn = animalTransform.GetComponent<Button>();
        if (btn == null) btn = animalTransform.GetComponentInChildren<Button>(true);
        item.buyButton = btn;

        TMP_Text priceTxt = null;
        if (btn != null)
        {
            priceTxt = btn.GetComponentInChildren<TMP_Text>(true);
        }
        if (priceTxt == null)
        {
            priceTxt = animalTransform.GetComponentInChildren<TMP_Text>(true);
        }
        item.priceText = priceTxt;

        return item;
    }

    public void OpenShop()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        RefreshShopUI();
    }

    public void CloseShop()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    public void OnBuyClicked(string animalName)
    {
        if (string.IsNullOrEmpty(animalName))
            return;

        if (AnimalShopManager.IsAnimalUnlocked(animalName))
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayButtonClick();

            return;
        }

        bool success = AnimalShopManager.BuyAnimal(animalName);
        if (success)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrectAnswer();

            RefreshShopUI();

            ShopAnimalItem purchasedItem = animalItems.Find(i => i != null && i.animalName.Equals(animalName, StringComparison.OrdinalIgnoreCase));
            if (purchasedItem != null && purchasedItem.ownedIndicator != null)
            {
                Vector3 baseScale = GetInitialScale(purchasedItem.ownedIndicator);
                StartCoroutine(AnimateOwnedScale(purchasedItem.ownedIndicator.transform, baseScale, baseScale * ownedScaleMultiplier));
            }
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrongAnswer();
        }
    }

    private IEnumerator AnimateOwnedScale(Transform target, Vector3 fromScale, Vector3 toScale)
    {
        if (target == null) yield break;

        float duration = 0.35f;
        float elapsed = 0f;
        Vector3 overshoot = toScale * 1.1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t < 0.6f)
            {
                float subT = t / 0.6f;
                target.localScale = Vector3.Lerp(fromScale, overshoot, Mathf.SmoothStep(0f, 1f, subT));
            }
            else
            {
                float subT = (t - 0.6f) / 0.4f;
                target.localScale = Vector3.Lerp(overshoot, toScale, Mathf.SmoothStep(0f, 1f, subT));
            }

            yield return null;
        }

        target.localScale = toScale;
    }

    public void RefreshShopUI()
    {
        if (shopPanel == null || animalItems == null) return;

        string latestAnimal = AnimalShopManager.GetLatestUnlockedAnimal();

        foreach (ShopAnimalItem item in animalItems)
        {
            if (item == null || string.IsNullOrEmpty(item.animalName)) continue;

            bool isUnlocked = AnimalShopManager.IsAnimalUnlocked(item.animalName);
            bool isLatest = item.animalName.Equals(latestAnimal, StringComparison.OrdinalIgnoreCase);
            int price = AnimalShopManager.GetAnimalPrice(item.animalName);

            if (item.buyButton != null)
            {
                item.buyButton.gameObject.SetActive(!isUnlocked);
            }

            if (item.priceText != null && !isUnlocked)
            {
                item.priceText.text = price.ToString();
            }

            if (item.ownedIndicator != null)
            {
                item.ownedIndicator.SetActive(true);

                Vector3 baseScale = GetInitialScale(item.ownedIndicator);
                Vector3 targetScale = (isUnlocked && isLatest) ? (baseScale * ownedScaleMultiplier) : baseScale;
                item.ownedIndicator.transform.localScale = targetScale;
            }
        }
    }
}
