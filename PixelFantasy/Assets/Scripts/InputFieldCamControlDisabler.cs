using Controllers;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldCamControlDisabler : MonoBehaviour
{
    private TMP_InputField _input;
    private bool _disabledInput;

    void Awake()
    {
        _input = GetComponent<TMP_InputField>();

        _input.onSelect.AddListener(InputSelected);
        _input.onDeselect.AddListener(InputDeseletected);
    }

    private void OnDestroy()
    {
        if (_disabledInput)
        {
            DisableCamInput(false);
        }
        
        _input.onSelect.RemoveListener(InputSelected);
        _input.onDeselect.RemoveListener(InputDeseletected);
    }

    private void InputSelected(string input)
    {
        DisableCamInput(true);
    }

    private void InputDeseletected(string input)
    {
        DisableCamInput(false);
    }

    private void DisableCamInput(bool disabled)
    {
        CameraManager.Instance.IgnoreKeyboardInput = disabled;
        _disabledInput = disabled;
    }
}
