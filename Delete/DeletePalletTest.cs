using Godot;
using MagicalMountainMinery.Data;

public partial class DeletePalletTest : Sprite2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ResourceStore.LoadPallet();
        var mat = Material as ShaderMaterial;

        mat.SetShaderParameter("colorpallet", ResourceStore.ColorPallet.ToArray());
        var arr = mat.GetShaderParameter("colorpallet");
        var tex = GetViewport().GetTexture();
        //this.GetNode<TextureRect>("TextureRect").Texture = tex;

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
