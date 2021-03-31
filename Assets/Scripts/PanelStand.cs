using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PanelStand : MonoBehaviour
{
    private const int SnappingLayer = 1 << 12;

    [SerializeField]
    private Transform snapPoint = default;
    [SerializeField]
    private Transform lookDirection = default;
    [SerializeField]
    private float maxDistance = default;
    [SerializeField]
    private PanelCastable[] castables = default;
    [SerializeField]
    private Highlightable[] highlightables = default;

    [Space]
    [SerializeField]
    private GameObject completionLight = default;
    [SerializeField]
    private Puzzle puzzle = default;

    private HashSet<PanelCaster> castersInZone = new HashSet<PanelCaster>();
    private HashSet<PanelCaster> castersLooking = new HashSet<PanelCaster>();
    private HashSet<PanelCaster> castersActive = new HashSet<PanelCaster>();

    public Transform SnapPoint => snapPoint;
    public Transform LookDirection => lookDirection;

    private void Awake()
    {
        foreach (Highlightable highlightable in highlightables)
        {
            highlightable.TargetValue = 0;
            highlightable.CurrentValue = 0;
        } 
        foreach (PanelCastable castable in castables)
        {
            castable.Initialize(this);
        }
        puzzle.OnPuzzleCompleted += (x, i) => completionLight.SetActive(true);
        puzzle.OnPuzzleBroken += (x) => completionLight.SetActive(false);
        completionLight.SetActive(false);
    }

    public bool IsCasterActive(PanelCaster caster) => castersActive.Contains(caster);

    public void OnCastStarted(PanelCaster caster, float distance)
    {
        UpdateCasterLooking(caster, distance < maxDistance);
    }

    public void OnCastEnded(PanelCaster caster)
    {
        UpdateCasterLooking(caster, false);
    }

    private void UpdateCasterLooking(PanelCaster caster, bool add)
    {
        if (add && castersLooking.Add(caster) && castersInZone.Contains(caster))
        {
            UpdateCasterActive(caster, true);
        }
        else if (!add && castersLooking.Remove(caster))
        {
            UpdateCasterActive(caster, false);
        }
    }

    private void UpdateCasterActive(PanelCaster caster, bool isActive)
    {
        if (isActive && castersActive.Add(caster))
        {
            Debug.Log("Cast started");
            ShowVisuals(caster, true);
        }
        else if (!isActive && castersActive.Remove(caster))
        {
            Debug.Log("Cast ended");
            ShowVisuals(caster, false);
        }
    }

    public void ShowVisuals(PanelCaster caster, bool show)
    {
        if (show && castersActive.Contains(caster))
        {
            foreach (Highlightable highlightable in highlightables)
            {
                highlightable.TargetValue = 1;
            }
        }
        else
        {
            foreach (Highlightable highlightable in highlightables)
            {
                highlightable.TargetValue = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null)
        {
            return;
        }
        PanelCaster caster = other.attachedRigidbody.GetComponent<PanelCaster>();
        if (caster != null && castersInZone.Add(caster) && castersLooking.Contains(caster))
        {
            UpdateCasterActive(caster, true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == null)
        {
            return;
        }
        PanelCaster caster = other.attachedRigidbody.GetComponent<PanelCaster>();
        if (caster != null && castersInZone.Remove(caster))
        {
            UpdateCasterActive(caster, false);
        }
    }
}
