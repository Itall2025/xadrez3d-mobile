using UnityEngine;

namespace Xadrez3D.Gameplay
{
    public sealed class OrbitCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 11f;
        [SerializeField] private float minDistance = 7f;
        [SerializeField] private float maxDistance = 16f;
        [SerializeField] private float yaw = 45f;
        [SerializeField] private float pitch = 45f;
        [SerializeField] private float minPitch = 18f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private float rotateSpeed = 0.22f;
        [SerializeField] private float pinchSpeed = 0.01f;
        [SerializeField] private float mouseZoomSpeed = 2.8f;

        private Vector2 _lastPointerPosition;
        private bool _dragging;
        private float _lastPinchDistance;

        private void LateUpdate()
        {
            HandleRotationInput();
            HandleZoomInput();
            ClampValues();
            ApplyTransform();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void HandleRotationInput()
        {
            if (Input.touchCount == 1)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    _lastPointerPosition = touch.position;
                    _dragging = true;
                }
                else if (touch.phase == TouchPhase.Moved && _dragging)
                {
                    var delta = touch.position - _lastPointerPosition;
                    yaw += delta.x * rotateSpeed;
                    pitch -= delta.y * rotateSpeed;
                    _lastPointerPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    _dragging = false;
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                _lastPointerPosition = Input.mousePosition;
                _dragging = true;
            }
            else if (Input.GetMouseButton(1) && _dragging)
            {
                var current = (Vector2)Input.mousePosition;
                var delta = current - _lastPointerPosition;
                yaw += delta.x * rotateSpeed;
                pitch -= delta.y * rotateSpeed;
                _lastPointerPosition = current;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                _dragging = false;
            }
        }

        private void HandleZoomInput()
        {
            if (Input.touchCount == 2)
            {
                var t0 = Input.GetTouch(0);
                var t1 = Input.GetTouch(1);
                var currentDistance = Vector2.Distance(t0.position, t1.position);

                if (_lastPinchDistance > 0f)
                {
                    var delta = currentDistance - _lastPinchDistance;
                    distance -= delta * pinchSpeed;
                }

                _lastPinchDistance = currentDistance;
            }
            else
            {
                _lastPinchDistance = 0f;
                var wheel = Input.mouseScrollDelta.y;
                if (Mathf.Abs(wheel) > 0.001f)
                {
                    distance -= wheel * mouseZoomSpeed;
                }
            }
        }

        private void ClampValues()
        {
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private void ApplyTransform()
        {
            if (target == null)
            {
                return;
            }

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var offset = rotation * new Vector3(0f, 0f, -distance);
            transform.position = target.position + offset;
            transform.rotation = rotation;
        }
    }
}
