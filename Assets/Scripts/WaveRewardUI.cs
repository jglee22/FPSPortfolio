using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveRewardUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    public WeaponUpgradeItem[] rewardItems;
    public int choiceCount = 3;
    public string panelTitle = "강화 선택";
    public Vector2 cardSize = new Vector2(320f, 240f);
    public float cardSpacing = 36f;

    bool hasChosen;
    GameObject panelRoot;
    GameObject overlayCanvasObject;
    TMP_FontAsset uiFont;
    float previousTimeScale = 1f;

    public IEnumerator ShowChoices()
    {
        List<WeaponUpgradeItem> choices = PickChoices();
        if (choices.Count == 0)
            yield break;

        hasChosen = false;
        previousTimeScale = Time.timeScale;
        IsOpen = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CreatePanel(choices);

        while (!hasChosen)
            yield return null;

        DestroyPanel();
        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsOpen = false;
    }

    List<WeaponUpgradeItem> PickChoices()
    {
        List<WeaponUpgradeItem> pool = new List<WeaponUpgradeItem>();
        if (rewardItems != null)
        {
            for (int i = 0; i < rewardItems.Length; i++)
            {
                if (rewardItems[i] != null)
                    pool.Add(rewardItems[i]);
            }
        }

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            WeaponUpgradeItem temp = pool[i];
            pool[i] = pool[swapIndex];
            pool[swapIndex] = temp;
        }

        int takeCount = Mathf.Min(Mathf.Max(1, choiceCount), pool.Count);
        if (takeCount < pool.Count)
            pool.RemoveRange(takeCount, pool.Count - takeCount);

        return pool;
    }

    void CreatePanel(List<WeaponUpgradeItem> choices)
    {
        Canvas hudCanvas = FindHudCanvas();
        CacheFont(hudCanvas);

        Canvas canvas = CreateOverlayCanvas(hudCanvas);
        if (canvas == null)
        {
            Debug.LogError("WaveRewardUI: 보상 Canvas를 만들지 못했습니다.");
            hasChosen = true;
            return;
        }

        panelRoot = CreateUiObject("WaveRewardPanel", canvas.transform);
        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        StretchFull(panelRect);
        panelRoot.transform.SetAsLastSibling();

        Image overlay = panelRoot.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.62f);
        overlay.raycastTarget = true;

        CreateLabel("Title", panelRoot.transform, panelTitle, 48f, new Vector2(0f, 220f), new Vector2(900f, 70f));

        float totalWidth = choices.Count * cardSize.x + (choices.Count - 1) * cardSpacing;
        float startX = -totalWidth * 0.5f + cardSize.x * 0.5f;

        for (int i = 0; i < choices.Count; i++)
        {
            float x = startX + i * (cardSize.x + cardSpacing);
            CreateChoiceCard(choices[i], new Vector2(x, -20f));
        }
    }

    void CreateChoiceCard(WeaponUpgradeItem reward, Vector2 position)
    {
        Button button = CreateButton("Reward_" + reward.name, panelRoot.transform, position, cardSize);
        string title = string.IsNullOrEmpty(reward.displayName) ? reward.name : reward.displayName;
        string description = reward.GetDescription();
        CreateLabel("Title", button.transform, title, 30f, new Vector2(0f, 42f), new Vector2(cardSize.x - 28f, 48f));
        if (!string.IsNullOrEmpty(description))
            CreateLabel("Value", button.transform, description, 22f, new Vector2(0f, -28f), new Vector2(cardSize.x - 28f, 72f));

        WeaponUpgradeItem selectedReward = reward;
        button.onClick.AddListener(() => ChooseReward(selectedReward));
    }

    void ChooseReward(WeaponUpgradeItem reward)
    {
        if (hasChosen || reward == null)
            return;

        GunController gunController = FindObjectOfType<GunController>();
        reward.ApplyToLoadout(gunController);
        hasChosen = true;
    }

    void DestroyPanel()
    {
        if (overlayCanvasObject != null)
            Destroy(overlayCanvasObject);
        else if (panelRoot != null)
            Destroy(panelRoot);

        overlayCanvasObject = null;
        panelRoot = null;
    }

    Canvas CreateOverlayCanvas(Canvas hudCanvas)
    {
        overlayCanvasObject = new GameObject("WaveRewardCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        overlayCanvasObject.layer = 5;

        Canvas canvas = overlayCanvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        canvas.overrideSorting = true;

        CanvasScaler scaler = overlayCanvasObject.GetComponent<CanvasScaler>();
        CanvasScaler hudScaler = hudCanvas != null ? hudCanvas.GetComponent<CanvasScaler>() : null;
        if (hudScaler != null)
        {
            scaler.uiScaleMode = hudScaler.uiScaleMode;
            scaler.referenceResolution = hudScaler.referenceResolution;
            scaler.screenMatchMode = hudScaler.screenMatchMode;
            scaler.matchWidthOrHeight = hudScaler.matchWidthOrHeight;
        }
        else
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        return canvas;
    }

    Canvas FindHudCanvas()
    {
        EnemyPoolManager poolManager = GetComponent<EnemyPoolManager>();
        if (poolManager != null && poolManager.waveText != null && poolManager.waveText.canvas != null)
            return poolManager.waveText.canvas;

        Canvas[] canvases = FindObjectsOfType<Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null && canvases[i].isActiveAndEnabled)
                return canvases[i];
        }

        return null;
    }

    void CacheFont(Canvas canvas)
    {
        EnemyPoolManager poolManager = GetComponent<EnemyPoolManager>();
        if (poolManager != null && poolManager.waveText != null)
            uiFont = poolManager.waveText.font;

        if (uiFont != null || canvas == null)
            return;

        TextMeshProUGUI label = canvas.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            uiFont = label.font;
    }

    void EnsureGraphicRaycaster(Canvas canvas)
    {
        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();
    }

    TextMeshProUGUI CreateLabel(string name, Transform parent, string text, float fontSize, Vector2 position, Vector2 size)
    {
        GameObject labelObject = CreateUiObject(name, parent);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        if (uiFont != null)
            label.font = uiFont;
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.12f, 0.12f, 0.12f, 0.94f);
        image.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.28f, 0.28f, 0.28f, 1f);
        colors.pressedColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        button.colors = colors;
        return button;
    }

    GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.layer = parent != null ? parent.gameObject.layer : 5;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void OnDestroy()
    {
        if (!IsOpen)
            return;

        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
        IsOpen = false;
    }
}
