using UnityEngine;

public enum SuspicionState
{
    Normal,
    Suspicious,
    Dangerous
}

[CreateAssetMenu(fileName = "New Person Data", menuName = "Phone System/Person Data")]
public class PersonData : ScriptableObject
{
    public string personName;
    public Sprite personImage;
    public SuspicionState currentState = SuspicionState.Normal;
}