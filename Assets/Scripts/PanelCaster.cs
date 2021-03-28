using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PanelCaster : MonoBehaviour
{
    private const int RaycastPhysicsLayer = 0;
    private const float LerpRatio = 0.05f;

    [SerializeField]
    private Character character = default;

    private List<ICastable> castablesInRange = new List<ICastable>();
    private ICastable currentTarget = null;
    private ICastable usedCastable = null;

    public Character Character => character;

    private void Update()
    {
        RaycastCastables();
        HandleInput();
        HandleSnapping();
    }

    private void HandleSnapping()
    {
        if (usedCastable != null)
        {
            Character.Rigidbody.position = Vector3.Lerp(Character.Rigidbody.position, usedCastable.SnapPoint.position, LerpRatio);
            Character.LookDirection = Quaternion.Lerp(Quaternion.Euler(Character.LookDirection), usedCastable.LookDirection.rotation, LerpRatio).eulerAngles;
        }
    }

    private void HandleInput()
    {
        if (usedCastable == null && currentTarget != null && Input.GetKeyDown(KeyCode.Mouse0) && currentTarget.Use(this))
        {
            usedCastable = currentTarget;
        }
        if (usedCastable != null && Input.GetKeyDown(KeyCode.Mouse1) && usedCastable.Unuse(this))
        {
            usedCastable = null;
        }
    }

    private void RaycastCastables()
    {
        Ray ray = Character.CameraManager.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, float.PositiveInfinity, 1 << RaycastPhysicsLayer, QueryTriggerInteraction.Ignore) == true)
        {
            ICastable castable = hit.collider.GetComponent<ICastable>();
            if (castable != currentTarget && currentTarget != null)
            {
                currentTarget.OnCastingEnded(this);
            }
            currentTarget = castable;
            if (currentTarget != null)
            {
                currentTarget.OnCastingStarted(this, hit.distance);
            }
        }
        else if (currentTarget != null)
        {
            currentTarget.OnCastingEnded(this);
            currentTarget = null;
        }
    }

    public void OnBeginUsing(ICastable castable)
    {
        Character.CameraManager.Raycaster.enabled = true;
        Character.CameraManager.ChangeCursorState(false);
    }

    public void OnEndUsing(ICastable castable)
    {
        Character.CameraManager.Raycaster.enabled = false;
        Character.CameraManager.ChangeCursorState(true);
    }

    private void OnDrawGizmos()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Gizmos.DrawRay(ray);
    }
}
