using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface ICastable
{
    GameObject gameObject { get; }
    Transform transform { get; }

    Transform SnapPoint { get; }
    Transform LookDirection { get; }

    void OnCastingStarted(PanelCaster caster, float distance);
    void OnCastingEnded(PanelCaster caster);

    bool Use(PanelCaster caster);
    bool Unuse(PanelCaster caster);
}
