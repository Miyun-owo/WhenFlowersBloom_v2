using System.Collections.Generic;
using UnityEngine;

public class s3_CardManager : MonoBehaviour
{
    public static s3_CardManager Instance;

    [Header("Database")]
    public CardData database;

    [Header("Active Cards")]
    private Dictionary<string, NoteCardData> activeCards =
        new Dictionary<string, NoteCardData>();
    private List<string> activeOrder = new List<string>();

    void Awake()
    {
        Instance = this;

        if (database != null)
        {
            database.Init();
        }
        else
        {
            Debug.LogError("s3_CardManager needs a CardData database assigned in the Inspector.");
        }
    }

    // Vuforia Enter
    public void OnCardEnter(string cardName)
    {
        if (database == null)
        {
            Debug.LogError("Cannot add card because s3_CardManager database is not assigned.");
            return;
        }

        var card = database.Get(cardName);
        if (card == null) return;

        if (!activeCards.ContainsKey(cardName))
        {
            activeOrder.Add(cardName);
        }

        activeCards[cardName] = card;
        Debug.Log($"ADD: {cardName}");

        UpdateMusic();
    }

    // Vuforia Exit
    public void OnCardExit(string cardName)
    {
        if (activeCards.ContainsKey(cardName))
        {
            activeCards.Remove(cardName);
            activeOrder.Remove(cardName);
            Debug.Log($"REMOVE: {cardName}");
        }

        UpdateMusic();
    }

    void UpdateMusic()
    {
        if (s3_MusicManager.Instance == null)
        {
            Debug.LogError("Cannot update music because there is no s3_MusicManager in the scene.");
            return;
        }

        var list = new List<NoteCardData>();
        foreach (var cardName in activeOrder)
        {
            if (activeCards.TryGetValue(cardName, out var card))
            {
                list.Add(card);
            }
        }

        s3_MusicManager.Instance.PlayCards(list);
        Debug.Log($"Playing {list.Count} active card sound(s).");
    }
}
