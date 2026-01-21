//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using MyTools.SaveManager;
//using UnityEngine.SceneManagement;
//using MyTools;
//using System.Collections;
//using Coffee.UIEffects;

//public class LevelBtn : MonoBehaviour
//{
//    public Button button;
//    public Image IconImage;
//    public Image LockImage;
//    public TextMeshProUGUI levelText;

//    private int? levelNumber;
//    private bool isUnlocked;

//    private void OnEnable()
//    {
//        if(gameObject.activeInHierarchy)
//            StartCoroutine(Unlock());
//    }

//    IEnumerator Unlock()
//    {
//        yield return new WaitUntil(() => levelNumber != null);
//        isUnlocked = SaveManager.Prefs.GetBool(SharedVariables.Level_ + levelNumber, false);

//        if (isUnlocked)
//        {
//            button.interactable = true;
//            if(IconImage) IconImage.color = Color.white;
//            button.onClick.RemoveAllListeners();
//            button.onClick.AddListener(() =>
//            {
//                Debug.Log("Loading SCene");
//                OnDrag();
//            });
//            //GetComponent<UIShiny>().Play();
//            LockImage.gameObject.SetActive(false);
//        }
//        else
//        {
//            button.interactable = false;
//            if (IconImage) IconImage.color = new Color(1f, 1f, 1f, 0.3f); // Dim for locked
//            LockImage.gameObject.SetActive(true);
//        }
//    }

//    public void Setup(int levelNumber, Sprite icon)
//    {
//        StartCoroutine(setup(levelNumber, icon));
//    }

//    private IEnumerator setup(int levelNumber, Sprite icon)
//    {
//        this.levelNumber = levelNumber;

//        levelText.text = "Level " + (levelNumber + 1);

//        if (IconImage && icon != null)
//            IconImage.sprite = icon;

//        yield return Unlock();
//    }

//    private void OnDrag()
//    {
//        Debug.Log("Level " + levelNumber + " selected (unlocked)");
//        if(SaveManager.Prefs.GetBool(SharedVariables.Level_ + levelNumber, false))
//        {
//            SaveManager.Prefs.SetInt(SharedVariables.CurrentLevelNo, (int)levelNumber, true);
//        }
//        else
//        {
//            SaveManager.Prefs.SetInt(SharedVariables.RepeatingLevels, (int)levelNumber, true);
//        }

//        UIManager.Instance.LoadingPanel.LoadingAsync(SharedVariables.Gameplay, LoadSceneMode.Additive, false,
//            new ButtonActionSimple() { OnBtnClick = OnClickAction.ShowPanel, Panel = PanelType.Gameplay });
//    }
//}
