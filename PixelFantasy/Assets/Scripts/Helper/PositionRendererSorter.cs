using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

public class PositionRendererSorter : MonoBehaviour
{
    [SerializeField] private float _offset = 0.5f;
    [SerializeField] private bool _runOnlyOnce = false;
    [SerializeField] private SortingGroup _sortingGroup;
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
    }

    private void LateUpdate()
    {
        if(_isLocked) return;
        
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = _timerMax;
            
            SortRendererPosition();
            
            if (_runOnlyOnce)
            {
                DestroySelf();
            }
        }
    }

    public void SetLocked(bool isLocked)
    {
        _isLocked = isLocked;
    }
    
    private void SortRendererPosition()
    {

        float yPos = transform.position.y;
        if (_checkLocal)
        {
            yPos = transform.localPosition.y;
        }
        
        int sortingOrder = (int)(_sortingOrderBase - (yPos + _offset) * 10);
        if (_sortingGroup != null)
        {
            _sortingGroup.sortingOrder = sortingOrder;
        }
        else
        {
            _myRenderer.sortingOrder = sortingOrder;
        }
    }

    public void DestroySelf()
    {
        Destroy(this);
    }

    [Button("Sort Position")]
    private void EditorSortPosition()
    {
        _myRenderer = gameObject.GetComponent<Renderer>();
        _sortingGroup = gameObject.GetComponent<SortingGroup>();
        SortRendererPosition();
    }

    public float Offset => _offset;
}
