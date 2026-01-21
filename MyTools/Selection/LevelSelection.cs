//using System.Collections.Generic;
//using MyTools.SaveManager;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using DG.Tweening;

//public class LevelSelectionManager : MonoBehaviour
//{

//    public RectTransform BtnsContainer;
//    public Button ChallangeBtn;

//    [Header("UI References")]
//    public Transform buttonContainer;
//    public LevelBtn levelButtonPrefab;
//    public List<Sprite> levelIcons;

//    [Header("Level Settings")]
//    public int totalLevels = 20;

//    void Start()
//    {
//        SaveManager.Prefs.SetBool(SharedVariables.Level_ + 0, true);

//        if(SaveManager.Prefs.HasKey(SharedVariables.RepeatingLevels))
//            SaveManager.Prefs.DeleteKey(SharedVariables.RepeatingLevels, true);

//        ChallangeBtn.onClick.RemoveAllListeners();
//        ChallangeBtn.onClick.AddListener(() =>
//        {
//            StartCoroutine(GenerateLevelButtons());
//            StartCoroutine(AnimateALittle());
//        });
//    }

//    IEnumerator AnimateALittle()
//    {
//        var container = buttonContainer as RectTransform;
//        yield return BtnsContainer.DOAnchorPos(BtnsContainer.anchoredPosition + new Vector2(0, 300), 0.3f).WaitForCompletion();
//        container.DOAnchorPos(Vector2.zero, 1f);
//    }

//    IEnumerator GenerateLevelButtons()
//    {
//        //levelButtons = new List<LevelBtn>();

//        yield return new WaitForSeconds(0.3f);
//        for (int i = 0; i < totalLevels; i++)
//        {
//            LevelBtn buttonObj = Instantiate(levelButtonPrefab, buttonContainer);
//            LevelBtn levelButton = buttonObj;

//            //Sprite sprite = levelIcons != null && levelIcons[i] ? levelIcons[i] : null;

//            levelButton.Setup(i, null);
//        }
//    }

//}
