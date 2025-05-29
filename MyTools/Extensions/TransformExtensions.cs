using UnityEngine;

public static class TransformExtensions
{
    public static Vector3 Reposition(this Transform transform, Directioons direction, Vector3 offset)
    {
        Camera camera = Camera.main;
        if (camera == null) 
        {
            Debug.Log("No Camera Found");
            return Vector3.zero;
        }
        Vector3 screenBottomLeft = camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
        Vector3 screenTopRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, camera.nearClipPlane));

        if (!transform.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            Debug.LogError("No SpriteRenderer found on the item.");
            return Vector3.zero;
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
                targetY = yMax - offset.y;
                targetX = Random.Range(xMin + offset.x, xMax - offset.x);
                break;
            case Directioons.Bottom:
                targetY = yMin + offset.y;
                targetX = Random.Range(xMin + offset.x, xMax - offset.x);
                break;
            case Directioons.Left:
                targetX = xMin + offset.x;
                targetY = Random.Range(yMin + offset.y, yMax - offset.y);
                break;
            case Directioons.Right:
                targetX = xMax - offset.x;
                targetY = Random.Range(yMin + offset.y, yMax - offset.y);
                break;
            case Directioons.Center:
                targetX = (xMin + xMax) / 2f;
                targetY = (yMin + yMax) / 2f;
                break;

            case Directioons.Random:
            default:
                targetX = Random.Range(xMin + offset.x, xMax - offset.x);
                targetY = Random.Range(yMin + offset.y, yMax - offset.y);
                break;
        }

        return new(targetX, targetY, 0);
    }
}

public enum Directioons
{
    None,
    Top,
    Bottom,
    Left,
    Right,
    Center,
    Random
}