using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Hover Effects")]
    public Color hoverColor = new Color(1, 0.8f, 0);
    public float hoverScale = 1.1f;
    public float hoverDuration = 0.2f;

    [Header("Click Effects")]
    public float clickScale = 0.9f;
    public float clickDuration = 0.1f;

    [Header("Scene Change")]
    public string targetSceneName; // Name of the scene to load

    private Vector3 originalScale;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(ScaleTo(originalScale * hoverScale, hoverDuration));
        spriteRenderer.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(ScaleTo(originalScale, hoverDuration));
        spriteRenderer.color = originalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(PlayClickAnimation());
    }

    IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        float elapsedTime = 0;
        Vector3 startingScale = transform.localScale;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(startingScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }

    IEnumerator PlayClickAnimation()
    {
        // Play click animation
        yield return StartCoroutine(ScaleTo(originalScale * clickScale, clickDuration / 2));
        yield return StartCoroutine(ScaleTo(originalScale, clickDuration / 2));

        // Change scene after animation completes
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("Target scene name is not set in the button!");
        }
    }
}