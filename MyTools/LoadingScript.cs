using MyBox;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyTools.LoadingManager
{
    public class LoadingScript : MonoBehaviour
    {
        public static LoadingScript Instance;

         private AnimationBase LoadingPanel;

        [SerializeField] private FillBarType FillBarType = FillBarType.None;
        [ConditionalField(nameof(FillBarType), false, FillBarType.FillImage)]
        [SerializeField] private Image FillBar;
        [ConditionalField(nameof(FillBarType), false, FillBarType.Slider)]
        [SerializeField] private Slider Slider;

        [Range(0f, 9f)]
        [SerializeField] private float Duration = 1;

        [Range(0f, 1f)]
        [SerializeField] private float FadeDuration = 1;

        [Space]
        [SerializeField] private GameObject[] DotList;

        void Start()
        {
            StartCoroutine(AnimateDots());
        }

        private void Show()
        {
            if(LoadingPanel == null)
            {
                LoadingPanel = GetComponent<AnimationBase>();
                if (LoadingPanel == null)
                {
                    Debug.LogError("LoadingPanel AnimationBase is not assigned or found.");
                    return;
                }
            }

            LoadingPanel.Show();
        }

        public void LoadScene(int sceneIndex, ButtonActionSimple onComplete = null)
        {
            Show();
            StartCoroutine(Load_Scene(sceneIndex, onComplete));
        }
        public void LoadScene(string sceneName, ButtonActionSimple onComplete = null)
        {
            Show();
            var sceneID = SceneUtility.GetBuildIndexByScenePath(sceneName);
            StartCoroutine(Load_Scene(sceneID, onComplete));
        }
        IEnumerator Load_Scene(int sceneIndex, ButtonActionSimple onComplete)
        {
            if (FillBar) FillBar.fillAmount = 0;
            if (Slider) Slider.value = 0f;

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
            UIManager.Instance.OnButtonClicked(onComplete);
            yield return FadeOut(0.1f);
        }

        public void LoadingAsync(int sceneIndex, bool manuallyFade = false, ButtonActionSimple onComplete = null)
        {
            Show();
            StartCoroutine(LoadSceneAsync(sceneIndex, manuallyFade, Duration, onComplete));
        }
        public void LoadingAsync(string sceneName, bool manuallyFade = false, ButtonActionSimple onComplete = null)
        {
            Show();
            var sceneID = SceneUtility.GetBuildIndexByScenePath(sceneName);
            StartCoroutine(LoadSceneAsync(sceneID, manuallyFade, Duration, onComplete));
        }

        IEnumerator LoadSceneAsync(int sceneIndex, bool manuallyFade, float delay, ButtonActionSimple onComplete)
        {
            Debug.Log("Loading Scene: " + sceneIndex);
            AdsManager.Instance?.ShowBigBannerAds();
            AudioPlayer.instance?.StopMusic();

            if (FillBar) FillBar.fillAmount = 0;
            if (Slider) Slider.value = 0f;

            float value = 0;
            float oldDuration = delay;
            while (delay > 0)
            {
                delay -= Time.deltaTime;
                value = (oldDuration - delay) / oldDuration * 0.5f;
                Filler(value);
                yield return null;
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
            operation.allowSceneActivation = false;

            yield return new WaitUntil(() => operation.progress >= value);

            while (!operation.isDone && operation.progress >= value)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                Filler(progress);
                if (operation.progress >= 0.9f)
                {
                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
            if (!manuallyFade)
            {
                AdsManager.Instance?.DestroyBigBanner();
                UIManager.Instance.OnButtonClicked(onComplete);
                yield return FadeOut(FadeDuration);
            }
        }

        internal void FakeLoading(Action onComplete)
        {
            CallFakeLoading(onComplete);
        }

        internal void FakeLoading(ButtonActionSimple onComplete)
        {
            CallFakeLoading(onComplete);
        }
            
        internal void CallFakeLoading(Action onComplete)
        {
            Show();
            StartCoroutine(_FakeLoading(new(), onComplete));
        }
        
        internal void CallFakeLoading(ButtonActionSimple onComplete)
        {
            Show();
            StartCoroutine(_FakeLoading(onComplete, null));
        }

        private IEnumerator _FakeLoading(ButtonActionSimple onComplete, Action action)
        {
            if (FillBar) FillBar.fillAmount = 0;
            if (Slider) Slider.value = 0f;

            float TimeElapse = 0;
            while (TimeElapse < Duration)
            {
                TimeElapse += Time.unscaledDeltaTime;
                float value = TimeElapse / Duration;
                Filler(value);
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);

            // OnComplete Ation
            action?.Invoke();
            UIManager.Instance.OnButtonClicked(onComplete);
            yield return FadeOut();
        }

        void Filler(float value)
        {
            switch (FillBarType)
            {
                case FillBarType.FillImage:
                    if (FillBar) FillBar.fillAmount = value;
                    break;

                case FillBarType.Slider:
                    if (Slider) Slider.value = value;
                    break;

                default:
                    break;
            }
        }

        public void FadeOutLoadingScreen(float duration = 1, ButtonActionSimple onComplete = null)
        {
            AdsManager.Instance?.DestroyBigBanner();
            StartCoroutine(FadeOut(duration, onComplete));
        }

        IEnumerator FadeIn(float duration = 1)
        {
            yield return new WaitForSeconds(duration);
        }

        IEnumerator FadeOut(float duration = 1, ButtonActionSimple onComplete = null)
        {
            LoadingPanel.Hide();
            UIManager.Instance.OnButtonClicked(onComplete);
            yield return new WaitForSeconds(duration);
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
    }
}

[Serializable]
public enum FillBarType
{
    None, FillImage, Slider
}