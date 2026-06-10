using UnityEngine;
using UnityEngine.EventSystems;
using System;

// фокус камеры
// синхронизация состояний выбора ui и объекта

// комментов минимум, скрипт простой

public class ObjectSelector : MonoBehaviour
{
    public static ObjectSelector Instance { get; private set; }

    [SerializeField] private Camera _camera;
    [SerializeField] private CameraController _cameraController;

    private ISceneObject _selectedObject;
    public ISceneObject SelectedObject => _selectedObject;

    public event Action<ISceneObject> OnSelectionChanged;

    private void Awake()
    {
        Instance = this;
        if (_camera == null) _camera = Camera.main;
    }

    private void Start()
    {
        GameSceneManager.Instance.OnObjectAdded += SelectObject;
        GameSceneManager.Instance.OnObjectRemoved += HandleRemovedObject;
        GameSceneManager.Instance.OnInitialization += SelectRandomObject;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var sceneObj = hit.collider.GetComponentInParent<SceneObject>();
                SelectObject(sceneObj);
            }
        }
    }

    private void HandleRemovedObject(ISceneObject obj)
    {
        if (_selectedObject == obj)
        {
            SelectRandomObject();
        }
    }

    private void SelectRandomObject()
    {
        var allObjects = GameSceneManager.Instance.GetAll();
        if (allObjects.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, allObjects.Count);
            SelectObject(allObjects[randomIndex]);
        }
        else SelectObject(null);
    } 

    public void SelectObject(ISceneObject obj)
    {
        if (_selectedObject == obj) return;

        _selectedObject = obj;
        OnSelectionChanged?.Invoke(obj);

        if (obj != null && _cameraController != null)
        {
            _cameraController.FocusOn(obj.ObjectTransform);
        }
    }

    private void OnDestroy()
    {
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnObjectAdded -= SelectObject;
            GameSceneManager.Instance.OnObjectRemoved -= HandleRemovedObject;
            GameSceneManager.Instance.OnInitialization -= SelectRandomObject;
        }
    }
}