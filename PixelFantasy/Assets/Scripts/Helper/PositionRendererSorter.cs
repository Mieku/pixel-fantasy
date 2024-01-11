using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

public class PositionRendererSorter : MonoBehaviour
{
    [SerializeField] private float _offset = 0.5f;
    [SerializeField] private bool _runOnlyOnce = false;
    private SortingGroup _sortingGroup;
    [SerializeField] private bool _checkLocal;
    
    private int _sortingOrderBase = 0;
    private float _timer;
    private float _timerMax = 0.1f;
    private Renderer _myRenderer;
    private bool _isLocked;

    private void Awake()
    {
        _myRenderer = gameObject.GetComponent<Renderer>();
        _sortingGroup = gameObject.GetComponent<SortingGroup>();

        if (_myRenderer == null && _sortingGroup == null)
        {
            Debug.LogWarning("PositionRendererSorter: Missing Renderer and SortingGroup components on Awake.");
        }
    }

    private void LateUpdate()
    {
        if (_isLocked) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = _timerMax;
            DebugComponentState();
            SortRendererPosition();
            
            if (_runOnlyOnce)
            {
                DestroySelf();
            }
        }
    }

    private void DebugComponentState()
    {
        if (_myRenderer == null && _sortingGroup == null)
        {
            Debug.LogError("PositionRendererSorter: Both Renderer and SortingGroup are null.");
        }
        else
        {
            if (_myRenderer != null && !_myRenderer.enabled)
            {
                Debug.LogWarning("PositionRendererSorter: Renderer is disabled.");
            }
            if (_sortingGroup != null && !_sortingGroup.enabled)
            {
                Debug.LogWarning("PositionRendererSorter: SortingGroup is disabled.");
            }
        }
    }

    private void SortRendererPosition()
    {
        float yPos = _checkLocal ? transform.localPosition.y : transform.position.y;
        int sortingOrder = Mathf.Clamp((int)(_sortingOrderBase - (yPos + _offset) * 10), -32768, 32767);

        if (_sortingGroup != null)
        {
            _sortingGroup.sortingOrder = sortingOrder;
        }
        else if (_myRenderer != null)
        {
            _myRenderer.sortingOrder = sortingOrder;
        }
        else
        {
            Debug.LogError("PositionRendererSorter: No Renderer or SortingGroup to sort at runtime.");
        }
    }

    public void SetLocked(bool isLocked)
    {
        _isLocked = isLocked;
    }

    public void DestroySelf()
    {
        Destroy(this);
    }

    [Button("Sort Position")]
    private void EditorSortPosition()
    {
        DebugComponentState();
        SortRendererPosition();
    }

    public float Offset => _offset;
}
