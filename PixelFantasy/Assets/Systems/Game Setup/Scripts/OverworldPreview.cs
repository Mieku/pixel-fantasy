using System;
using System.Collections;
using UnityEngine;
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
        public GameObject PlacementIconSelected;
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
            if (!inView)// && !isDragging)
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
                //isDragging = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                //isDragging = false;
                SelectPosition(Input.mousePosition);
            }

            // if (isDragging)
            // {
            //     Vector3 delta = Input.mousePosition - lastMousePosition;
            //     lastMousePosition = Input.mousePosition;
            //
            //     Vector3 cameraMovement = new Vector3(-delta.x / Screen.width, -delta.y / Screen.height, 0);
            //     tilemapCamera.transform.Translate(cameraMovement * tilemapCamera.orthographicSize * movementSensitivity, Space.World);
            //
            //     // Clamp the camera position within the defined boundaries
            //     ClampCameraPosition();
            // }
            //
            // // Handle zooming with the mouse wheel
            // if (inView && Input.mouseScrollDelta.y != 0)
            // {
            //     float newSize = tilemapCamera.orthographicSize - Input.mouseScrollDelta.y * zoomSensitivity;
            //     tilemapCamera.orthographicSize = Mathf.Clamp(newSize, 5f, 12f); // Clamping the zoom level
            //
            //     // Re-apply the boundary constraints after zooming
            //     ClampCameraPosition();
            // }

            PlacementIcon.transform.position = ConvertMousePosToOverworldPos(Input.mousePosition);
            
            // Check if valid
            if (!_isDoingInvalidSequence)
            {
                var posData = DetectTiles(Input.mousePosition);
                if(IsOverworldSelectionValid(posData))
                {
                    PlacementIcon.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    PlacementIcon.GetComponent<Image>().color = Color.red;
                }
            }
        }

        private Vector2 ConvertMousePosToOverworldPos(Vector2 mousePos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRectTransform, mousePos, MainCam, out var localPoint);

            // Convert local point to normalized coordinates
            Vector2 normalizedPoint = new Vector2(
                (localPoint.x + rawImageRectTransform.rect.width * 0.5f) / rawImageRectTransform.rect.width,
                (localPoint.y + rawImageRectTransform.rect.height * 0.5f) / rawImageRectTransform.rect.height
            );

            // Convert normalized coordinates to world position
            Ray ray = tilemapCamera.ViewportPointToRay(new Vector3(normalizedPoint.x, normalizedPoint.y, 0));
            Plane plane = new Plane(Vector3.forward, Vector3.zero);

            if (plane.Raycast(ray, out var distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);

                worldPosition.x = Mathf.RoundToInt(worldPosition.x);
                worldPosition.y = Mathf.RoundToInt(worldPosition.y);

                return worldPosition;
            }
    
            return Vector2.negativeInfinity;
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

        public void SelectPosition(Vector2 pos)
        {
            var posData = DetectTiles(pos);

            if (IsOverworldSelectionValid(posData))
            {
                PlacementIconSelected.SetActive(true);
                PlacementIconSelected.transform.position = ConvertMousePosToOverworldPos(pos);
                _chooseWorldPanel.GetSelectionData(posData);
            }
            else
            {
                StartCoroutine(InvalidSelectionSequence());
            }
        }

        private bool _isDoingInvalidSequence;
        private IEnumerator InvalidSelectionSequence()
        {
            // Have PlacementIcon flash red 3 times and then return to normal colour
            var placementIconImage = PlacementIcon.GetComponent<Image>();
            _isDoingInvalidSequence = true;
            
            for (int i = 0; i < 2; i++)
            {
                placementIconImage.color = Color.red;
                yield return new WaitForSeconds(0.15f);
                placementIconImage.color = Color.white;
                yield return new WaitForSeconds(0.15f);
            }

            _isDoingInvalidSequence = false;
        }

        private bool IsOverworldSelectionValid(OverworldSelectionData selectData)
        {
            int numSafe = 0;

            if (selectData.BottomLeftFeature is EFeatureTileType.Grass or EFeatureTileType.Forest)
            {
                numSafe++;
            }
            
            if (selectData.BottomRightFeature is EFeatureTileType.Grass or EFeatureTileType.Forest)
            {
                numSafe++;
            }
            
            if (selectData.TopLeftFeature is EFeatureTileType.Grass or EFeatureTileType.Forest)
            {
                numSafe++;
            }
            
            if (selectData.TopRightFeature is EFeatureTileType.Grass or EFeatureTileType.Forest)
            {
                numSafe++;
            }
            
            return numSafe >= 2;
        }

        private OverworldSelectionData DetectTiles(Vector2 pos)
        {
            Vector3 worldPosition = ConvertMousePosToOverworldPos(pos);

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
            return data;
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
                case "Mountains":
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