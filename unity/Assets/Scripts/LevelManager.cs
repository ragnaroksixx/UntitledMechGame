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
    GameObject screen;
    public static LevelManager instance;
    private void Start()
    {
        instance = this;
        if (isFirstLoad)
            screen = startScreen;
        else
            screen = gameOverScreen;
        screen.SetActive(true);
        HoodController.instance.Open();
    }
    public void StartGame()
    {
        screen.SetActive(false);
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
        screen.SetActive(true);
        state = GameState.POST;
        foreach (SpawnSequencer item in GameObject.FindObjectsOfType<SpawnSequencer>())
        {
            item.StopAllCoroutines();
        }
        Enemy.KillAll(false);
        HoodController.instance.Open();
    }
}
