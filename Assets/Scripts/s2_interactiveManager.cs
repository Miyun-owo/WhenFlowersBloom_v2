using System.Collections.Generic;
using UnityEngine;

public class s2_interactiveManager : MonoBehaviour
{
    private class CardRow
    {
        public string cardName;
        public int cardID;
        public string angle0;
        public string angle90;
        public string angle180;
        public string angle270;
        public string mark;
    }

    private readonly Dictionary<int, CardRow> table = new Dictionary<int, CardRow>();

    public int CurrentCardID { get; private set; } = -1;
    public string CurrentCardName { get; private set; } = "Unknown";
    public string CurrentWord { get; private set; } = "Unknown";
    public bool IsCurrentMark { get; private set; }
    public bool IsKnownCard { get; private set; }

    void Awake()
    {
        BuildTable();
    }

    private void BuildTable()
    {
        table.Clear();
        AddRow("06_get-married", 6, "happiness", "duty", "restraint", "safeguard", "duty");
        AddRow("07_learning", 7, "growth", "prepare_for_wedding", "useless", "dream", "prepare_for_wedding");
        AddRow("05_employment", 5, "self-actualization", "transition", "burden", "abnormal", "burden");
        AddRow("08_travel", 8, "freedom", "take_a_break", "dangerous", "memories", "take_a_break");
    }

    private void AddRow(string cardName, int cardID, string angle0, string angle90, string angle180, string angle270, string mark)
    {
        table[cardID] = new CardRow
        {
            cardName = cardName,
            cardID = cardID,
            angle0 = angle0,
            angle90 = angle90,
            angle180 = angle180,
            angle270 = angle270,
            mark = mark
        };
    }

    public string Evaluate(int cardID, float discreteAngle)
    {
        CurrentCardID = cardID;

        if (!table.TryGetValue(cardID, out CardRow row))
        {
            CurrentCardName = "Unknown";
            CurrentWord = "Unknown";
            IsCurrentMark = false;
            IsKnownCard = false;
            return CurrentWord;
        }

        CurrentCardName = row.cardName;
        CurrentWord = GetWordByAngle(row, discreteAngle);
        IsCurrentMark = CurrentWord == row.mark;
        IsKnownCard = true;

        return CurrentWord;
    }

    public bool EvaluateIsMark(int cardID, float discreteAngle)
    {
        Evaluate(cardID, discreteAngle);
        return IsCurrentMark;
    }

    public bool GetCurrentIsMark()
    {
        return IsCurrentMark;
    }

    private string GetWordByAngle(CardRow row, float discreteAngle)
    {
        int angle = NormalizeDiscreteAngle(discreteAngle);

        if (angle == 0) return row.angle0;
        if (angle == 90) return row.angle90;
        if (angle == 180) return row.angle180;
        if (angle == 270) return row.angle270;

        return "Unknown";
    }

    private int NormalizeDiscreteAngle(float discreteAngle)
    {
        int angle = Mathf.RoundToInt(discreteAngle / 90f) * 90;
        angle %= 360;

        if (angle < 0)
        {
            angle += 360;
        }

        return angle;
    }
}
