using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int CurrentLevelNo = 0;

    public GameStatus gameStatus;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    internal GameStatus.GameState CurrentState()
    {
        return gameStatus.CurrentGameState;
    }

    internal void LevelFail()
    {
        throw new NotImplementedException();
    }

}
