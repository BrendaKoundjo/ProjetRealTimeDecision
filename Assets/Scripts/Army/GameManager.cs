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

    /* ========================= UI ========================= */


    /// Canvas affiché au démarrage pour choisir la difficulté
    [Header("UI")]
    public GameObject canvasDifficulty;

    /* ===================== DIFFICULTÉ ===================== */


    /// Difficulté actuellement sélectionnée
    /// Easy par défaut
    [Header("Difficulty")]
    public Difficulty currentDifficulty = Difficulty.Easy;

    /* ==================== ARMÉE ENNEMIE ==================== */

    /// Référence vers l'ArmyManager de l'armée ennemie
    /// Utilisé pour ajouter des drones en fonction de la difficulté
    [Header("Enemy Army")]
    public ArmyManager enemyArmy;

    /// Awake est appelé avant Start
    /// Ici :
    /// - Mise en place du Singleton
    /// - Pause du jeu tant que la difficulté n'est pas choisie
    private void Awake()
    {
        // Empêche plusieurs GameManager dans la scène
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Met le jeu en pause tant que le menu de difficulté est affiché
        Time.timeScale = 0f;
    }


    /// Appelé par le TMP_Dropdown (OnValueChanged)
    /// Met à jour la difficulté en fonction de l'option choisie
    public void SetDifficultyFromDropdown(TMP_Dropdown dropdown)
    {
        // Conversion de l'index du dropdown vers l'enum Difficulty
        currentDifficulty = (Difficulty)dropdown.value;

        Debug.Log($"[GameManager] Difficulty selected: {currentDifficulty}");
    }


    /// Appelé quand le joueur clique sur le bouton "Start"
    public void StartGame()
    {
        Debug.Log("[GameManager] Starting game");

        // Cache le menu de difficulté
        canvasDifficulty.SetActive(false);

        // Relance le temps du jeu
        Time.timeScale = 1f;

        // Applique les effets de la difficulté choisie
        ApplyDifficulty();
    }


    /// ajoute un certain nombre de drones ennemis supplémentaires en fonction de la difficulté choisie
    void ApplyDifficulty()
    {
        int extraDrones = 0;

        // Choix du nombre de drones selon la difficulté
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

        // Demande à l'armée ennemie de spawner les drones supplémentaires
        enemyArmy.SpawnExtraDrones(extraDrones);
    }
}
