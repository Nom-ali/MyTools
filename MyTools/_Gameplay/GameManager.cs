using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using MyTools;
using MyBox;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Foldout("Additional UI", true)]
    [SerializeField] internal Button[] SpawnBtns;




    [Foldout("Game Manager", true)]
    [Separator("********** Gameplay **********")]
    [SerializeField] private GameStatus gameStatus;

    [Separator("********** Pool **********")]
    [SerializeField] private PoolManager poolManager;

    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Start the game and load the level.
    /// </summary>
    /// <returns></returns>
    internal void Start()
    {
        LoadLevel();
    }

    /// <summary>
    /// Get Current state of the game.
    /// </summary>
    /// <returns></returns>
    internal GameStatus.GameState CurrentState()
    {
        return gameStatus.CurrentGameState;
    }

    /// <summary>
    /// Load the level based on the current level number.
    /// </summary>
    /// <returns></returns>
    void LoadLevel()
    {
        SpawnListener();

        if (UIManager.Instance)
            UIManager.Instance.ManualFadeLoading();
    
    }

    void SpawnListener()
    {
        for (int i = 0; i < SpawnBtns.Length; i++)
        {
            Button button = SpawnBtns[i];
            int index = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (poolManager & poolManager.PoolList[index] != null && poolManager.PoolList[index].Unlokced)
                {
                    (string, GameObject) temp = poolManager.PoolList[index].pool.SpawnFromSinglePool(false);
                    if (temp.Item2)
                    {
                        temp.Item2.transform.position = temp.Item2.transform.Reposition(Directioons.Top, new Vector3(0, -2, 0));
                        temp.Item2.SetActive(true);
                        if(temp.Item2.TryGetComponent(out DragControllerNew controller))
                        {
                            controller.StringKey = temp.Item1;
                            controller.BtnID = index;
                        }
                    }
                    else
                    {
                        Debug.LogError($"No object available in the pool for {poolManager.PoolList[index].pool}");
                    }
                }
            });
        }
    }


    //private void OnApplicationQuit()
    //{
    //    CustomAnalytics.LogEvent($"Game QUIT duraing gameplay at Level No {LevelNo}");
    //}
}
