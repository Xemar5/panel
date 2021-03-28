using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Highlightable : MonoBehaviour
{
    [SerializeField]
    private float startingValue = 0;
    [SerializeField]
    private float moveTowardsSpeed = 1;
    [SerializeField]
    private float lerpSpeed = 0;

    private float lastValue;

    public float CurrentValue { get; set; }
    public float TargetValue { get; set; }

    protected virtual void Awake()
    {
        CurrentValue = startingValue;
        TargetValue = startingValue;
        lastValue = startingValue;
    }

    protected virtual void Update()
    {
        CurrentValue = Mathf.MoveTowards(CurrentValue, TargetValue, moveTowardsSpeed * Time.deltaTime);
        CurrentValue = Mathf.Lerp(CurrentValue, TargetValue, lerpSpeed);
        if (CurrentValue != lastValue)
        {
            lastValue = CurrentValue;
            SetValue(CurrentValue);
        }
    }

    protected virtual void SetValue(float value) { }
}
