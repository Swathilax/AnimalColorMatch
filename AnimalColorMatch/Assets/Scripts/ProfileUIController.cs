using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfileUIController : MonoBehaviour
{
    [System.Serializable]
    public class AvatarOption
    {
        public string avatarId;
        public Sprite avatarSprite;
        public Button avatarButton;
    }

    public const string AVATAR_KEY = "PlayerSelectedAvatar";
    public const string DEFAULT_AVATAR = "1";

    [Header("Profile Panel & Header/Buttons")]
    public GameObject profilePanel;
    public Button openProfileButton;
    public Button closeProfileButton;
    public Button saveButton;

    [Header("Avatar Display Images")]
    public Image homeProfileAvatarImage;
    public Image previewAvatarImage;

    [Header("Avatar Selection List")]
    public List<AvatarOption> avatarOptions = new List<AvatarOption>();

    [Header("Visual Feedback")]
    public float selectedScaleMultiplier = 1.15f;

    private string _savedAvatarId = DEFAULT_AVATAR;
    private string _previewAvatarId = DEFAULT_AVATAR;
    private readonly Dictionary<Button, Vector3> _initialButtonScales = new Dictionary<Button, Vector3>();
    private Vector3 _cachedUniformBaseScale = Vector3.zero;
    private readonly Dictionary<string, Sprite> _cachedSprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        CacheInitialButtonScales();
        AutoWireElements();
        LoadSavedAvatar();
        RebindSceneElements();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        CacheInitialButtonScales();
        AutoWireElements();
        LoadSavedAvatar();
        RebindSceneElements();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        CacheInitialButtonScales();
        AutoWireElements();
        LoadSavedAvatar();
        RebindSceneElements();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _initialButtonScales.Clear();
        _cachedUniformBaseScale = Vector3.zero;
        CacheInitialButtonScales();
        AutoWireElements();
        LoadSavedAvatar();
        RebindSceneElements();
    }

    private void CacheInitialButtonScales()
    {
        if (avatarOptions == null) return;

        foreach (AvatarOption opt in avatarOptions)
        {
            if (opt != null && opt.avatarButton != null)
            {
                if (!_initialButtonScales.ContainsKey(opt.avatarButton))
                {
                    Vector3 current = opt.avatarButton.transform.localScale;
                    if (current.x > 0.6f && profilePanel != null && profilePanel.transform.localScale.x > 2f)
                    {
                        current = new Vector3(1f / profilePanel.transform.localScale.x, 1f / profilePanel.transform.localScale.y, 1f);
                    }
                    _initialButtonScales[opt.avatarButton] = current;
                }
            }
        }

        if (_cachedUniformBaseScale == Vector3.zero)
        {
            foreach (var kvp in _initialButtonScales)
            {
                if (kvp.Value != Vector3.zero && (_cachedUniformBaseScale == Vector3.zero || kvp.Value.sqrMagnitude < _cachedUniformBaseScale.sqrMagnitude))
                {
                    _cachedUniformBaseScale = kvp.Value;
                }
            }
        }
    }

    public void LoadSavedAvatar()
    {
        string defaultId = (avatarOptions != null && avatarOptions.Count > 0 && !string.IsNullOrEmpty(avatarOptions[0].avatarId)) 
            ? avatarOptions[0].avatarId 
            : DEFAULT_AVATAR;

        _savedAvatarId = PlayerPrefs.GetString(AVATAR_KEY, defaultId);
        _previewAvatarId = _savedAvatarId;
    }

    public void RebindSceneElements()
    {
        AutoWireElements();
        UpdateHomeProfileDisplay();
        UpdatePreviewDisplay();
        UpdateSelectionScales();
    }

    public void AutoWireElements()
    {
        if (profilePanel == null)
        {
            GameObject found = GameObject.Find("Profile");
            if (found == null) found = GameObject.Find("ProfilePanel");
            if (found == null) found = GameObject.Find("Profile Panel");

            if (found == null)
            {
                Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (Transform t in allTransforms)
                {
                    if (t != null && (t.gameObject.name == "Profile" || t.gameObject.name == "ProfilePanel" || t.gameObject.name == "Profile Panel") && t.GetComponent<RectTransform>() != null)
                    {
                        if (t.gameObject.scene.isLoaded)
                        {
                            profilePanel = t.gameObject;
                            break;
                        }
                    }
                }
            }
            else
            {
                profilePanel = found;
            }
        }

        if (openProfileButton == null)
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Button b in allButtons)
            {
                if (b == null) continue;
                string bName = b.gameObject.name.ToLower();
                if (bName.Contains("profile") && (profilePanel == null || !b.transform.IsChildOf(profilePanel.transform)))
                {
                    openProfileButton = b;
                    break;
                }
            }
        }

        if (openProfileButton != null)
        {
            openProfileButton.onClick.RemoveListener(OpenProfilePanel);
            openProfileButton.onClick.AddListener(OpenProfilePanel);

            if (homeProfileAvatarImage == null)
            {
                Image[] childImages = openProfileButton.GetComponentsInChildren<Image>(true);
                foreach (Image img in childImages)
                {
                    if (img != null && img.gameObject != openProfileButton.gameObject)
                    {
                        homeProfileAvatarImage = img;
                        break;
                    }
                }

                if (homeProfileAvatarImage == null)
                {
                    homeProfileAvatarImage = openProfileButton.GetComponent<Image>();
                }
            }
        }

        if (homeProfileAvatarImage == null)
        {
            GameObject avatarObj = GameObject.Find("ProfileAvatar");
            if (avatarObj != null)
            {
                homeProfileAvatarImage = avatarObj.GetComponent<Image>();
            }
        }

        if (profilePanel != null)
        {
            if (closeProfileButton == null)
            {
                Button[] panelButtons = profilePanel.GetComponentsInChildren<Button>(true);
                foreach (Button b in panelButtons)
                {
                    if (b == null) continue;
                    string bName = b.gameObject.name.ToLower();
                    if (bName.Contains("close") || bName.Contains("exit") || bName.Contains("x") || bName.Contains("cancel"))
                    {
                        closeProfileButton = b;
                        break;
                    }
                }
            }

            if (saveButton == null)
            {
                Button[] panelButtons = profilePanel.GetComponentsInChildren<Button>(true);
                foreach (Button b in panelButtons)
                {
                    if (b == null) continue;
                    string bName = b.gameObject.name.ToLower();
                    if (bName.Contains("save") || bName.Contains("confirm") || bName.Contains("done"))
                    {
                        saveButton = b;
                        break;
                    }
                }
            }

            if (previewAvatarImage == null || previewAvatarImage == homeProfileAvatarImage)
            {
                Transform specialAvatar = profilePanel.transform.Find("Special Avatar");
                if (specialAvatar == null)
                {
                    Transform[] all = profilePanel.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in all)
                    {
                        if (t != null && t.name.Equals("Special Avatar", StringComparison.OrdinalIgnoreCase))
                        {
                            specialAvatar = t;
                            break;
                        }
                    }
                }

                if (specialAvatar != null)
                {
                    previewAvatarImage = specialAvatar.GetComponent<Image>();
                }
            }

            Transform holder = profilePanel.transform.Find("Avatar");
            if (holder == null)
            {
                Transform[] all = profilePanel.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in all)
                {
                    if (t != null && t.name.Equals("Avatar", StringComparison.OrdinalIgnoreCase) && t.GetComponent<Button>() == null)
                    {
                        holder = t;
                        break;
                    }
                }
            }

            if (holder != null)
            {
                Image holderImg = holder.GetComponent<Image>();
                if (holderImg != null && holderImg.sprite != null && !holderImg.sprite.name.Equals("ProfileHolder", StringComparison.OrdinalIgnoreCase))
                {
                    Sprite ph = FindSpriteByName("ProfileHolder");
                    if (ph != null) holderImg.sprite = ph;
                }
            }
        }

        if (closeProfileButton != null)
        {
            closeProfileButton.onClick.RemoveListener(CloseProfilePanel);
            closeProfileButton.onClick.AddListener(CloseProfilePanel);
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(SaveProfileSelection);
            saveButton.onClick.AddListener(SaveProfileSelection);
        }

        bool needsRebuild = (avatarOptions == null || avatarOptions.Count == 0);
        if (!needsRebuild)
        {
            foreach (AvatarOption opt in avatarOptions)
            {
                if (opt == null || opt.avatarButton == null)
                {
                    needsRebuild = true;
                    break;
                }
            }
        }

        if (needsRebuild && profilePanel != null)
        {
            avatarOptions = new List<AvatarOption>();
            string[] defaultAvatars = { "1", "2", "3", "4", "5" };
            string[] animalNames = { "Bear", "Elephant", "Fox", "Lion", "Tiger" };

            Button[] allPanelButtons = profilePanel.GetComponentsInChildren<Button>(true);

            for (int i = 0; i < defaultAvatars.Length; i++)
            {
                string id = defaultAvatars[i];
                string animal = animalNames[i];

                AvatarOption opt = new AvatarOption();
                opt.avatarId = id;
                opt.avatarSprite = GetAvatarSprite(animal);

                foreach (Button btn in allPanelButtons)
                {
                    if (btn == null || btn == closeProfileButton || btn == saveButton) continue;

                    string bName = btn.gameObject.name.ToLower();
                    if (bName.Contains("(" + id + ")") || bName.Contains(animal.ToLower()) || bName.EndsWith(id))
                    {
                        opt.avatarButton = btn;
                        break;
                    }
                }

                if (opt.avatarButton != null)
                {
                    avatarOptions.Add(opt);
                }
            }
        }

        CacheInitialButtonScales();

        if (avatarOptions != null)
        {
            foreach (AvatarOption opt in avatarOptions)
            {
                if (opt == null || opt.avatarButton == null) continue;

                if (opt.avatarSprite == null)
                {
                    opt.avatarSprite = GetAvatarSprite(opt.avatarId);
                }

                if (opt.avatarSprite != null)
                {
                    Image btnImg = opt.avatarButton.GetComponent<Image>();
                    if (btnImg != null && btnImg.sprite == null)
                    {
                        btnImg.sprite = opt.avatarSprite;
                    }
                }

                string targetId = opt.avatarId;
                opt.avatarButton.onClick.RemoveAllListeners();
                opt.avatarButton.onClick.AddListener(() => OnSelectAvatarClicked(targetId));
            }
        }
    }

    public Sprite GetAvatarSprite(string avatarId)
    {
        if (string.IsNullOrEmpty(avatarId)) return null;

        if (avatarOptions != null)
        {
            foreach (AvatarOption opt in avatarOptions)
            {
                if (opt != null && !string.IsNullOrEmpty(opt.avatarId))
                {
                    if (opt.avatarId.Equals(avatarId, StringComparison.OrdinalIgnoreCase) && opt.avatarSprite != null)
                    {
                        return opt.avatarSprite;
                    }
                }
            }

            if (int.TryParse(avatarId, out int num))
            {
                int zeroIdx = num - 1;
                if (zeroIdx >= 0 && zeroIdx < avatarOptions.Count && avatarOptions[zeroIdx] != null && avatarOptions[zeroIdx].avatarSprite != null)
                {
                    return avatarOptions[zeroIdx].avatarSprite;
                }
            }

            foreach (AvatarOption opt in avatarOptions)
            {
                if (opt != null && opt.avatarSprite != null)
                {
                    string sName = opt.avatarSprite.name.ToLower();
                    string cleanTarget = avatarId.Trim().ToLower();
                    if (sName.Contains(cleanTarget) || (opt.avatarButton != null && opt.avatarButton.gameObject.name.ToLower().Contains(cleanTarget)))
                    {
                        return opt.avatarSprite;
                    }
                }
            }
        }

        if (_cachedSprites.TryGetValue(avatarId, out Sprite cached) && cached != null)
        {
            return cached;
        }

        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        string target = avatarId.Trim().ToLower();

        foreach (Sprite s in allSprites)
        {
            if (s == null) continue;
            string sName = s.name.ToLower();

            if ((target == "1" || target == "bear") && (sName == "bear profile" || sName == "special avatar" || sName == "green bear 1"))
            {
                _cachedSprites[avatarId] = s;
                return s;
            }
            if ((target == "2" || target == "elephant") && (sName.Contains("elephant avatar") || sName == "elephant"))
            {
                _cachedSprites[avatarId] = s;
                return s;
            }
            if ((target == "3" || target == "fox") && (sName.Contains("fox avatar") || sName == "fox"))
            {
                _cachedSprites[avatarId] = s;
                return s;
            }
            if ((target == "4" || target == "lion") && (sName.Contains("lion avatar") || sName == "lion"))
            {
                _cachedSprites[avatarId] = s;
                return s;
            }
            if ((target == "5" || target == "tiger") && (sName.Contains("tiger avatar") || sName == "tiger"))
            {
                _cachedSprites[avatarId] = s;
                return s;
            }
        }

        foreach (Sprite s in allSprites)
        {
            if (s != null && s.name.ToLower().Contains(target))
            {
                _cachedSprites[avatarId] = s;
                return s;
            }
        }

        if (avatarOptions != null && avatarOptions.Count > 0 && avatarOptions[0] != null && avatarOptions[0].avatarSprite != null)
        {
            return avatarOptions[0].avatarSprite;
        }

        return null;
    }

    private Sprite FindSpriteByName(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;

        Sprite[] allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (Sprite s in allSprites)
        {
            if (s != null && s.name.Equals(spriteName, StringComparison.OrdinalIgnoreCase))
            {
                return s;
            }
        }
        return null;
    }

    public void OpenProfilePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        _previewAvatarId = _savedAvatarId;

        if (profilePanel != null)
        {
            profilePanel.SetActive(true);
        }

        UpdatePreviewDisplay();
        UpdateSelectionScales();
    }

    public void CloseProfilePanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        _previewAvatarId = _savedAvatarId;
        UpdatePreviewDisplay();
        UpdateSelectionScales();

        if (profilePanel != null)
        {
            profilePanel.SetActive(false);
        }
    }

    public void OnSelectAvatarClicked(string avatarId)
    {
        if (string.IsNullOrEmpty(avatarId)) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        _previewAvatarId = avatarId;

        UpdatePreviewDisplay();
        AnimateSelection(avatarId);
    }

    public void SaveProfileSelection()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCorrectAnswer();

        _savedAvatarId = _previewAvatarId;
        PlayerPrefs.SetString(AVATAR_KEY, _savedAvatarId);
        PlayerPrefs.Save();

        UpdateHomeProfileDisplay();

        if (profilePanel != null)
        {
            profilePanel.SetActive(false);
        }
    }

    public void UpdateHomeProfileDisplay()
    {
        Sprite s = GetAvatarSprite(_savedAvatarId);
        if (s == null) return;

        if (homeProfileAvatarImage != null)
        {
            homeProfileAvatarImage.sprite = s;
        }

        if (openProfileButton != null)
        {
            Image btnImg = openProfileButton.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.sprite = s;
            }

            Image[] childImgs = openProfileButton.GetComponentsInChildren<Image>(true);
            foreach (Image child in childImgs)
            {
                if (child != null)
                {
                    child.sprite = s;
                }
            }
        }
    }

    public void UpdatePreviewDisplay()
    {
        Sprite s = GetAvatarSprite(_previewAvatarId);
        if (s == null) return;

        if (previewAvatarImage != null && previewAvatarImage != homeProfileAvatarImage)
        {
            previewAvatarImage.sprite = s;
        }
    }

    private Vector3 GetBaseScale()
    {
        if (_cachedUniformBaseScale != Vector3.zero)
        {
            return _cachedUniformBaseScale;
        }

        if (avatarOptions != null)
        {
            foreach (var opt in avatarOptions)
            {
                if (opt != null && opt.avatarButton != null && _initialButtonScales.TryGetValue(opt.avatarButton, out Vector3 s))
                {
                    if (s != Vector3.zero && (_cachedUniformBaseScale == Vector3.zero || s.sqrMagnitude < _cachedUniformBaseScale.sqrMagnitude))
                    {
                        _cachedUniformBaseScale = s;
                    }
                }
            }
        }

        if (_cachedUniformBaseScale != Vector3.zero)
        {
            return _cachedUniformBaseScale;
        }

        if (profilePanel != null && profilePanel.transform.localScale.x > 1.5f)
        {
            return new Vector3(1f / profilePanel.transform.localScale.x, 1f / profilePanel.transform.localScale.y, 1f);
        }

        return Vector3.one;
    }

    public void UpdateSelectionScales()
    {
        if (avatarOptions == null) return;

        Vector3 baseScale = GetBaseScale();
        Vector3 selectedScale = baseScale * selectedScaleMultiplier;

        foreach (AvatarOption opt in avatarOptions)
        {
            if (opt == null || opt.avatarButton == null) continue;

            bool isSelected = opt.avatarId.Equals(_previewAvatarId, StringComparison.OrdinalIgnoreCase);
            opt.avatarButton.transform.localScale = isSelected ? selectedScale : baseScale;
        }
    }

    private void AnimateSelection(string selectedId)
    {
        if (avatarOptions == null) return;

        Vector3 baseScale = GetBaseScale();
        Vector3 selectedScale = baseScale * selectedScaleMultiplier;

        foreach (AvatarOption opt in avatarOptions)
        {
            if (opt == null || opt.avatarButton == null) continue;

            bool isSelected = opt.avatarId.Equals(selectedId, StringComparison.OrdinalIgnoreCase);
            Vector3 targetScale = isSelected ? selectedScale : baseScale;

            StartCoroutine(AnimateButtonScale(opt.avatarButton.transform, opt.avatarButton.transform.localScale, targetScale));
        }
    }

    private IEnumerator AnimateButtonScale(Transform target, Vector3 fromScale, Vector3 toScale)
    {
        if (target == null) yield break;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.localScale = Vector3.Lerp(fromScale, toScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        target.localScale = toScale;
    }
}
