using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// подписывается на события GameSceneManager
// создает и удаляет UI элементы
// служит связующим между GameSceneManager и UIElement

public class UIController : MonoBehaviour
{
    public static UIController Instance {  get; private set; }

    // минус - жесткая привязка к реализации
    private GameSceneManager _gameSceneManager;
    private ObjectSelector _objectSelector;

    [SerializeField] private GameObject _contentParent;
    [SerializeField] private GameObject _content;
    [SerializeField] private ScrollRect _scrollRect;

    [SerializeField] private Color _normalColor = Color.green;
    [SerializeField] private Color _selectedColor = Color.yellow;
    [SerializeField] private float _scrollDuration = 0.3f;

    private UIElement _selectedElement;
    private Coroutine _scrollCoroutine;
    private Dictionary<ISceneObject, UIElement> _elements = new();

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        if (_contentParent == null || _content ==  null || _scrollRect == null)
        {
            Debug.LogError("Critical failure.");
            this.enabled = false;
        }
        _elements = new Dictionary<ISceneObject, UIElement>();
    }

    private void Start()
    {
        _gameSceneManager = GameSceneManager.Instance;
        _objectSelector = ObjectSelector.Instance;
        if (_gameSceneManager == null || _objectSelector == null)
        {
            Debug.LogError("Critical failure.");
            this.enabled = false;
            return;
        }

        _gameSceneManager.OnObjectAdded += CreateItem;
        _objectSelector.OnSelectionChanged += CameraSelectionChanged;
    }

    // создание ui элемента + вызов инииализации + подписка
    public void CreateItem(ISceneObject obj)
    {
        GameObject itm = Instantiate(_content, _contentParent.transform);

        UIElement element = itm.GetComponent<UIElement>();

        if (element == null)
        {
            Debug.LogError("No UIElement component on prefab.");
            return;
        }

        element.Initialize(obj);
        element.RequestDeleted += HandleDeleteRequest;

        _elements.Add(obj, element);
        element.OnSelected += HandleUISelection;

        element.SetHighlight(false, _normalColor);
    }

    // удаление + отписка
    private void HandleDeleteRequest(ISceneObject obj, UIElement element)
    {
        _elements.Remove(obj);
        element.OnSelected -= HandleUISelection;
        if (_selectedElement == element) _selectedElement = null;

        _gameSceneManager.RemoveObject(obj);
        element.RequestDeleted -= HandleDeleteRequest;
        Destroy(element.gameObject);
    }

    private void CameraSelectionChanged(ISceneObject obj)
    {
        if (obj == null)
        {
            ClearSelection();
            return;
        }

        if (_elements.TryGetValue(obj, out UIElement element))
        {
            SelectElement(element);
            ScrollToObject(element);
        }
    }

    private void HandleUISelection(ISceneObject obj)
    {
        _objectSelector.SelectObject(obj);
    }

    private void SelectElement(UIElement element)
    {
        if (_selectedElement != null)
            _selectedElement.SetHighlight(false, _normalColor);

        _selectedElement = element;
        element.SetHighlight(true, _selectedColor);
    }

    private void ClearSelection()
    {
        if (_selectedElement != null)
        {
            _selectedElement.SetHighlight(false, _normalColor);
            _selectedElement = null;
        }
    }

    private void ScrollToObject(UIElement element)
    {
        if (_scrollCoroutine != null)
            StopCoroutine(_scrollCoroutine);

        _scrollCoroutine = StartCoroutine(ScrollCoroutine(element));
    }

    private IEnumerator ScrollCoroutine(UIElement element)
    {
        RectTransform contentRect = _scrollRect.content;
        RectTransform elementRect = element.GetComponent<RectTransform>();

        float targetPosition = CalculateTargetPosition(elementRect, contentRect);
        float startPosition = _scrollRect.verticalNormalizedPosition;
        float elapsed = 0f;

        while (elapsed < _scrollDuration)
        {
            elapsed += Time.deltaTime;
            _scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, elapsed / _scrollDuration);
            yield return null;
        }

        _scrollRect.verticalNormalizedPosition = targetPosition;
    }

    private float CalculateTargetPosition(RectTransform element, RectTransform content)
    {
        float contentHeight = content.rect.height;
        float viewportHeight = _scrollRect.viewport.rect.height;

        if (contentHeight <= viewportHeight)
            return 1f;

        float elementTop = -element.anchoredPosition.y;
        float elementCenter = elementTop - element.rect.height / 2f;
        float normalizedPosition = elementCenter / (contentHeight - viewportHeight);
        return 1f - Mathf.Clamp01(normalizedPosition);
    }

    // технически умирают одновременно, но пусть будет
    private void OnDestroy()
    {
        if (_gameSceneManager != null)
            _gameSceneManager.OnObjectAdded -= CreateItem;

        if (_objectSelector != null)
            _objectSelector.OnSelectionChanged -= CameraSelectionChanged;
    }
}