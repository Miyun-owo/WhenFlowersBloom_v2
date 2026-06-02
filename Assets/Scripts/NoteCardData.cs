using UnityEngine;

[CreateAssetMenu(menuName = "Card Info")]
public class NoteCardData : ScriptableObject
{
    public int cardID;
    public string cardName;
    public float frequency;
}
