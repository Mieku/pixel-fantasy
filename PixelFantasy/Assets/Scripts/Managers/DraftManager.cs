using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Managers
{
    public class DraftManager : Singleton<DraftManager>
    {
        [SerializeField] private KinlingPositionPreview _positionPreviewPrefab;
    
        private List<KinlingPositionPreview> _positionPreviews = new List<KinlingPositionPreview>();
        private List<Kinling> _selectedDraftedKinlings = new List<Kinling>();
        private Vector2 _previewStartPosition;

        public void BeginOrdersPreview(List<Kinling> draftedKinlings, Vector2 startPos)
        {
            ClearPreviews();
            _selectedDraftedKinlings = draftedKinlings;
            _previewStartPosition = startPos;
            foreach (var kinling in _selectedDraftedKinlings)
            {
                CreatePositionPreview(kinling);
            }
        }

        // Happens on right click drag
        public void ContinueOrdersPreview(Vector2 currentPos)
        {
            var positions = GetPositionsWithin(_previewStartPosition, currentPos, _selectedDraftedKinlings.Count);
            for (int i = 0; i < _selectedDraftedKinlings.Count; i++)
            {
                var pos = positions[i];
                _positionPreviews[i].transform.position = pos;
            }
        }

        // Happens on right click released
        public void CompleteOrdersPreview()
        {
            ClearPreviews();
            _selectedDraftedKinlings.Clear();
        }

        private void ClearPreviews()
        {
            foreach (var preview in _positionPreviews)
            {
                Destroy(preview.gameObject);
            }
            _positionPreviews.Clear();
        }

        private void CreatePositionPreview(Kinling kinling)
        {
            var preview = Instantiate(_positionPreviewPrefab, transform);
            preview.gameObject.SetActive(true);
            preview.Init(kinling.RuntimeData);
            _positionPreviews.Add(preview);
        }

        private List<Vector2> GetPositionsWithin(Vector2 startPos, Vector2 endPos, int numPositions)
        {
            List<Vector2> results = new List<Vector2>();
    
            if (numPositions == 1)
            {
                return new List<Vector2>() { endPos };
            }

            if (numPositions == 2)
            {
                return new List<Vector2>() { startPos, endPos };
            }
    
            // Add the start position as the first item
            results.Add(startPos);

            // Calculate the step size based on the number of positions to generate
            Vector2 step = (endPos - startPos) / (numPositions - 1);

            // Generate intermediate positions
            for (int i = 1; i < numPositions - 1; i++)
            {
                Vector2 position = startPos + step * i;
                results.Add(position);
            }

            // Add the end position as the last item
            results.Add(endPos);

            return results;
        }

    }
}
