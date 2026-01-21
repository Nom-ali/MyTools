using System.Collections;
using System.Diagnostics;
using UnityEngine;

[ExecuteAlways]
public class SpriteMaskSlider : MonoBehaviour, IDragController
{
    public enum AnimationType { StepByStep, Full }
    public enum Direction { Left, Right, Up, Down }

    [Header("Slider Settings")]
    [Range(0f, 1f)]
    public float sliderValue = 0f;
    public Direction direction = Direction.Right;

    [Header("References")]
    public SpriteMask spriteMask;
    public SpriteRenderer fullRenderer;
    public SpriteRenderer targetRenderer;

    [Header("Animation")]
    public AnimationType animationType = AnimationType.StepByStep;

    [Header("Step Settings")]
    [MyBox.ConditionalField(nameof(animationType), false, AnimationType.StepByStep)]
    public int stepCount = 10;

    [MyBox.ConditionalField(nameof(animationType), false, AnimationType.StepByStep)]
    [Range(0, 10)] private int currentStep = 0;

    [Header("Animation Settings")]
    public float targetValue = 0f;
    public float animationSpeed = 5f;
    public float scaleAddition = 0.05f;
    public float KnifeOffsetX = -0.3f;

    [Header("Swipe Settings")]
    [Tooltip("Minimum screen distance (pixels) to consider a swipe.")]
    public float swipeThreshold = 50f;

    [Tooltip("If true, a swipe UP will trigger full animation regardless of AnimationType.")]
    public bool fullOnSwipeUp = true;


    [MyBox.ReadOnly, SerializeField] internal Transform knife;

    private Coroutine animationCoroutine;
    internal System.Action OnComplete = null;

    // swipe bookkeeping
    private Vector2 _pointerDownPos;
    private bool _pointerDownCaptured;

    public bool CanMove { get; }

    private void Start()
    {
        if (spriteMask == null || targetRenderer == null) return;

        // Optional: Initialize state
        ApplyMaskTransform();
    }


    public void TriggerStepForward()
    {
        if (currentStep >= stepCount) return;

        currentStep++;
        StartAnimationToStep(currentStep);
    }

    public void TriggerStepBackward()
    {
        if (currentStep <= 0) return;

        currentStep--;
        StartAnimationToStep(currentStep);
    }

    public void ResetSteps()
    {
        currentStep = 0;
        StartAnimationToStep(0);
    }

