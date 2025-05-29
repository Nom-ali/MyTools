//using System;
//using System.Collections;
//using UnityEngine;

//public class LevelManager : MonoBehaviour
//{
//    [SerializeField] private LevelBase currentLevel;
//    [SerializeField] private LevelBase[] levelsList;

//    internal int totalLevels => levelsList.Length;

//    internal IEnumerator InitLevel(int levelNo)
//    {
//        for (int i = 0; i < levelsList.Length; i++)
//        {
//            if (i == levelNo)
//                currentLevel = levelsList[i];
//            else
//                levelsList[i].gameObject.SetActive(false);
//        }
//        currentLevel.gameObject.SetActive(true);
//        yield return currentLevel.SetupLevel();
//    }

//    public IEnumerator CheckLevelComplete_(Action levelCompleteAction)
//    {
//        if (currentLevel)
//        {
//            bool Check = currentLevel.PlacedObjectsList.TrueForAll(Item => Item.Placed == true);
//            Debug.Log("Level Completed: " + Check);

//            if (Check)
//            {
//                Debug.Log("********** <color=yellow> LEVEL COMPLETE </color>**********");

//                //if(SaveManager.GetLevelModule(3, LevelNo) == true && SaveManager.Prefs.GetBool(SharedVariables.rate, false) == false)
//                //{
//                //    RateUsPanel.SetActive(true);

//                //    yield return new WaitUntil(() => RateUsPanel.activeSelf == false);
//                //}

//                levelCompleteAction?.Invoke();
//                yield return null;
//            }
//        }
//    }
//}
