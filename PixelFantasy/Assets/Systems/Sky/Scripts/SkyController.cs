using UnityEngine;
using System.Collections.Generic;
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

        private Transform mainCamera;
        private int currentClouds = 0;

        void Start()
        {
            mainCamera = Camera.main.transform;

            float height = Camera.main.orthographicSize * 2;
            float initialYSpacing = height / initialClouds;

            for (int i = 0; i < initialClouds; i++)
            {
                float yPos = mainCamera.position.y + initialYSpacing * i - (height / 2);
                SpawnInitialCloud(new Vector3(mainCamera.position.x + Random.Range(-height * Camera.main.aspect / 2, height * Camera.main.aspect / 2), yPos, 0));
            }

            InvokeRepeating("SpawnCloud", 0.0f, spawnRate);
        }

        void Update()
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
            float height = Camera.main.orthographicSize * 2;
            float width = height * Camera.main.aspect;

            float x = mainCamera.position.x + ((cloudDirection.x > 0) ? -width / 2 : width / 2) + 5;
            float y = mainCamera.position.y + Random.Range(-height / 2, height / 2);

            return new Vector3(x, y, 0);
        }

        bool IsNaturallyOutOfBounds(Vector3 position, float cloudWidth)
        {
            float width = Camera.main.orthographicSize * 2 * Camera.main.aspect;

            return position.x < mainCamera.position.x - width / 2 - cloudWidth;
        }
    }
}
