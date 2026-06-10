using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPickerIntegration : MonoBehaviour
{
    // поле интеграции 
    [SerializeField] private FlexibleColorPicker _fcp;

    private Renderer _currentSelectedRenderer;

    private void Start()
    {
        if (_fcp ==  null)
        {
            Debug.LogError("Null color picker.");
            this.enabled = false;
            return;
        }
        ObjectSelector.Instance.OnSelectionChanged += SyncPickerToObjectColor;

        _fcp.onColorChange.AddListener(ApplyColorToSelectedObject);
    }

    private void SyncPickerToObjectColor(ISceneObject obj)
    {
        if (_fcp != null && obj != null)
        {
            _currentSelectedRenderer = null;

            Color targetColor = Color.white;

            var renderer = obj.GetRenderer();

            if (renderer == null)
            {
                Debug.LogError("Color field was not found on that object.");
                return;
            }

            targetColor = renderer.material.color;
            _fcp.color = targetColor;
            _currentSelectedRenderer = renderer;
        }
        else Debug.LogError("ColorPickerIntegrationLayer - component is null.");
    }

    private void ApplyColorToSelectedObject(Color newColor)
    {
        if (_currentSelectedRenderer != null)
        {
            _currentSelectedRenderer.material.color = newColor;
        }
    }
}