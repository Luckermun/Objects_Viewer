using UnityEngine;
using UnityEngine.EventSystems;


//  вообще, синемашину бы прикрутить
//  но тут решил не париться

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;

    [Header("Distance and zoom:")]
    [SerializeField] private float _distance = 5f;
    [SerializeField] private float _minDistance = 1f;
    [SerializeField] private float _maxDistance = 50f;
    [SerializeField] private float _zoomSpeed = 5f;

    [Header("Rotation:")]
    [SerializeField] private float _rotateSpeed = 5f;
    [SerializeField] private float _minVerticalAngle = -80f;
    [SerializeField] private float _maxVerticalAngle = 80f;
    private Vector2 _rot = new(0, 30);

    [Header("Panning:")]
    [SerializeField] private float _panSpeed = 5f;

    private void LateUpdate()
    {
        if (!_target) return;

        // блокировка ввода над ui
        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (!isOverUI)
            HandleInput();

        UpdatePos();
    }


    private void HandleInput()
    {
        _distance = Mathf.Clamp(_distance - Input.GetAxis("Mouse ScrollWheel") * _zoomSpeed, _minDistance, _maxDistance);

        if (Input.GetMouseButton(0))
        {
            _rot.x += Input.GetAxis("Mouse X") * _rotateSpeed;
            _rot.y = Mathf.Clamp(_rot.y - Input.GetAxis("Mouse Y") * _rotateSpeed, _minVerticalAngle, _maxVerticalAngle);

            _rot.x = (_rot.x + 360f) % 360f;
        }

        if (Input.GetMouseButton(1))
        {
            float panMultiplier = _panSpeed * _distance * 0.01f;

            Vector3 move = (transform.right * Input.GetAxis("Mouse X") + Vector3.up * Input.GetAxis("Mouse Y")) * panMultiplier;
            _target.position += move;
        }
    }

    private void UpdatePos()
    {
        transform.position = _target.position + Quaternion.Euler(_rot.y, _rot.x, 0) * new Vector3(0, 0, -_distance);
        transform.LookAt(_target);
    }

    public void FocusOn(Transform t)
    {
        _target = t;
        UpdatePos(); 
    }
    public Transform Target => _target;
}