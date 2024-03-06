using System;
using UnityEngine;

/// <summary> Draw enums correctly within nodes. Without it, enums show up at the wrong positions. </summary>
/// <remarks> Enums with this attribute are not detected by EditorGui.ChangeCheck due to waiting before executing </remarks>
[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum )]
public class DefaultNoodleColorAttribute : Attribute
{
    public Color Color { get; private set; }
    public Color SelectedColor { get; private set; }

    public DefaultNoodleColorAttribute( float colorR, float colorG, float colorB )
    {
        SelectedColor = new Color( colorR, colorG, colorB );
        Color = new Color( SelectedColor.r * 0.6f, SelectedColor.g * 0.6f, SelectedColor.b * 0.6f );
    }

    public DefaultNoodleColorAttribute( byte colorR, byte colorG, byte colorB )
    {
        SelectedColor = new Color32( colorR, colorG, colorB, 255 );
        Color = new Color( SelectedColor.r * 0.6f, SelectedColor.g * 0.6f, SelectedColor.b * 0.6f );
    }

    public DefaultNoodleColorAttribute( float colorR, float colorG, float colorB, float selectedColorR, float selectedColorG, float selectedColorB )
    {
        SelectedColor = new Color( selectedColorR, selectedColorG, selectedColorB );
        Color = new Color( colorR, colorG, colorB );
    }

    public DefaultNoodleColorAttribute( byte colorR, byte colorG, byte colorB, byte selectedColorR, byte selectedColorG, byte selectedColorB )
    {
        SelectedColor = new Color32( selectedColorR, selectedColorG, selectedColorB, 255 );
        Color = new Color32( colorR, colorG, colorB, 255 );
    }
}