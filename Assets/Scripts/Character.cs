using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Position")]
    [SerializeField]
    private new Rigidbody rigidbody = default;
    [SerializeField]
    private float acceleration = 3;
    [SerializeField]
    private float speed = 3;
    [SerializeField]
    private float breaking = 0.93f;

    [Header("Camera")]
    [SerializeField]
    private Transform cameraPivot = default;
    [SerializeField]
    private float cameraHorizontalSpeed = 3;
    [SerializeField]
    private float cameraVerticalSpeed = 2;
    [SerializeField]
    private float lowestAngle = -90;
    [SerializeField]
    private float highestAngle = 90;

    private Vector3 direction;
    private Vector3 velocity;

    public CameraManager CameraManager => CameraManager.Instance;
    public Rigidbody Rigidbody => rigidbody;
    public HashSet<object> MovementPreventors { get; private set; } = new HashSet<object>();
    public Vector3 LookDirection
    {
        get
        {
            return new Vector3(CameraManager.Instance.VerticalTransform.eulerAngles.x, CameraManager.Instance.HorizontalTransform.eulerAngles.y, 0);
        }
        set
        {
            Vector3 vertical = CameraManager.Instance.VerticalTransform.eulerAngles;
            vertical.x = value.x;
            CameraManager.Instance.VerticalTransform.eulerAngles = vertical;

            Vector3 horizontal = CameraManager.Instance.HorizontalTransform.eulerAngles;
            horizontal.y = value.y;
            CameraManager.Instance.HorizontalTransform.eulerAngles = horizontal;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            CameraManager.ToggleCursorState();

        direction =
            Input.GetAxis("Horizontal") * Vector3.right +
            Input.GetAxis("Vertical") * Vector3.forward;

        Vector2 delta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y"));

        if (CameraManager.IsCursorLocked == true && MovementPreventors.Count == 0)
        {
            Vector3 lookDirection = LookDirection;
            if (lookDirection.x < -180)
                lookDirection.x += 360;
            if (lookDirection.x > 180)
                lookDirection.x -= 360;
            lookDirection.x -= delta.y * cameraVerticalSpeed;
            if (lookDirection.x < lowestAngle)
                lookDirection.x = lowestAngle;
            if (lookDirection.x > highestAngle)
                lookDirection.x = highestAngle;
            lookDirection.y += delta.x * cameraHorizontalSpeed;
            LookDirection = lookDirection;
        }
    }

    private void FixedUpdate()
    {
        if (MovementPreventors.Count == 0)
        {
            velocity = Vector3.MoveTowards(velocity, direction.normalized * Mathf.Clamp(direction.magnitude, 0, 1) * speed / breaking, acceleration * Time.fixedDeltaTime);
            rigidbody.position += CameraManager.Instance.HorizontalTransform.rotation * velocity;
            velocity *= breaking;
        }
    }
    private void LateUpdate()
    {
        CameraManager.Instance.Pivot.position = cameraPivot.position;
    }

}
