using UnityEngine;
using System.Collections.Generic;
using Cinemachine;
using Managers;

namespace Systems.Sky.Scripts
{
    public class SkyController : MonoBehaviour
    {
        [Header("Cloud Properties")]
        public List<Sprite> cloudSprites;
        public float spawnRate = 1.0f;
        public float cloudSpeed = 0.01f;
        public int maxClouds = 20;
        public int initialClouds = 5;
        public Vector2 cloudDirection = new Vector2(1, 0); // Default direction: moving to the right
        public CinemachineVirtualCamera VCamera;

        private Transform mainCameraTranform;
        private int currentClouds = 0;

        void Start()
        {
            mainCameraTranform = VCamera.transform;
            SetupInitialClouds();
            InvokeRepeating("SpawnCloud", 0.0f, spawnRate);
        }

        void Update()
        {
            MoveAndUpdateClouds();
        }
        
        void SetupInitialClouds()
        {
            float height = VCamera.m_Lens.OrthographicSize * 2;
            float width = height * VCamera.m_Lens.Aspect;

            // Calculate initial Y spacing to distribute clouds vertically
            float initialYSpacing = height / (initialClouds + 1);

            for (int i = 0; i < initialClouds; i++)
            {
                // Randomize initial X position across the entire width of the spawn area
                float x = mainCameraTranform.position.x + Random.Range(-width / 2, width / 2);

                // Distribute initial Y positions evenly across the height of the camera view
                float y = mainCameraTranform.position.y - VCamera.m_Lens.OrthographicSize + (initialYSpacing * (i + 1));

                // Use the updated position for spawning the cloud
                SpawnInitialCloud(new Vector3(x, y, 0));
            }
        }
        
        void MoveAndUpdateClouds()
        {
            foreach (Transform child in transform)
            {
                child.position += new Vector3(cloudDirection.x * cloudSpeed, cloudDirection.y * cloudSpeed, 0) * TimeManager.Instance.DeltaTime;
                float cloudWidth = child.GetComponent<SpriteRenderer>().bounds.size.x;
                if (IsNaturallyOutOfBounds(child.position, cloudWidth))
                {
                    Destroy(child.gameObject);
                    currentClouds--;
                }
            }
        }

        void SpawnCloud()
        {
            if (currentClouds >= maxClouds) return;

            GameObject cloud = new GameObject("Cloud");
            cloud.transform.parent = this.transform;
            cloud.transform.position = GeneratePositionOutsideCameraBounds();

            SpriteRenderer sr = cloud.AddComponent<SpriteRenderer>();
            sr.sprite = cloudSprites[Random.Range(0, cloudSprites.Count)];

            currentClouds++;
        }

        void SpawnInitialCloud(Vector3 startPosition)
        {
            if (currentClouds >= maxClouds) return;

            GameObject cloud = new GameObject("Cloud");
            cloud.transform.parent = this.transform;
            cloud.transform.position = startPosition;

            SpriteRenderer sr = cloud.AddComponent<SpriteRenderer>();
            sr.sprite = cloudSprites[Random.Range(0, cloudSprites.Count)];

            currentClouds++;
        }

        Vector3 GeneratePositionOutsideCameraBounds()
        {
            float height = VCamera.m_Lens.OrthographicSize * 2;
            float width = height * VCamera.m_Lens.Aspect;

            float x = mainCameraTranform.position.x + ((cloudDirection.x > 0) ? -width / 2 : width / 2) + 5;
            float y = mainCameraTranform.position.y + Random.Range(-height / 2, height / 2);

            return new Vector3(x, y, 0);
        }

        bool IsNaturallyOutOfBounds(Vector3 position, float cloudWidth)
        {
            float width = VCamera.m_Lens.OrthographicSize * 2 * VCamera.m_Lens.Aspect;

            return position.x < mainCameraTranform.position.x - width / 2 - cloudWidth;
        }
    }
}
