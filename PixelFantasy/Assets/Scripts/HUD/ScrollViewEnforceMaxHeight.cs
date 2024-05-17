using UnityEngine;
using UnityEngine.UI;

[Tooltip("Attach to scroll view game object")]
public class ScrollViewEnforceMaxHeight : MonoBehaviour
{
    public RectTransform content;
    public RectTransform scrollView;
    public float maxHeight = 100f;

    void Update()
    {
        float contentHeight = content.sizeDelta.y;
        float newHeight = Mathf.Min(contentHeight, maxHeight);

        // Set the height of the Scroll View
        scrollView.sizeDelta = new Vector2(scrollView.sizeDelta.x, newHeight);
    }
}