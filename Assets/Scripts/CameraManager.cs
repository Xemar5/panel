using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private new Camera camera = default;
    [SerializeField]
    private Transform horizontalTransform = default;
    [SerializeField]
    private Transform verticalTransform = default;
    [SerializeField]
    private PhysicsRaycaster raycaster = default;
    [SerializeField]
    private Image cursor = default;
    [SerializeField]
    private float lerpDragDuration = 0.3f;

    private bool isCursorLocked = false;
    private float lerpRatio = 0;

    public Camera Camera => camera;
    public Transform HorizontalTransform => horizontalTransform;
    public Transform VerticalTransform => verticalTransform;
    public Transform Pivot => horizontalTransform;
    public PhysicsRaycaster Raycaster => raycaster;
    public bool IsCursorLocked => isCursorLocked;

    public static CameraManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log($"Another CameraManager exists.");
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void LateUpdate()
    {
        if (!isCursorLocked)
        {
            lerpRatio = 1;
            Vector3 localPosition = cursor.transform.localPosition;
            Vector3 mousePosition = Input.mousePosition;
            localPosition.x = mousePosition.x - Screen.width / 2;
            localPosition.y = mousePosition.y - Screen.height / 2;
            cursor.transform.localPosition = localPosition;
        }
        else
        {
            cursor.transform.localPosition = Vector3.Lerp(Vector3.zero, cursor.transform.localPosition, lerpRatio);
            lerpRatio = Mathf.MoveTowards(lerpRatio, 0, lerpDragDuration * Time.deltaTime);
        }
    }

    public void ChangeCursorState(bool isLocked)
    {
        /// Setting to the opposite value because we will change it again in <see cref="ChangeCursorState"/>.
        isCursorLocked = !isLocked;
        ToggleCursorState();
    }
    public void ToggleCursorState()
    {
        isCursorLocked = !isCursorLocked;
        if (isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
#if UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
#else
            Cursor.lockState = CursorLockMode.Confined;
#endif
            Cursor.visible = false;
        }
    }

}
