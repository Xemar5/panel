using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OutlineHighlightable : Highlightable
{
    private const string ThicknessPropertyName = "_Thickness";

    [SerializeField]
    private new Renderer renderer = default;
    [SerializeField]
    private float topThickness = 0.1f;
    [SerializeField]
    private float pulseStrength = 0;
    [SerializeField]
    private float pulseFrequency = 1;

    private int thicknessPropertyId;

    protected override void Awake()
    {
        thicknessPropertyId = Shader.PropertyToID(ThicknessPropertyName);
    }
    protected override void Update()
    {
        base.Update();
        float pulse = Mathf.Sin(Time.time * pulseFrequency * Mathf.PI * 2) * pulseStrength * CurrentValue;
        renderer.material.SetFloat(thicknessPropertyId, CurrentValue * topThickness + pulse);
    }
}
