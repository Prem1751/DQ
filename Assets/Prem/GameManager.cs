using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private int playerScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int value)
    {
        playerScore += value;
        Debug.Log("คะแนนปัจจุบัน: " + playerScore);
    }

    public int GetScore()
    {
        return playerScore;
    }

    public void ResetScore()
    {
        playerScore = 0;
    }
}