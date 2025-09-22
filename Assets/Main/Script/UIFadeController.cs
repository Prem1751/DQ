using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIFadeController : MonoBehaviour
{
    [Header("การตั้งค่า Fade")]
    public Canvas targetCanvas;          // Canvas สำหรับควบคุม
    public float fadeInDuration = 1.0f;  // เวลาในการแสดง (วินาที)
    public float stayDuration = 2.0f;    // เวลาที่แสดงก่อนจางหาย (วินาที)
    public float fadeOutDuration = 1.0f; // เวลาในการหาย (วินาที)
    public bool startOnAwake = true;     // เริ่มทำงานทันทีเมื่อเริ่มเกม

    [Header("การตั้งค่าเพิ่มเติม")]
    public bool disableAfterFadeOut = true; // ปิด Canvas หลังจากหายไป
    public bool ignoreTimeScale = false;    // ไม่ขึ้นกับเวลาเกม

    private CanvasGroup canvasGroup;

    void Start()
    {
        // ตั้งค่า Canvas
        InitializeCanvas();

        // เริ่มทำงานอัตโนมัติ
        if (startOnAwake)
        {
            StartFadeSequence();
        }
    }

    void InitializeCanvas()
    {
        // ถ้าไม่ได้กำหนด Canvas ให้ใช้ของ GameObject นี้
        if (targetCanvas == null)
        {
            targetCanvas = GetComponent<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogError("ไม่พบ Canvas component!");
                return;
            }
        }

        // สร้างหรือหา CanvasGroup
        canvasGroup = targetCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = targetCanvas.gameObject.AddComponent<CanvasGroup>();
        }

        // ตั้งค่าเริ่มต้น - ซ่อน Canvas
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // เปิด Canvas เตรียมพร้อม
        targetCanvas.enabled = true;
    }

    // เริ่มลำดับการแสดงและหาย
    public void StartFadeSequence()
    {
        StopAllCoroutines();
        StartCoroutine(FadeSequence());
    }

    // ลำดับการทำงาน: Fade In → รอ → Fade Out
    IEnumerator FadeSequence()
    {
        if (targetCanvas == null || canvasGroup == null) yield break;

        // เปิด Canvas
        targetCanvas.enabled = true;

        // Fade In - ค่อยๆ ปรากฏ
        yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

        // ตั้งค่าให้สามารถโต้ตอบได้ขณะที่แสดง
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // รอระยะเวลาที่กำหนด
        yield return StartCoroutine(Wait(stayDuration));

        // ตั้งค่าไม่ให้โต้ตอบได้ขณะที่จางหาย
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Fade Out - ค่อยๆ หายไป
        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));

        // ปิด Canvas หลังจากหายไป
        if (disableAfterFadeOut)
        {
            targetCanvas.enabled = false;
        }

        Debug.Log("Fade sequence completed for Canvas: " + targetCanvas.name);
    }

    // Coroutine สำหรับการ Fade
    IEnumerator Fade(float fromAlpha, float toAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

            float currentAlpha = Mathf.Lerp(fromAlpha, toAlpha, elapsedTime / duration);
            canvasGroup.alpha = currentAlpha;

            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }

    // Coroutine สำหรับรอเวลา
    IEnumerator Wait(float duration)
    {
        if (duration <= 0) yield break;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }

    // ฟังก์ชันสำหรับเริ่มใหม่จากภายนอก
    public void RestartFade()
    {
        StartFadeSequence();
    }

    // ฟังก์ชันสำหรับข้ามการแสดง
    public void SkipFade()
    {
        StopAllCoroutines();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (disableAfterFadeOut && targetCanvas != null)
        {
            targetCanvas.enabled = false;
        }
    }

    // ฟังก์ชันสำหรับแสดงทันที
    public void ShowImmediately()
    {
        StopAllCoroutines();
        if (targetCanvas != null && canvasGroup != null)
        {
            targetCanvas.enabled = true;
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    // ฟังก์ชันสำหรับซ่อนทันที
    public void HideImmediately()
    {
        StopAllCoroutines();
        if (targetCanvas != null && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            if (disableAfterFadeOut)
            {
                targetCanvas.enabled = false;
            }
        }
    }
}