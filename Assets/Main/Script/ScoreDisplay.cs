using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelMessageText;

    [Header("Message Settings")]
    [SerializeField] private float messageDelay = 1f;

    [Header("Level Messages")]
    [SerializeField] private string level1Message = "เริ่มต้นดีแล้ว! ต่อสู้เพื่อคะแนนที่สูงขึ้น!";
    [SerializeField] private string level2Message = "ยอดเยี่ยม! คุณทำได้ดีมาก!";
    [SerializeField] private string level3Message = "น่าทึ่งมาก! คุณเป็นผู้เชี่ยวชาญแท้ๆ!";

    private void Start()
    {
        // ตรวจสอบว่ามี GameManager หรือไม่
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager ไม่พบ!");
            return;
        }

        UpdateScoreDisplay();
    }

    public void UpdateScoreDisplay()
    {
        if (GameManager.Instance == null) return;

        int currentScore = GameManager.Instance.GetScore();

        // แสดงคะแนน
        if (scoreText != null)
        {
            scoreText.text = "คะแนนของน้องคือ " + currentScore;
        }

        // แสดงข้อความตามระดับหลังจากหน่วงเวลา
        if (levelMessageText != null)
        {
            StartCoroutine(ShowLevelMessage(currentScore));
        }
    }

    private System.Collections.IEnumerator ShowLevelMessage(int score)
    {
        // ซ่อนข้อความก่อน
        levelMessageText.text = "";

        // หน่วงเวลา
        yield return new WaitForSeconds(messageDelay);

        // กำหนดข้อความตามระดับคะแนน
        string message = GetLevelMessage(score);
        levelMessageText.text = message;
    }

    private string GetLevelMessage(int score)
    {
        if (score >= 0 && score <= 10)
        {
            return level1Message; // ระดับ 1: 0-10 คะแนน
        }
        else if (score >= 11 && score <= 20)
        {
            return level2Message; // ระดับ 2: 11-20 คะแนน
        }
        else if (score >= 21 && score <= 30)
        {
            return level3Message; // ระดับ 3: 21-30 คะแนน
        }
        else
        {
            return "คะแนนของคุณเกินขีดจำกัด! สุดยอด!"; // เกิน 30 คะแนน
        }
    }

    // เรียกใช้เมื่อต้องการอัพเดทคะแนนจากที่อื่น
    public void OnScoreChanged()
    {
        UpdateScoreDisplay();
    }
}