    private void StartAnimationToStep(int step)
    {
        targetValue = (float)step / stepCount;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateSliderValue());
    }

    private IEnumerator AnimateSliderValue()
    {
        while (Mathf.Abs(sliderValue - targetValue) > 0.001f)
        {
            sliderValue = Mathf.Lerp(sliderValue, targetValue, Time.deltaTime * animationSpeed);
            ApplyMaskTransform();
            yield return new WaitForEndOfFrame();
        }

        sliderValue = targetValue;
        ApplyMaskTransform();
        animationCoroutine = null;

        if (currentStep >= stepCount || animationType == AnimationType.Full)
        {
            OnComplete?.Invoke();
        }
    }

    private void ApplyMaskTransform()
    {
        Vector2 targetSize = targetRenderer.bounds.size;
        Vector2 maskSpriteSize = spriteMask.sprite.bounds.size;
        Vector2 fullSize = fullRenderer.bounds.size;

        float baseScaleX = (targetSize.x > fullSize.x ? targetSize.x : fullSize.x) / maskSpriteSize.x;
        float baseScaleY =  (targetSize.y > fullSize.y ? targetSize.y : fullSize.y) / maskSpriteSize.y;

        float maskedValue = 1f - sliderValue;

        Vector3 finalScale = Vector3.one;
        Vector3 maskLocalPosition = Vector3.zero;

        switch (direction)
        {
            case Direction.Right:
                finalScale = new Vector3(baseScaleX * maskedValue, baseScaleY + scaleAddition, 1);
                maskLocalPosition = new Vector3(-targetSize.x * (1 - maskedValue) / 2f, 0, 0);
                break;

            case Direction.Left:
                finalScale = new Vector3(baseScaleX * maskedValue, baseScaleY + scaleAddition, 1);
                maskLocalPosition = new Vector3(targetSize.x * (1 - maskedValue) / 2f, 0, 0);
                break;

            case Direction.Up:
                finalScale = new Vector3(baseScaleX + scaleAddition, baseScaleY * maskedValue, 1);
                maskLocalPosition = new Vector3(0, -targetSize.y * (1 - maskedValue) / 2f, 0);
                break;

            case Direction.Down:
                finalScale = new Vector3(baseScaleX + scaleAddition, baseScaleY * maskedValue, 1);
                maskLocalPosition = new Vector3(0, targetSize.y * (1 - maskedValue) / 2f, 0);
                break;
        }

        spriteMask.transform.localScale = finalScale;
        spriteMask.transform.localPosition = maskLocalPosition;

        if (knife)
        {
            Vector3 maskWorldPos = spriteMask.transform.localPosition;
            Vector3 knifeLocalToParent = knife.transform.parent.TransformPoint(maskWorldPos);
            knife.transform.position = new Vector3(knifeLocalToParent.x + KnifeOffsetX, spriteMask.transform.position.y, 0);
        }
    }

    [MyBox.ButtonMethod]
    private void Reset()
    {
        ResetSteps();
    }

    public void OnDrag()
    {
        //Debug.Log("Dragging Detected");
    }

    //public void OnClickDown()
    //{
    //    //Debug.Log("Mouse Down Detected");
    //    if (animationCoroutine == null)
    //    {
    //        TriggerStepForward();
    //    }
    //}

    //public void OnClickUp()
    //{
    //    //Debug.Log("OnClickUp");
    //}

    public void PlayFullAnimation()
    {
        // drive to 1.0 from current sliderValue
        targetValue = 1f;
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateSliderValue());
    }

    public void OnClickDown()
    {
        _pointerDownCaptured = true;
        _pointerDownPos = GetPointerPosition();
        // Do not trigger step here anymore; we decide on OnClickUp based on swipe/tap.
    }
    public void OnClickUp()
    {
        if (!_pointerDownCaptured)
            return;

        _pointerDownCaptured = false;

        // Capture swipe data
        Vector2 upPos = GetPointerPosition();
        Vector2 delta = upPos - _pointerDownPos;

        bool isSwipe = delta.magnitude >= swipeThreshold;
        if (!isSwipe)
        {
            // Not a swipe => handle normally (e.g., step)
            if (animationType == AnimationType.StepByStep && animationCoroutine == null)
                TriggerStepForward();
            return;
        }

        // Determine swipe direction
        Direction swipeDirection = GetSwipeDirection(delta);

        // --- FULL animation type ---
        if (animationType == AnimationType.Full)
        {
            // Play full animation if swipe matches inspector direction
            if (swipeDirection == direction)
            {
                PlayFullAnimation();
            }
            return;
        }

        // --- STEP-BY-STEP animation type ---
        if (animationType == AnimationType.StepByStep)
        {
            // Optional: allow directional swipe to trigger full animation even in step mode
            if (swipeDirection == direction)
            {
                PlayFullAnimation();
                return;
            }

            // Otherwise, advance step
            if (animationCoroutine == null)
                TriggerStepForward();
        }
    }
    private Direction GetSwipeDirection(Vector2 delta)
    {
        // Compare absolute axis values to determine dominant direction
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Horizontal swipe
            return delta.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            // Vertical swipe
            return delta.y > 0 ? Direction.Up : Direction.Down;
        }
    }



    // ---- helpers ----
    private Vector2 GetPointerPosition()
    {
        return Input.mousePosition;
    }
}
