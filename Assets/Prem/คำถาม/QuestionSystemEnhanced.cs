using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CompleteTMPQuestionSystem : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] answers;
        public int correctAnswerIndex;
        public int[] partialCreditAnswers; // คำตอบที่ได้คะแนนบางส่วน
    }

    [System.Serializable]
    public class AnswerEffects
    {
        public bool useTextFeedback = true;
        [TextArea(3, 5)] public string feedbackText;
        public Color textColor = Color.white;

        public bool useParticleEffect = false;
        public GameObject particleEffect;

        public bool useScreenEffect = false;
        public Color effectColor;
        public float effectDuration = 0.5f;

        public bool useAnimation = false;
        public Animator targetAnimator;
        public string animationTrigger;

        public bool playSound = false;
        public AudioClip soundEffect;
    }

    [Header("Question Setup")]
    public Question[] questions;
    private int currentQuestionIndex = 0;

    [Header("TMP References")]
    public TMP_Text questionText;
    public TMP_Text feedbackText;
    public Button[] answerButtons;
    public TMP_Text[] answerTexts;
    public Image flashPanel;

    [Header("Effects Settings")]
    public AnswerEffects correctEffects;
    public AnswerEffects partialEffects;
    public AnswerEffects wrongEffects;

    [Header("Game Settings")]
    public int scoreForCorrectAnswer = 10;
    public int scoreForPartialAnswer = 5;
    public float feedbackDisplayTime = 2f;
    public string nextSceneName;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (flashPanel != null)
        {
            flashPanel.color = new Color(0, 0, 0, 0);
        }
    }

    private void Start()
    {
        if (questions.Length == 0)
        {
            Debug.LogError("No questions assigned!");
            return;
        }

        InitializeAnswerTexts();
        ShowQuestion(currentQuestionIndex);
    }

    private void InitializeAnswerTexts()
    {
        answerTexts = new TMP_Text[answerButtons.Length];
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerTexts[i] = answerButtons[i].GetComponentInChildren<TMP_Text>();
        }
    }

    private void ShowQuestion(int index)
    {
        // Reset UI and effects
        feedbackText.gameObject.SetActive(false);
        if (flashPanel != null) flashPanel.color = new Color(0, 0, 0, 0);

        // Set up current question
        Question currentQuestion = questions[index];
        questionText.text = currentQuestion.questionText;

        // Set up answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < currentQuestion.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerTexts[i].text = currentQuestion.answers[i];

                answerButtons[i].onClick.RemoveAllListeners();
                int answerIndex = i;
                answerButtons[i].onClick.AddListener(() => CheckAnswer(answerIndex));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void CheckAnswer(int answerIndex)
    {
        Question currentQuestion = questions[currentQuestionIndex];

        // Disable all buttons
        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }

        if (answerIndex == currentQuestion.correctAnswerIndex)
        {
            HandleCorrectAnswer();
        }
        else if (System.Array.Exists(currentQuestion.partialCreditAnswers, x => x == answerIndex))
        {
            HandlePartialAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }

        Invoke("NextStep", feedbackDisplayTime);
    }

    private void HandleCorrectAnswer()
    {
        GameManager.Instance.AddScore(scoreForCorrectAnswer);
        ApplyEffects(correctEffects);
    }

    private void HandlePartialAnswer()
    {
        GameManager.Instance.AddScore(scoreForPartialAnswer);
        ApplyEffects(partialEffects);
    }

    private void HandleWrongAnswer()
    {
        // ใช้เฉพาะข้อความที่กำหนดใน Inspector โดยไม่แก้ไขข้อความอัตโนมัติ
        ApplyEffects(wrongEffects);
    }

    private void ApplyEffects(AnswerEffects effects)
    {
        // Text feedback
        if (effects.useTextFeedback)
        {
            feedbackText.text = effects.feedbackText; // แสดงข้อความตามที่กำหนดไว้
            feedbackText.color = effects.textColor;
            feedbackText.gameObject.SetActive(true);
        }

        // Particle effect
        if (effects.useParticleEffect && effects.particleEffect != null)
        {
            Instantiate(effects.particleEffect, transform.position, Quaternion.identity);
        }

        // Screen effect
        if (effects.useScreenEffect && flashPanel != null)
        {
            StartCoroutine(FlashScreen(effects.effectColor, effects.effectDuration));
        }

        // Animation
        if (effects.useAnimation && effects.targetAnimator != null)
        {
            effects.targetAnimator.SetTrigger(effects.animationTrigger);
        }

        // Sound effect
        if (effects.playSound && effects.soundEffect != null)
        {
            audioSource.PlayOneShot(effects.soundEffect);
        }
    }

    private IEnumerator FlashScreen(Color color, float duration)
    {
        flashPanel.color = color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(color.a, 0, elapsed / duration);
            flashPanel.color = new Color(color.r, color.g, color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        flashPanel.color = new Color(0, 0, 0, 0);
    }

    private void NextStep()
    {
        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Length)
        {
            ShowQuestion(currentQuestionIndex);

            // Re-enable buttons
            foreach (Button button in answerButtons)
            {
                button.interactable = true;
            }
        }
        else
        {
            // Load next scene
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}