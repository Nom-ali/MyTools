using DG.Tweening;
using MyBox;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RNA.LoadingManager
{
    public class LoadingScript : MonoBehaviour
    {
        public static LoadingScript Instance;

        [SerializeField] private CanvasGroup Canvas;
        
        [SerializeField] private FillBarType FillBarType = FillBarType.None;
        [ConditionalField(nameof(FillBarType), false, FillBarType.FillImage)]
        [SerializeField] private Image FillBar;
        [ConditionalField(nameof(FillBarType), false, FillBarType.Slider)]
        [SerializeField] private Slider Slider;

        [Range(0f, 9f)]
        [SerializeField] private float Duration = 1;   

        [Range(0f, 1f)]
        [SerializeField] private float FadeDuration = 1;
        [SerializeField] private GameObject[] DotList;

        [ReadOnly, SerializeField] private bool m_FadeScreen = false;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            Canvas.alpha = 0.0f;
        }
        // Start is called before the first frame update
        void Start()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            StartCoroutine(AnimateDots());
        }

        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(Load_Scene(sceneIndex));
        }
        public void LoadScene(string sceneName)
        {
            var sceneID = SceneUtility.GetBuildIndexByScenePath(sceneName);
            StartCoroutine(Load_Scene(sceneID));
        }
        IEnumerator Load_Scene(int sceneIndex)
        {
            if(FillBar) FillBar.fillAmount = 0;
            if(Slider) Slider.value = 0f;

            m_FadeScreen = true;
            yield return FadeIn(FadeDuration);
            float TimeElapse = 0;
            while (TimeElapse < Duration)
            {
                TimeElapse += Time.unscaledDeltaTime;
                float value = TimeElapse / Duration;
                Filler(value);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);

            SceneManager.LoadScene(sceneIndex);
            
            yield return new WaitForSeconds(.3f);
            yield return FadeOut(0.1f);
            m_FadeScreen = false;
        }

        public void LoadingAsync(int sceneIndex, bool manuallyFade = false)
        {
            StartCoroutine(LoadSceneAsync(sceneIndex, manuallyFade, Duration));
        } 
        public void LoadingAsync(string sceneName, bool manuallyFade = false)
        {
            var sceneID = SceneUtility.GetBuildIndexByScenePath(sceneName);
            StartCoroutine(LoadSceneAsync(sceneID, manuallyFade, Duration));
        }

        IEnumerator LoadSceneAsync(int sceneIndex, bool manuallyFade, float delay)
        {
            AdManagers.Instance?.ShowBigBannerAds();
            AudioPlayer.instance?.StopMusic();

            if (FillBar) FillBar.fillAmount = 0;
            if (Slider) Slider.value = 0f;
            //m_FadeScreen = manuallyFade;

            yield return FadeIn(FadeDuration);

            float value = 0;
            float oldDuration = delay;
            while (delay > 0)
            {
                delay -= Time.deltaTime;
                value = (oldDuration - delay) / oldDuration * 0.5f;
                //value = (oldDuration - (delay / oldDuration)) / oldDuration * 2;
                //Debug.Log("New Value: " + value);
                Filler(value);
                yield return null;
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
            operation.allowSceneActivation = false;

            yield return new WaitUntil(() => operation.progress >= value);

            while (!operation.isDone && operation.progress >= value)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                //Debug.Log("New Value progress: " + progress);
                Filler(progress);
                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
            if(!manuallyFade)
            {
                AdManagers.Instance?.DestroyBigBanner();
                yield return FadeOut(FadeDuration);
                //m_FadeScreen = false;
            }
        }

        void Filler(float value)
        {
            switch (FillBarType)
            {
                case FillBarType.FillImage:
                   if(FillBar) FillBar.fillAmount = value; 
                    break;

                case FillBarType.Slider:
                    if(Slider) Slider.value = value; 
                    break;

                default:
                    break;
            }
        }

        IEnumerator AnimateDots()
        {
            if (DotList.Length <= 0)
                yield break;

            Array.ForEach(DotList, dot => dot.SetActive(false));

            while (true)
            {
                for (int i = 0; i < DotList.Length; i++)
                {
                    DotList[i].SetActive(true);
                    yield return new WaitForSeconds(0.4f);
                }
                Array.ForEach(DotList, dot => dot.SetActive(false));
                yield return null;
            }
        }

        public void FadeOutLoadingScreen(float duration = 1)
        {
            AdManagers.Instance?.DestroyBigBanner();
            StartCoroutine(FadeOut(duration));
            //m_FadeScreen = false;
        }

        IEnumerator FadeIn(float duration = 1)
        {
            if (Canvas & Canvas.gameObject.activeInHierarchy == false)
                Canvas.gameObject.SetActive(true);
            
            Canvas.alpha = 0;
            Canvas.DOFade(1, duration).SetUpdate(true).OnComplete(() =>
            {
                Canvas.alpha = 1;
            });
            yield return null;

        }  
        
        IEnumerator FadeOut(float duration = 1)
        {
            if (Canvas & Canvas.gameObject.activeInHierarchy == false)
                Canvas.gameObject.SetActive(true);
            
            Canvas.alpha = 1;
            Canvas.DOFade(0, duration).SetUpdate(true).OnComplete(() =>
            {
                Canvas.alpha = 0;
                Canvas.gameObject.SetActive(false);
                AudioPlayer.instance?.PlayBGMusic();
            });
            yield return null;
        }

        //IEnumerator FadeScreen(float duration = 1)
        //{          
        //    if (!m_FadeScreen)
        //        yield break;
            
        //    if (Canvas & Canvas.gameObject.activeInHierarchy == false)
        //        Canvas.gameObject.SetActive(true);
            
        //    float targetValue = Canvas.alpha == 0 ? 1 : 0;

        //    Canvas.DOFade(targetValue, duration).SetUpdate(true).OnComplete(() =>
        //    {
               
        //        if (targetValue == 0)
        //            Canvas.gameObject.SetActive(false);
        //    });
        //    yield return null;
        //}
    }
}

[System.Serializable]
public enum FillBarType
{
    None, FillImage, Slider
}