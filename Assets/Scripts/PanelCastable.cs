using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PanelCastable : MonoBehaviour, ICastable
{
    private PanelStand stand;

    public Transform SnapPoint => stand.SnapPoint;
    public Transform LookDirection => stand.LookDirection;

    public void Initialize(PanelStand stand)
    {
        this.stand = stand;
    }

    public void OnCastingEnded(PanelCaster caster)
    {
        stand.OnCastEnded(caster);
    }

    public void OnCastingStarted(PanelCaster caster, float distance)
    {
        stand.OnCastStarted(caster, distance);
    }

    public bool Use(PanelCaster caster)
    {
        if (stand.IsCasterActive(caster) && caster.Character.MovementPreventors.Add(this))
        {
            Debug.Log($"Using castable.");
            stand.ShowVisuals(caster, false);
            caster.OnBeginUsing(this);
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool Unuse(PanelCaster caster)
    {
        if (caster.Character.MovementPreventors.Remove(this))
        {
            Debug.Log($"Stopped using castable.");
            caster.OnEndUsing(this);
            stand.ShowVisuals(caster, true);
            return true;
        }
        else
        {
            return false;
        }
    }
}
