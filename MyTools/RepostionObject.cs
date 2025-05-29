using UnityEngine;
using DG.Tweening;

public class RepostionObject
{
    [SerializeField] private Vector2 PositionOffset = Vector2.zero;
 
    private float zOffset = 0;

    public RepostionObject(Vector3 offset)
    {
        PositionOffset = offset;
    }

    internal void Reposition(Transform item, Directioons direction, System.Action onComplete = null)
    {
        Camera camera = Camera.main;
        if (camera == null) return;

        Vector3 screenBottomLeft = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        Vector3 screenTopRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camera.nearClipPlane));

        if (!item.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            Debug.LogError("No SpriteRenderer found on the item.");
            return;
        }

        Vector2 spriteSize = spriteRenderer.bounds.size;

        float xMin = screenBottomLeft.x + spriteSize.x / 2;
        float xMax = screenTopRight.x - spriteSize.x / 2;
        float yMin = screenBottomLeft.y + spriteSize.y / 2;
        float yMax = screenTopRight.y - spriteSize.y / 2;

        float targetY;
        float targetX;

        switch (direction)
        {
            case Directioons.Top:
                targetY = yMax - PositionOffset.y;
                targetX = Random.Range(xMin + PositionOffset.x, xMax - PositionOffset.x);
                break;
            case Directioons.Bottom:
                targetY = yMin + PositionOffset.y;
                targetX = Random.Range(xMin + PositionOffset.x, xMax - PositionOffset.x);
                break;
            case Directioons.Left:
                targetX = xMin + PositionOffset.x;
                targetY = Random.Range(yMin + PositionOffset.y, yMax - PositionOffset.y);
                break;
            case Directioons.Right:
                targetX = xMax - PositionOffset.x;
                targetY = Random.Range(yMin + PositionOffset.y, yMax - PositionOffset.y);
                break;
            case Directioons.Center:
                targetX = (xMin + xMax) / 2f;
                targetY = (yMin + yMax) / 2f;
                break;
            case Directioons.Random:
            default:
                targetX = Random.Range(xMin + PositionOffset.x, xMax - PositionOffset.x);
                targetY = Random.Range(yMin + PositionOffset.y, yMax - PositionOffset.y);
                break;
        }

        item.gameObject.SetActive(true);
        item.transform.parent = null;
        Vector2 size = item.GetComponent<SpriteRenderer>().sprite.bounds.size;
        Debug.Log($"Scaling down {item.name} from {size.x}", item);
        if (size.x > 5f)
        {
            item.transform.localScale /= 1.2f;
        }
      
        zOffset += -0.01f;
        Vector3 position = new(targetX, targetY, zOffset);
        Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(-360f, 360f));

        Debug.Log($"Positioning {item.name} to {position} with rotation {randomRotation.eulerAngles}", item.gameObject);
        item.DORotate(randomRotation.eulerAngles, 0.3f);
        item.DOScale(item.localScale, 0.7f).From(Vector3.zero).SetEase(Ease.OutBack);
        item.DOLocalMove(position, 0.7f).OnComplete(() =>
        {
            Debug.Log("Coroutine completed", item.gameObject);
        });      
    }


}
