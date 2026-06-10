using System;
using System.Collections.Generic;   
using UnityEngine;
using UnityEngine.UI;

// отвечает за создание ui элементов, служит связующим между GameSceneManager и UIElement

// в идеале - сделать интерфейс IGameManager, чтобы подменять реализацию, например, в UIManager
//  но посчитал переусложнением для тестового
// также неплохо сделать глобального EventManager

// имеет место быть небольшое нарушение ответсвенности

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [SerializeField] private List<ISceneObject> _objects = new();

    [SerializeField] private GameObject[] _prefabObjectsPool;

    [SerializeField] private Vector3 _generationOffset = new Vector3(5,0,3);

    [SerializeField] private Button _myButton;

    private int _currentGeneration;

    public event Action<ISceneObject> OnObjectAdded; // передает созданный объект в событии
    public event Action<ISceneObject> OnObjectRemoved; // передает удаляемый объект в событии
    public event Action OnInitialization;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // метод первичной инициализации 
    public void OnAwakeButtonClicked()
    {
        for (int i = 0; i < _currentGeneration; i++)
        {
            GenerateObject();
        }
        FindAllSceneObj();
        OnInitialization.Invoke();
    }

    public void ObjectsToGenerate(int amount = 5)
    {
        _currentGeneration = amount;
    }

    // регистрация + вызов события
    private void Register(ISceneObject obj)
    {
        if (!_objects.Contains(obj))
        {
            _objects.Add(obj);
            OnObjectAdded?.Invoke(obj);
        }
    }

    public void RemoveObject(ISceneObject obj)
    {
        if (_objects.Contains(obj))
        {
            OnObjectRemoved?.Invoke(obj);
            _objects.Remove(obj);

            if (obj is MonoBehaviour mb) Destroy(mb.gameObject);
        }
    }

    // для кнопки в ui
    public void AddObject()
    {
        GenerateObject();
    }

    // объект генерируется в офсете перед камерой и регистрируется
    public ISceneObject GenerateObject()
    {
        int indexToGenerate = UnityEngine.Random.Range(0, _prefabObjectsPool.Length);

        Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-_generationOffset.x, _generationOffset.x), 
            0,
            UnityEngine.Random.Range(-_generationOffset.z, _generationOffset.z));

        SceneObject obj = Instantiate(_prefabObjectsPool[indexToGenerate]).GetComponent<SceneObject>();

        if (obj == null)
        {
            Debug.LogError("No SceneObject component on generated prefab,");
            return null;
        }

        obj.transform.position = FindFirstObjectByType<Camera>().gameObject.transform.position + 
            randomOffset;

        Register(obj);

        Debug.Log("Generation succesfull.");

        return obj;
    }

    // вызывается только при первичной инициализации - возрастает стоимость вызова 
    //  в зависимости от количесва объектов
    private void FindAllSceneObj()
    {
        foreach (var obj in FindObjectsOfType<SceneObject>()) Register(obj);

        Debug.Log($"Objects captured: ({_objects.Count}).");
    }

    // ридонли список всех объектов
    public IReadOnlyList<ISceneObject> GetAll() => _objects.AsReadOnly();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}