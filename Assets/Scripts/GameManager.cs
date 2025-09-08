using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class SaveData
{
    public Difficulty difficulty;
    public List<int> matchedCardIDs;
    public float timer;
    public int score;
    public int moves;
}

public enum Difficulty { Easy, Medium, Hard }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Button NextBtn, HomeBtn, ExitBtn;


    [Header("Game Object")]
    public GameObject CardPanel;
    public GameObject LevelSelector;
    public GameObject MessageBox;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public Transform cardParent;
    public Sprite[] cardFaces;
    public Sprite cardBack;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI movesText;
    public GridLayoutGroup gridLayoutGroup;

    [Header("Layout Settings")]
    public int columns = 4;
    public int rows = 3;

    [Header("Sound Effects")]
    public AudioClip flipClip;
    public AudioClip matchClip;
    public AudioClip mismatchClip;
    public AudioClip gameOverClip;

    private List<Card> cards = new List<Card>();
    private Card firstCard, secondCard;
    private bool canInteract = false;
    private float timer = 0f;
    private int score = 0;
    private int moves = 0;
    private int matchedPairs = 0;
    private int totalPairs = 0;
    private Difficulty currentDifficulty;

    public bool CanInteract => canInteract;

    void Awake()
    {
        Instance = this;
    }

    void StartGame()
    {
        if (!LoadGame())
            SetupGame(currentDifficulty);
    }

    void Update()
    {
        if (canInteract)
        {
            timer += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void SetupGame(Difficulty diff)
    {
        currentDifficulty = diff;

        ClearBoard();
        ResetGameState();
        UpdateUI();

        List<int> ids = GenerateShuffledIds(totalPairs);
        int totalCards = ids.Count;
        int columns, rows;
        CalculateGrid(totalCards, out columns, out rows);
        ApplyGridSettings(columns, rows);

        foreach (int id in ids)
        {
            var obj = Instantiate(cardPrefab, cardParent);
            var card = obj.GetComponent<Card>();

            card.SetupCard(id, cardFaces[id], cardBack);
            cards.Add(card);
        }
        AdjustCardSize(columns, rows);
    }
    private void CalculateGrid(int totalCards, out int columns, out int rows)
    {
        //columns = Mathf.CeilToInt(Mathf.Sqrt(totalCards));
        //rows = Mathf.CeilToInt((float)totalCards / columns);
        int size = Mathf.CeilToInt(Mathf.Sqrt(totalCards));
        columns = size;
        rows = size;

        if (columns * rows < totalCards)
        {
            rows += 1; // Add an extra row if needed
        }
    }

    private void ApplyGridSettings(int columns, int rows)
    {
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = columns;
    }

    private void AdjustCardSize(int columns, int rows)
    {
        RectTransform parentRect = cardParent.GetComponent<RectTransform>();

        float spacingX = gridLayoutGroup.spacing.x;
        float spacingY = gridLayoutGroup.spacing.y;
        float paddingX = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
        float paddingY = gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;

        float availableWidth = parentRect.rect.width - paddingX - spacingX * (columns - 1);
        float availableHeight = parentRect.rect.height - paddingY - spacingY * (rows - 1);

        float cellWidth = availableWidth / columns;
        float cellHeight = availableHeight / rows;

        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);

        StartCoroutine("cardDisplay");

    }

    IEnumerator cardDisplay()
    {
        foreach (Card card in cards)
        {
            card.FlipCard();
            //c.FlipCard();
        }
        yield return new WaitForSeconds(2);
        foreach (Card card in cards)
        {
            card.FlipCard();
            //c.FlipCard();
        }
    }
    public void ApplyDifficultySettings(string diff)
    {
        switch (diff)
        {
            case "Easy":
                totalPairs = 3;
                break;
            case "Medium":
                totalPairs = 6;
                break;
            case "Hard":
                totalPairs = 8;
                break;
            default:
                totalPairs = 6;
                break;
        }
        CardPanel.SetActive(true);
        LevelSelector.SetActive(false);
        StartGame();

    }

    private void ResetGameState()
    {
        matchedPairs = 0;
        score = 0;
        moves = 0;
        timer = 0f;
        canInteract = true;
    }

    private List<int> GenerateShuffledIds(int pairCount)
    {
        List<int> ids = new List<int>();
        for (int i = 0; i < pairCount; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }
        Shuffle(ids);
        return ids;
    }

    public void OnCardFlipped(Card card)
    {

        if (firstCard == null)
            firstCard = card;
        else if (secondCard == null && card != firstCard)
        {
            secondCard = card;
            moves++;
            UpdateMovesUI();
            StartCoroutine(CheckMatchRoutine());
        }
    }

    IEnumerator CheckMatchRoutine()
    {
        canInteract = false;
        yield return new WaitForSeconds(0.5f);

        if (firstCard.cardID == secondCard.cardID)
        {
            firstCard.SetMatched();
            secondCard.SetMatched();
            matchedPairs++;
            PlaySound(matchClip);
            score += 10;
            Destroy(firstCard.gameObject);
            Destroy(secondCard.gameObject);
        }
        else
        {
            firstCard.FlipCard();
            secondCard.FlipCard();
            PlaySound(mismatchClip);
        }

        UpdateScoreUI();
        firstCard = secondCard = null;
        canInteract = true;

        if (matchedPairs == totalPairs)
            HandleGameOver();
    }

    private void HandleGameOver()
    {
        canInteract = false;
        PlaySound(gameOverClip);

        score += Mathf.Max(0, 300 - (int)timer);
        UpdateScoreUI();

        SaveGame();
        Debug.Log($"Game Over! Final Score: {score}");
        MessageShow($"Game Over! Final Score: {score}");
        NextBtn.gameObject.SetActive(true);

    }

    //Next level Start
    public void NextLevel()
    {
        NextBtn.gameObject.SetActive(false);
        //totalPairs += 4;
        int totalCards = totalPairs * 2;

        // Go to the next perfect square greater than current
        int nextSize = Mathf.CeilToInt(Mathf.Sqrt(totalCards)) + 1;
        int nextCardCount = nextSize * nextSize;

        totalPairs = nextCardCount / 2; // each pair is 2 cards

        SetupGame(currentDifficulty);
    }


    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    void ClearBoard()
    {
        foreach (Transform c in cardParent)
            Destroy(c.gameObject);
        cards.Clear();
        firstCard = secondCard = null;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }

    void SaveGame()
    {
        SaveData data = new SaveData
        {
            difficulty = currentDifficulty,
            matchedCardIDs = GetMatchedIDs(),
            timer = timer,
            score = score,
            moves = moves
        };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "savegame.json"), json);
    }

    bool LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, "savegame.json");
        if (!File.Exists(path)) return false;

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        SetupGame(data.difficulty);
        timer = data.timer;
        score = data.score;
        moves = data.moves;
        UpdateUI();
        return true;
    }

    List<int> GetMatchedIDs()
    {
        List<int> ids = new List<int>();
        foreach (var c in cards)
            if (c.IsMatched)
                ids.Add(c.cardID);
        return ids;
    }

    void UpdateTimerUI() => timerText.text = $"{(int)(timer / 60):00}:{(int)(timer % 60):00}";

    void UpdateScoreUI() => scoreText.text = $"{score}";
    void UpdateMovesUI() => movesText.text = $"{moves}";


    void UpdateUI()
    {
        UpdateTimerUI();
        UpdateScoreUI();
        UpdateMovesUI();
    }
    public void Home()
    {
        ClearBoard();
        ResetGameState();
        UpdateUI();
        CardPanel.SetActive(false);
        LevelSelector.SetActive(true);
        StartGame();
    }

    public void MessageShow(string txt)
    {
        StartCoroutine(Show(txt));
    }
    IEnumerator Show(string txt)
    {
        MessageBox.gameObject.SetActive(true);
        MessageBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = txt;
        yield return new WaitForSeconds(3f);
        MessageBox.gameObject.SetActive(false);
    }
    public void ApplictionQuit()
    {
        Application.Quit();
    }
}
