using System;
using UnityEngine;
using UnityEngine.UI;

public class UIElement : MonoBehaviour
{
    public event Action<ISceneObject, UIElement> RequestDeleted;

    [SerializeField] private Button _deleteButton;
    [SerializeField] private Button _pickButton;

    [SerializeField] private Text _objectName;

    public event Action<ISceneObject> OnSelected;
    private Image _backgroundImage;

    private ISceneObject _linkedObject;

    public bool Initialize(ISceneObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("Passed null object.");
            Destroy(gameObject);
            return false;
        }

        _linkedObject = obj;
 
        _deleteButton.onClick.AddListener(OnDeleteClicked);

        _backgroundImage = GetComponent<Image>();

        _pickButton.onClick.AddListener(() => OnSelected?.Invoke(_linkedObject));

        _objectName.text = ((MonoBehaviour)obj).gameObject.name;

        return true;
    }

    public void SetHighlight(bool isHighlighted, Color color)
    {
        if (_backgroundImage != null)
            _backgroundImage.color = color;
    }

    private void OnDeleteClicked()
    {
        RequestDeleted?.Invoke(_linkedObject, this);
    }

    private void OnDestroy()
    {
        if (_deleteButton != null)
            _deleteButton.onClick.RemoveListener(OnDeleteClicked);
    }
}