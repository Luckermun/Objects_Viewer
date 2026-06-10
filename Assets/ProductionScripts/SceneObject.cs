using System;
using UnityEngine;

public class SceneObject : MonoBehaviour, ISceneObject
{
    public event Action OnDestroying;

    private Renderer _ren;

    public Transform ObjectTransform => transform;

    private UIElement _assignedElement;

    private void Awake()
    {
        _ren = GetComponent<Renderer>();
        if (_ren == null)
        {
            enabled = false;
            return;
        }
    }

    public Renderer GetRenderer()
    {
        return _ren;
    }
    public void AssignElement(UIElement element)
    {
        _assignedElement = element;
    }

    private void OnDestroy()
    {
        OnDestroying.Invoke();
    }
}