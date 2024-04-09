using Godot;
using MagicalMountainMinery.Main;
using System;

public partial class GameButton : TextureButton, IUIComponent
{

    [Export]
    public string UIID { get; set; }
    public ShaderMaterial selectMat {  get; set; }
	public override void _Ready()
	{
		if(this.Material != null)
		{
			this.Material.ResourceLocalToScene = true;
            selectMat = this.Material as ShaderMaterial;

            selectMat.ResourceLocalToScene = true;
            selectMat.SetShaderParameter("width", 0);
			
			//this.Material.set

        }

		this.Connect(SignalName.MouseEntered, Callable.From(OnEnter));
        this.Connect(SignalName.MouseExited, Callable.From(OnExit));
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnEnter()
	{
		if(!this.Disabled)
			EventDispatch.HoverUI(this);
	}
    public void OnExit()
    {
        if (!this.Disabled)
            EventDispatch.ExitUI(this);
    }
}
