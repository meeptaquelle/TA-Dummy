// HUDManager.cs
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [SerializeField] private GameObject hudCanvas;
    [SerializeField] private TMP_Text keyCountText;
    [SerializeField] private GameObject pickupPrompt;
    [SerializeField] private GameObject escapePrompt;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        pickupPrompt.SetActive(false);
        escapePrompt.SetActive(false);
        UpdateKeyCount(0, GameManager.Instance.totalKeys);
    }

    public void UpdateKeyCount(int current, int total)
    {
        keyCountText.text = $"Keys: {current}/{total}";
    }

    public void ShowPickupPrompt(bool show)
    {
        pickupPrompt.SetActive(show);
    }

    public void ShowEscapePrompt()
    {
        StartCoroutine(ShowBriefly(escapePrompt, 4f));
    }

    System.Collections.IEnumerator ShowBriefly(GameObject obj, float duration)
    {
        obj.SetActive(true);
        yield return new WaitForSecondsRealtime(duration); // realtime so it works with timeScale
        obj.SetActive(false);
    }
}