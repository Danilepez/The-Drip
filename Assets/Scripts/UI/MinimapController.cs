using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Image mapImage;
    [SerializeField] private RectTransform playerIcon;
    [SerializeField] private Transform player;

    [Header("Floor Sprites")]
    [SerializeField] private Sprite secondFloorMap;
    [SerializeField] private Sprite firstFloorMap;
    [SerializeField] private Sprite basementMap;

    [Header("World Bounds")]
    [SerializeField] private Vector2 worldMin = new Vector2(-35f, -55f);
    [SerializeField] private Vector2 worldMax = new Vector2(65f, 30f);

    [Header("Floor Heights")]
    [SerializeField] private float basementMaxY = 0.45f;
    [SerializeField] private float firstFloorMaxY = 3.5f;

    [Header("Options")]
    [SerializeField] private bool rotateIconWithPlayer = true;

    private RectTransform mapRect;
    private Sprite currentSprite;

    private void Awake()
    {
        CacheReferences();
    }

    private void Start()
    {
        UpdateMapForCurrentFloor();
        UpdatePlayerIcon();
    }

    private void LateUpdate()
    {
        CacheReferences();
        UpdateMapForCurrentFloor();
        UpdatePlayerIcon();
    }

    private void CacheReferences()
    {
        if (mapImage == null)
        {
            mapImage = GetComponent<Image>();
        }

        if (mapRect == null && mapImage != null)
        {
            mapRect = mapImage.rectTransform;
        }

        if (playerIcon == null)
        {
            Transform iconTransform = transform.Find("PlayerIcon");
            if (iconTransform != null)
            {
                playerIcon = iconTransform as RectTransform;
            }
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void UpdateMapForCurrentFloor()
    {
        if (mapImage == null || player == null)
        {
            return;
        }

        Sprite targetSprite = GetSpriteForFloor(player.position.y);
        if (targetSprite != null && targetSprite != currentSprite)
        {
            currentSprite = targetSprite;
            mapImage.sprite = targetSprite;
        }
    }

    private void UpdatePlayerIcon()
    {
        if (playerIcon == null || player == null || mapRect == null)
        {
            return;
        }

        float normalizedX = Mathf.InverseLerp(worldMin.x, worldMax.x, player.position.x);
        float normalizedY = Mathf.InverseLerp(worldMin.y, worldMax.y, player.position.z);

        normalizedX = Mathf.Clamp01(normalizedX);
        normalizedY = Mathf.Clamp01(normalizedY);

        Vector2 mapSize = mapRect.rect.size;
        Vector2 iconPosition = new Vector2((normalizedX - 0.5f) * mapSize.x, (normalizedY - 0.5f) * mapSize.y);
        playerIcon.anchoredPosition = iconPosition;

        if (rotateIconWithPlayer)
        {
            playerIcon.localRotation = Quaternion.Euler(0f, 0f, -player.eulerAngles.y);
        }
    }

    private Sprite GetSpriteForFloor(float playerHeight)
    {
        if (playerHeight <= basementMaxY)
        {
            return basementMap != null ? basementMap : firstFloorMap;
        }

        if (playerHeight <= firstFloorMaxY)
        {
            return firstFloorMap != null ? firstFloorMap : basementMap;
        }

        return secondFloorMap != null ? secondFloorMap : firstFloorMap;
    }
}
