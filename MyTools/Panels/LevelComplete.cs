using RNA;
using RNA.SaveManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelComplete : AddListeners
{
    [Space(10)]
    [SerializeField] private TextMeshProUGUI CoinsEarnedText;
    [SerializeField] private TextMeshProUGUI CoinsText;
    [SerializeField] private Button Reward2X_Btn;

    private GameManager gameManager => SingletonManager.GetSingleton<GameManager>();

    public override void Init(UIManagerBase manager)
    {
        OnInitComplete.RemoveAllListeners();
        OnInitComplete.AddListener(AddListener);
        base.Init(manager);
    }

    //called from inspector
    public void AddListener()
    {
        CoinsEarnedText.text = gameManager.CoinsEarned.ToString();
        SaveManager.Currency.Value += gameManager.CoinsEarned;
        CoinsText.text = SaveManager.Currency.Value.ToString();

        Reward2X_Btn.onClick.RemoveAllListeners();
        Reward2X_Btn.onClick.AddListener(() =>
        {
            Debug.Log("Rewarded ADs Reward");
            int doubleCoins = gameManager.CoinsEarned * 2;
            CoinsEarnedText.text = doubleCoins.ToString();
            SaveManager.Currency.Value += gameManager.CoinsEarned;
            CoinsText.text = SaveManager.Currency.Value.ToString();
        });
    }

}
