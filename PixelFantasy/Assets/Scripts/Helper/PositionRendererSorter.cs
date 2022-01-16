using UnityEngine;

public class PositionRendererSorter : MonoBehaviour
{
    [SerializeField] private float _offset = 0.5f;
    [SerializeField] private bool _runOnlyOnce = false;
    
    private int _sortingOrderBase = 5000;
    private float _timer;
    private float _timerMax = 0.1f;
    private Renderer _myRenderer;

    private void Awake()
    {
        _myRenderer = gameObject.GetComponent<Renderer>();
    }

    private void LateUpdate()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = _timerMax;
            _myRenderer.sortingOrder = (int)(_sortingOrderBase - transform.position.y - _offset);
            if (_runOnlyOnce)
            {
                Destroy(this);
            }
        }
    }
}
