using UnityEngine;
using System.Collections;

public class ScaleAnimationControl : MonoBehaviour
{
    [Header("Scale Settings")]
    public float targetScale = 0.5f; // ขนาดเป้าหมายเมื่อย่อ
    public float scaleDuration = 1f; // ระยะเวลาในการย่อ

    [Header("Audio Settings")]
    public AudioClip shrinkSound; // เสียงเมื่อย่อขนาด
    private AudioSource audioSource;

    private Animator animator;

    private void Start()
    {
        // ดึง Animator Component
        animator = GetComponent<Animator>();

        // ดึงหรือเพิ่ม AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // เริ่มการย่อขนาดเมื่อ Scene เริ่ม
        StartCoroutine(ScaleTo(targetScale, scaleDuration));

        // ตั้งค่า Animator ถ้ามี
        if (animator != null)
        {
            animator.SetBool("isShrinking", true);
        }
    }

    private IEnumerator ScaleTo(float target, float duration)
    {
        // เล่นเสียงเมื่อเริ่มย่อ
        if (shrinkSound != null)
        {
            audioSource.PlayOneShot(shrinkSound);
        }

        float startScale = transform.localScale.x;
        float timer = 0f;

        // ปรับขนาดแบบราบรื่น
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newScale = Mathf.Lerp(startScale, target, timer / duration);
            transform.localScale = new Vector3(newScale, newScale, 1f);
            yield return null;
        }

        // คงขนาดเป้าหมายไว้
        transform.localScale = new Vector3(target, target, 1f);
    }
}