using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestAddedPopup : MonoBehaviour
{
    public TMP_Text messageText;
    public float displayDuration = 2.0f;
    public float fadeDuration = 0.25f;
    public CanvasGroup canvasGroup; // for fade in/out

    private void Reset()
    {
        // attempt to auto-wire common components when first added in inspector
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (messageText == null) messageText = GetComponentInChildren<TMP_Text>();
    }

    public void Initialize(string message)
    {
        if (messageText != null) messageText.text = message;
        StartCoroutine(ShowAndDestroy());
    }

    private IEnumerator ShowAndDestroy()
    {
        // Fade-in
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // Wait (use unscaled time so pause doesn't block)
        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade-out
        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - (t / fadeDuration));
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}