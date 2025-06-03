using MyTools.LoadingManager;
using MyTools.SaveManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButtonHolder : MonoBehaviour
{
    public Button button;
    [SerializeField] Image image;
    [SerializeField] GameObject Lock;
    [SerializeField] private Text LevelText;
    [SerializeField] private int LevelIndex = -1;

    private void Start()
    {
        if (!button) TryGetComponent(out button);
        if(!image) TryGetComponent(out image);
        if (!LevelText) GetComponentInChildren<Text>();
    }

    public void SetupData(ButtonInfo buttonInfo)
    {
        if(buttonInfo.levelNo != null) LevelIndex = (int)buttonInfo.levelNo;
        
        if (!buttonInfo.LevelTitle.Equals("") && LevelText)
            LevelText.text = buttonInfo.LevelTitle + (buttonInfo.levelNo + 1).ToString();
        else
            LevelText.gameObject.SetActive(false);

        if (buttonInfo.sprite && image) image.sprite = buttonInfo.sprite;

        if (Lock) Lock.SetActive(buttonInfo.Locked);
        
        if (button)
        {
            var unlockedLevel = SaveManager.Prefs.GetInt(SharedVariables.UnlockedLevels, 0);
            button.onClick.RemoveAllListeners();
            if (LevelIndex <= unlockedLevel)
                button.onClick.AddListener(() =>
                {
                    SelectLevel(LevelIndex);
                    button.onClick.RemoveAllListeners();
                });
        }
    }

    void SelectLevel(int levelIndex)
    {
        var levelStatus = SaveManager.Prefs.GetInt(SharedVariables.Level_ + levelIndex, 0) == 1;
        if (levelStatus)
            SaveManager.Prefs.SetInt(SharedVariables.RepeatingLevels, 1);
        SaveManager.Prefs.SetInt(SharedVariables.CurrentLevelNo, levelIndex);
        Debug.Log("Selected Index: " + levelIndex);

        LoadingScript.Instance.LoadingAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

public class ButtonInfo
{
    public Sprite sprite;
    public int? levelNo;
    public string LevelTitle;
    public bool Locked = false;

    public ButtonInfo()
    {

    }
}
