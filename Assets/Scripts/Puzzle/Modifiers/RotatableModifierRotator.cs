using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class RotatableModifierRotator : MonoBehaviour, IPointerDownHandler
{
    public event Action<RotatableModifierRotator> OnClicked;

    [SerializeField]
    private new Renderer renderer = default;

    public void Initialize(InteractablePiece owner, RotatableModifier rotatableModifier)
    {
        renderer.material.color = owner.Master.Palette[rotatableModifier.ColorIndex].color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }
}
