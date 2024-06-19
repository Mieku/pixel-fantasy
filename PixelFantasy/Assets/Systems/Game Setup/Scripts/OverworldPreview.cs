using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Systems.Game_Setup.Scripts
{
    public class OverworldPreview : MonoBehaviour
    {
        [SerializeField] private ChooseWorldPanel _chooseWorldPanel;
        
        public Camera MainCam;
        public RawImage rawImage; // Reference to the Raw Image
        public Camera tilemapCamera; // Reference to the Camera rendering the Tilemap
        public RectTransform rawImageRectTransform; // Reference to the Raw Image's RectTransform
        public GameObject PlacementIcon;
        public Canvas OverworldCanvas;
        public Tilemap tilemap; // Reference to the Tilemap
        
        public float movementSensitivity = 0.1f; // Movement sensitivity multiplier
        public float zoomSensitivity = 1.0f; // Zoom sensitivity multiplier
        public BoxCollider2D boundaryCollider; // 2D Box Collider defining the boundary

        private Vector3 lastMousePosition;
        private bool isDragging;
        private bool placementLocked;
        public bool IsEnabled;

        void Update()
        {
            if (!IsEnabled) return;
            if(placementLocked) return;
            
            // Check if the mouse is over the Raw Image
            var inView = RectTransformUtility.RectangleContainsScreenPoint(rawImageRectTransform, Input.mousePosition, MainCam);
            if (!inView && !isDragging)
            {
                PlacementIcon.SetActive(false);
                return;
            }
            else
            {
                PlacementIcon.SetActive(true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
                isDragging = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                DetectTiles();
            }

            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                lastMousePosition = Input.mousePosition;

                Vector3 cameraMovement = new Vector3(-delta.x / Screen.width, -delta.y / Screen.height, 0);
                tilemapCamera.transform.Translate(cameraMovement * tilemapCamera.orthographicSize * movementSensitivity, Space.World);

                // Clamp the camera position within the defined boundaries
                ClampCameraPosition();
            }

            // Handle zooming with the mouse wheel
            if (inView && Input.mouseScrollDelta.y != 0)
            {
                float newSize = tilemapCamera.orthographicSize - Input.mouseScrollDelta.y * zoomSensitivity;
                tilemapCamera.orthographicSize = Mathf.Clamp(newSize, 5f, 12f); // Clamping the zoom level

                // Re-apply the boundary constraints after zooming
                ClampCameraPosition();
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRectTransform, Input.mousePosition, MainCam, out var localPoint);

            // Convert local point to normalized coordinates
            Vector2 normalizedPoint = new Vector2(
                (localPoint.x + rawImageRectTransform.rect.width * 0.5f) / rawImageRectTransform.rect.width,
                (localPoint.y + rawImageRectTransform.rect.height * 0.5f) / rawImageRectTransform.rect.height
            );

            // Convert normalized coordinates to world position
            Ray ray = tilemapCamera.ViewportPointToRay(new Vector3(normalizedPoint.x, normalizedPoint.y, 0));
            Plane plane = new Plane(Vector3.forward, Vector3.zero);
            float distance;

            if (plane.Raycast(ray, out distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);
                worldPosition.x -= OverworldCanvas.transform.position.x;
                worldPosition.y -= OverworldCanvas.transform.position.y;

                worldPosition.x = Mathf.RoundToInt(worldPosition.x);
                worldPosition.y = Mathf.RoundToInt(worldPosition.y);

                PlacementIcon.transform.localPosition = worldPosition;
            }
        }

        private void ClampCameraPosition()
        {
            Bounds bounds = boundaryCollider.bounds;
            Vector3 cameraPosition = tilemapCamera.transform.position;
            float halfWidth = tilemapCamera.orthographicSize * tilemapCamera.aspect;
            float halfHeight = tilemapCamera.orthographicSize;

            float clampedX = Mathf.Clamp(cameraPosition.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
            float clampedY = Mathf.Clamp(cameraPosition.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);

            tilemapCamera.transform.position = new Vector3(clampedX, clampedY, cameraPosition.z);
        }

        private void DetectTiles()
        {
            Vector3 worldPosition = PlacementIcon.transform.position;

            // Determine the positions of the four surrounding tiles
            Vector3Int tilePos1 = tilemap.WorldToCell(worldPosition + new Vector3(-0.5f, 0.5f, 0)); // Top-left
            Vector3Int tilePos2 = tilemap.WorldToCell(worldPosition + new Vector3(0.5f, 0.5f, 0));  // Top-right
            Vector3Int tilePos3 = tilemap.WorldToCell(worldPosition + new Vector3(-0.5f, -0.5f, 0)); // Bottom-left
            Vector3Int tilePos4 = tilemap.WorldToCell(worldPosition + new Vector3(0.5f, -0.5f, 0));  // Bottom-right

            // Read the data from these four tiles
            TileBase tile1 = tilemap.GetTile(tilePos1);
            TileBase tile2 = tilemap.GetTile(tilePos2);
            TileBase tile3 = tilemap.GetTile(tilePos3);
            TileBase tile4 = tilemap.GetTile(tilePos4);

            var data = GetSelectionData( tile1, tile2, tile3, tile4, worldPosition);
            _chooseWorldPanel.GetSelectionData(data);
        }

        private OverworldSelectionData GetSelectionData(TileBase topLeftTile, TileBase topRightTile, TileBase bottomLeftTile,
            TileBase bottomRightTile, Vector2 position)
        {
            var topLeft = ConvertTileToFeature(topLeftTile);
            var topRight = ConvertTileToFeature(topRightTile);
            var bottomLeft = ConvertTileToFeature(bottomLeftTile);
            var bottomRight = ConvertTileToFeature(bottomRightTile);

            OverworldSelectionData data = new OverworldSelectionData()
            {
                Position = position,
                TopLeftFeature = topLeft,
                TopRightFeature = topRight,
                BottomLeftFeature = bottomLeft,
                BottomRightFeature = bottomRight
            };

            return data;
        }

        private EFeatureTileType ConvertTileToFeature(TileBase tile)
        {
            if (tile == null) return EFeatureTileType.Ocean;
            
            var tileName = tile.name;
            switch (tileName)
            {
                case "Grass":
                    return EFeatureTileType.Grass;
                case "Mountain":
                    return EFeatureTileType.Mountain;
                case "River":
                    return EFeatureTileType.River;
                case "Dungeon":
                    return EFeatureTileType.Dungeon;
                case "Forest":
                    return EFeatureTileType.Forest;
                case "Ocean":
                default:
                    return EFeatureTileType.Ocean;
            }
        }

        [Serializable]
        public class OverworldSelectionData
        {
            public Vector2 Position;
            public EFeatureTileType TopLeftFeature;
            public EFeatureTileType TopRightFeature;
            public EFeatureTileType BottomLeftFeature;
            public EFeatureTileType BottomRightFeature;
        }

        public enum EFeatureTileType
        {
            Ocean,
            River,
            Grass,
            Forest,
            Mountain,
            Dungeon,
        }

        public void SetPlacementLocked(bool isLocked)
        {
            placementLocked = isLocked;
        }
    }
}
