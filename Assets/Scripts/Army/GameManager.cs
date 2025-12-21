using UnityEngine;
using TMPro;

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject canvasDifficulty;

    [Header("Difficulty")]
    public Difficulty currentDifficulty = Difficulty.Easy;

    [Header("Enemy Army")]
    public ArmyManager enemyArmy;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 0f;
    }

    public void SetDifficultyFromDropdown(TMP_Dropdown dropdown)
    {
        currentDifficulty = (Difficulty)dropdown.value;
        Debug.Log($"[GameManager] Difficulty selected: {currentDifficulty}");
    }

    public void StartGame()
    {
        Debug.Log("[GameManager] Starting game");

        canvasDifficulty.SetActive(false);
        Time.timeScale = 1f;

        ApplyDifficulty();
    }

    void ApplyDifficulty()
    {
        int extraDrones = 0;

        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                extraDrones = 0;
                break;

            case Difficulty.Medium:
                extraDrones = 2;
                break;

            case Difficulty.Hard:
                extraDrones = 5;
                break;
        }

        enemyArmy.SpawnExtraDrones(extraDrones);
    }

}
