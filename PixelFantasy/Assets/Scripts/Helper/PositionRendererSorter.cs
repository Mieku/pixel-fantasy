using UnityEngine;
using UnityEngine.Rendering;

public class PositionRendererSorter : MonoBehaviour
{
    [SerializeField] private float _offset = 0.5f;
    [SerializeField] private bool _runOnlyOnce = false;
    [SerializeField] private SortingGroup _sortingGroup;
    
    private int _sortingOrderBase = 0;
    private float _timer;
    private float _timerMax = 0.1f;
    private Renderer _myRenderer;

    private void Awake()
    {
        _myRenderer = gameObject.GetComponent<Renderer>();
        _sortingGroup = gameObject.GetComponent<SortingGroup>();
    }

    private void LateUpdate()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = _timerMax;


            if (_sortingGroup != null)
            {
                _sortingGroup.sortingOrder = (int)(_sortingOrderBase - (transform.position.y + _offset) * 10);
            }
            else
            {
                _myRenderer.sortingOrder = (int)(_sortingOrderBase - (transform.position.y + _offset) * 10);
            }
            
            
            
            
            
            if (_runOnlyOnce)
            {
                Destroy(this);
            }
        }
    }
}
