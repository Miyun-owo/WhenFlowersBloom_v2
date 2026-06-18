using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Music/Note Card Database")]
public class CardData : ScriptableObject
{
    public List<NoteCardData> cards;

    private Dictionary<string, NoteCardData> lookup;

    public void Init()
    {
        lookup = new Dictionary<string, NoteCardData>(System.StringComparer.OrdinalIgnoreCase);

        if (cards == null) return;

        foreach (var card in cards)
        {
            if (card == null || string.IsNullOrEmpty(card.cardName))
                continue;

            lookup[card.cardName] = card;
        }
    }

    public NoteCardData Get(string cardName)
    {
        if (lookup == null)
            Init();

        if (lookup.TryGetValue(cardName, out var card))
            return card;

        string normalized = NormalizeCardName(cardName);
        if (!string.IsNullOrEmpty(normalized) && lookup.TryGetValue(normalized, out card))
            return card;

        Debug.LogWarning($"Card not found: {cardName}");
        return null;
    }

    string NormalizeCardName(string cardName)
    {
        if (string.IsNullOrEmpty(cardName))
            return cardName;

        return cardName.ToLowerInvariant().Replace("_", "-");
    }
}
