using System;
using UnityEngine;

public interface ISceneObject
{
    public Renderer GetRenderer();
    Transform ObjectTransform { get; }

    event Action OnDestroying;
}