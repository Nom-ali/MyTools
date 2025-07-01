using UnityEngine;
using UnityEngine.UI;
using MyBox;
namespace MyTools.Timer
{
    public class Timer : MonoBehaviour
    {
        public float levelTime = 60f; // Set the time for each level in seconds
        public Text timerText; // Reference to UI text to display timer
        public RectTransform clockHand;

        [ReadOnly, SerializeField] private float currentTime = 0f;
        [ReadOnly, SerializeField] private bool isLevelRunning = false;

        public bool GreaterThanZero => currentTime > 0f;
        public void StartTimer(float levelTime)
        {
            this.levelTime = levelTime;
            StartLevelTimer();
        }

        private void Update()
        {
            if (isLevelRunning && GameManager.Instance.CurrentState().Equals(GameStatus.GameState.None))
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay();
                UpdateClockHand();
                if (currentTime <= 0)
                {
                    FinishLevel();
                    currentTime = 0;
                    UpdateTimerDisplay();
                    return;
                }
            }
        }

        void StartLevelTimer()
        {
            currentTime = levelTime;
            isLevelRunning = true;
        }

        void FinishLevel()
        {
            isLevelRunning = false;
            // You can add logic here for level completion or triggering next level
            Debug.Log("<color=red>********** Level Failed **********</color>");
            GameManager.Instance.LevelFail();
        }

        void UpdateTimerDisplay()
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);

            string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
            timerText.text = timerString;
        }
        void UpdateClockHand()
        {
            float rotationFraction = currentTime / levelTime;
            float rotationAngle = rotationFraction * 360f;
            clockHand.localRotation = Quaternion.Euler(0, 0, rotationAngle);
        }

        public void IncreaseTime(float extraTime)
        {
            if (currentTime <= 0)
                currentTime += extraTime;

            isLevelRunning = true;
        }

    }
}