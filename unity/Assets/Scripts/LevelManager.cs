using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public bool isGameOver { get => state == GameState.POST; }
    public bool isPlaying { get => state == GameState.GAME; }
    static bool isFirstLoad = false;
    public enum GameState
    {
        PRE,
        GAME,
        POST
    }
    public static GameState state = GameState.PRE;
    public GameObject startScreen, gameOverScreen;
    public static LevelManager instance;
    private void Start()
    {
        instance = this;
        startScreen.SetActive(true);
        gameOverScreen.SetActive(false);
        HoodController.instance.Open();
    }
    public void StartGame()
    {
        startScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        state = GameState.GAME;
        foreach (SpawnSequencer item in GameObject.FindObjectsOfType<SpawnSequencer>())
        {
            item.StartCoroutine(item.PlaySequences());
        }
        HoodController.instance.StartGame();
        ScoreSystem.instance.NewRound();
        MechController.player.OnGameStart();
    }

    public void EndGame()
    {
        startScreen.SetActive(true);
        gameOverScreen.SetActive(true);
        state = GameState.POST;
        foreach (SpawnSequencer item in GameObject.FindObjectsOfType<SpawnSequencer>())
        {
            item.StopAllCoroutines();
        }
        Enemy.KillAll(false);
        HoodController.instance.Open();
    }
}
