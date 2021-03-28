using UnityEngine;

public class ColoredModifier : Modifier
{
    [SerializeField]
    private int colorIndex;

    public int ColorIndex => colorIndex;

    public override void Initialize(InteractablePiece owner)
    {
        Renderer[] renderers = owner.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (colorIndex < 0 || colorIndex >= owner.Master.Palette.Count)
            {
                renderer.material.color = Color.magenta;
            }
            else
            {
                renderer.material.color = owner.Master.Palette[colorIndex].color;
            }
        }

    }

}