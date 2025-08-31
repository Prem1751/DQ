using UnityEngine;

[System.Serializable]
public class PersonData
{
    public string personName;
    public string description;
    public Sprite personImage;
    public SuspicionState currentState = SuspicionState.None;
}

public enum SuspicionState
{
    None,       // ไม่มีสัญลักษณ์
    Question,   // เครื่องหมาย ?
    Cross,      // เครื่องหมาย X (ไม่ใช่คนร้าย)
    Check       // เครื่องหมาย ✓ (คนร้าย)
}