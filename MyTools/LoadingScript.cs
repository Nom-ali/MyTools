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
        [SerializeField] private AnimationBase LoadingPanel;

        [SerializeField] private FillBarType FillBarType = FillBarType.None;
        [ConditionalField(nameof(FillBarType), false, FillBarType.FillImage)]
        [SerializeField] private Image FillBar;
        [ConditionalField(nameof(FillBarType), false, FillBarType.Slider)]
        [SerializeField] private Slider Slider;

        [Range(0f, 9f)]
        [SerializeField] private float Duration = 1;

        [Range(0f, 1f)]
        [SerializeField] private float FadeDuration = 1;

        [SerializeField] ButtonActionSimple OnComplete;

        [Space]
        [SerializeField] private GameObject[] DotList;

        void Start()
        {
            StartCoroutine(AnimateDots());
        }

        public void LoadScene(int sceneIndex)
        {
            LoadingPanel.Show();
            StartCoroutine(Load_Scene(sceneIndex));
        }
        public void LoadScene(string sceneName)
        {
            LoadingPanel.Show();
            var sceneID = SceneUtility.GetBuildIndexByScenePath(sceneName);
            StartCoroutine(Load_Scene(sceneID));
        }
        IEnumerator Load_Scene(int sceneIndex)
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
            yield return FadeOut(0.1f);
        }

        public void LoadingAsync(int sceneIndex, bool manuallyFade = false)
        {
            LoadingPanel.Show();
            StartCoroutine(LoadSceneAsync(sceneIndex, manuallyFade, Duration));
        }
        public void LoadingAsync(string sceneName, bool manuallyFade = false)
        {
            LoadingPanel.Show();
            var sceneID = SceneUtility.GetBuildIndexByScenePath(sceneName);
            StartCoroutine(LoadSceneAsync(sceneID, manuallyFade, Duration));
        }

        IEnumerator LoadSceneAsync(int sceneIndex, bool manuallyFade, float delay)
        {
            //AdsManager.Instance?.ShowBigBannerAds();
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
                //AdsManager.Instance?.DestroyBigBanner();
                yield return FadeOut(FadeDuration);
            }
        }

        internal void FakeLoading(UIManagerBase uIManager)
        {
            if (FillBar) FillBar.fillAmount = 0;
            if (Slider) Slider.value = 0f;
            LoadingPanel.Show();
            StartCoroutine(_FakeLoading(uIManager));
        }

        private IEnumerator _FakeLoading(UIManagerBase uIManager)
        {
            float TimeElapse = 0;
            while (TimeElapse < Duration)
            {
                TimeElapse += Time.unscaledDeltaTime;
                float value = TimeElapse / Duration;
                Filler(value);
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
            AddActions(uIManager);
        }

        void AddActions(UIManagerBase uIManager)
        {
            uIManager.OnButtonClicked(OnComplete);
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

        public void FadeOutLoadingScreen(float duration = 1)
        {
            //AdsManager.Instance?.DestroyBigBanner();
            StartCoroutine(FadeOut(duration));
        }

        IEnumerator FadeIn(float duration = 1)
        {
            LoadingPanel.Show();
            yield return new WaitForSeconds(duration);
        }

        IEnumerator FadeOut(float duration = 1)
        {
            LoadingPanel.Hide();
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