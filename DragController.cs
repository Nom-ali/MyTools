using DG.Tweening;
using System.Collections;
using UnityEngine;

public class DragController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private int[] ObjectID;
    [SerializeField] private Vector3 InitialRotation;
    [SerializeField] private ScaleType UseScale = ScaleType.None;
    [MyBox.ConditionalField(nameof(UseScale), false, ScaleType.ByValue)]
    [SerializeField] private float  multiplier = 1f;
    
    [SerializeField] private bool Placed = false;
    
    [Space]
    [MyBox.ReadOnly, SerializeField] private string compareTag = "Placement";
    [MyBox.ReadOnly, SerializeField] private Transform CurrentCollider = null;

    public bool ItemPlaced => Placed;

    private Vector3 OrignalScale;
    private Vector3 OriginalPosition;
    private Quaternion OriginalRotation;

    private int SortingOrder;

    Coroutine coroutine = null;

    private void Start()
    {
        if(coroutine == null) 
            spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void OnMouseDown()
    {
        if(coroutine == null)
            coroutine = StartCoroutine(OnClicked());
    }

    private void OnMouseUp()
    {
        StartCoroutine(OnUp());
    }

    private IEnumerator OnClicked()
    {
        // get orignal values
        OriginalPosition = transform.position;
        OriginalRotation = transform.rotation;
        OrignalScale = transform.localScale;

        SortingOrder = spriteRenderer.sortingOrder;

        // Set Sorting Order
        spriteRenderer.sortingOrder = 100;

        //Scaling Object
        if (UseScale == ScaleType.ByValue)
            transform.localScale = transform.localScale * multiplier;

        while (Input.GetMouseButton(0))
        {
            // Get position
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            mouseWorldPosition.z = 0; // Ensure z position is 0 for 2D

            // Apply offset to the sprite position
            transform.position = mouseWorldPosition; 
            
            // Smoothly rotate towards the target rotation
            Quaternion targetQuaternion = Quaternion.Euler(InitialRotation);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetQuaternion, 500 * Time.deltaTime);

            yield return null;
        }
    }

    IEnumerator OnUp()
    {
        yield return null;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
      
        // Placing object in its placement
        if (!Placed && CurrentCollider != null && System.Array.Exists(ObjectID, ID => ID == CurrentCollider.GetComponent<PlacementCollider>().ObjectID))
        {
            spriteRenderer.sortingOrder = 1;
            Vector3 tempPos = CurrentCollider.position;
            transform.DOMove(tempPos, 0.3f);

            if(UseScale == ScaleType.ByValue)
                transform.DOScale(OrignalScale, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    transform.parent = CurrentCollider;
                });
            else
                transform.parent = CurrentCollider;

            AudioPlayer.instance?.PlayOneShot(AudioType.Droppable);
            GameManager.Instance.ShowParticle(CurrentCollider.transform.position);

            GetComponent<Collider2D>().enabled = false;
            CurrentCollider.GetComponent<Collider2D>().enabled = false;

            Placed = true;
            GameManager.Instance.CheckLevelComplete();
        }
        else
        {
            transform.DOMove(OriginalPosition, 0.5f).OnComplete(() => spriteRenderer.sortingOrder = SortingOrder );
            transform.DORotate(OriginalRotation.eulerAngles, 0.5f);
            transform.DOScale(OrignalScale, 0.5f);
        }
    }

    public void OnTriggerEnterObject(Collider2D collision)
    {
        if (collision.CompareTag(compareTag))
        {
            CurrentCollider = collision.transform;
        }
    }

    public void OnTriggerExitObject(Collider2D collision)
    {
        if (collision.CompareTag(compareTag))
        {
            CurrentCollider = null;
        }
    }

}

[System.Serializable]
public enum ScaleType
{
    None, ByValue, KeepOrignal
}
