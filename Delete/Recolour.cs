using Godot;
using MagicalMountainMinery.Delete;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

public partial class Recolour : Node2D
{
	public Sprite2D sprite {  get; set; }
	string CopperScale1 = "190706,2f100d,471b14,60281a,773821,8c4926,a25d2d,b87333,cc8e44,d3a761,ddc185,e6d6a7,f1eacc,f9f6e8,ffffff";
	string scale2 = "71391f,401b07,cea58e,ffe5b7,ff9261,895e49,b75819,cd8647,964316";
	string scale3 = "2d190e,5c341e,eba687,cd8647,ffe2d9,b75819,964316,362619,725138";
	string scale4 = "2d190e,5c341e,eba687,cd8647,ffe2d9,b75819,964316,362619,725138,291400,291401,291500,4f2a00,4d2a00,633500,623500,4d2a02,4f2a02,281400,2b1400,4f2800,633400,4f2b00,633502,91291c,91291e,633503,633501,633700,90291e,93291e,361b00,361b01,efa44f,efe89a,93291c,91291f,613500,c86525,371b00,402100,efe898,92291e,eea44f,402300,341b00,432100,c86425,361b02,c86524,371a00,412100,291600,291402,422100,402102,eda44f,402103,402000,4f2802,613501,402101,4f2a01,4e2a00,4e2b00,91281e,412101,efe99a,361900,efe89b,4e2a01,361a00,402302,341b02,402301,2a1400,623501,912b1c,912b1e,93281e,633702,ede89a,efa44e,efe998,c86725,efa64f,efa44d,c86527,ca6525,291601,361a02,2b1401,291403";


	public Dictionary<Color, Color> ColorReplaceList = new Dictionary<Color, Color>();
	//public List<KeyValuePair<Color, Color>> ColorOriginList = new List<KeyValuePair<Color, Color>>();

	public int MidTone {  get; set; }
    public float Weight { get; set; }
    public float Breadth { get; set; }
    public Sprite2D SpriteReplace { get; set; }
    public Sprite2D SpriteOrigin { get; set; }

	public Map<Color, float> CopperList;
    public override void _Ready()
	{
        CopperList = MakeGrayscale(this.GetNode<Sprite2D>("ColorReplacer").Texture);
		CreateColorReplacer(CopperList, this.GetNode<GridContainer>("ColorReplacerGrid"));
        //CopperList = CopperList.OrderBy(item => item.Value).ToDictionary<Color, float>();

        SpriteOrigin = this.GetNode<Sprite2D>("Original");
        var original = MakeGrayscale(this.GetNode<Sprite2D>("Original").Texture);
        CreateColorReplacer(original, this.GetNode<GridContainer>("OriginalGrid"));

        SpriteReplace = this.GetNode<Sprite2D>("Target");
        var replace = MakeGrayscale(SpriteReplace.Texture);

        ColorReplaceList = CreateColorReplacer(original, this.GetNode<GridContainer>("TargetGrid"), true);

        var midtone = this.GetNode<HSlider>("HSlider3");
        midtone.MaxValue = CopperList.OrderT2.Count() - 1;
        midtone.MinValue = 0;
        midtone.Step = 1;
        midtone.Value = midtone.MaxValue / 2;
		MidTone = (int)midtone.Value;
        //OnWeightChange(0);
        OnMidToneChange(MidTone);



    }
	public void OnMidToneChange(float value)
    {
        MidTone = (int)value;
        var test = RecolorClosest(SpriteOrigin.Texture, CopperList);
        this.GetNode<Sprite2D>("Slider").Texture = ImageTexture.CreateFromImage(test);
        this.GetNode<Label>("Label3").Text = "Mid Tone: " + MidTone;
    }
    public void OnWeightChange(float value)
	{
        Weight = value;
        var test = RecolorClosest(SpriteOrigin.Texture, CopperList);
        this.GetNode<Sprite2D>("Slider").Texture = ImageTexture.CreateFromImage(test);
		this.GetNode<Label>("Label").Text = "Color Weight: " + value;
    }
	public void OnBreadthChange(float value)
	{
        Breadth = value;
        var test = RecolorClosest(SpriteOrigin.Texture, CopperList);
        this.GetNode<Sprite2D>("Slider").Texture = ImageTexture.CreateFromImage(test);
        this.GetNode<Label>("Label2").Text = "Color Breadth: " + value;
    }

    public Dictionary<Color, Color> CreateColorReplacer(Map<Color, float> colors, GridContainer grid, bool connect = false)
	{
        //var sorted = colors.OrderByDescending((item) => item.Value).ToList();
		var sortedDic = new Dictionary<Color, Color>();
        foreach (var entry in colors.OrderT2)
		{
			var col = new ColorTracker() 
			{ 
				Color = colors.Get(entry), 
				CustomMinimumSize = new Vector2(50, 20),
				OriginColor = colors.Get(entry),
            };
			var greyCol = new Color(entry, entry, entry);

            var grey = new ColorTracker() 
			{ 
				Color = greyCol, 
				CustomMinimumSize = new Vector2(50, 20),
				OriginColor = greyCol,
                TrackerPair = col,
            };

			grid.AddChild(col);
            grid.AddChild(grey);

            if (connect)
				grey.Connect(ColorTracker.SignalName.ColorPicked, new Callable(this, nameof(OnColorChange)));
        }

		return sortedDic;

    }


	public void OnColorChange(ColorTracker tracker)
	{
		//get the origin color pair
		var pair = tracker.TrackerPair;

		//change list to reflect the greyscale change
		ColorReplaceList[pair.Color] = tracker.Color;

		//now replace the image colors
        var image =   Recolor(SpriteOrigin.Texture, ColorReplaceList);
        SpriteReplace.Texture = ImageTexture.CreateFromImage(image);

    }

	/// <summary>
	/// Recolors the given image with the dictionary of colors. Each color key is replaced with the value 
	/// (if the keys match the image pixel).
	/// </summary>
	/// <param name="tex"></param>
	/// <param name="recolor"></param>
	/// <returns></returns>
    public Image Recolor(Texture2D tex, Dictionary<Color, Color> recolor)
	{
        var image = tex.GetImage();
        var size = tex.GetSize();
        for (int x = 0; x < size.X; x++)
		{
			for (int y = 0; y < size.Y; y++)
			{
                var pixel = image.GetPixel(x, y);
				if(recolor.TryGetValue(pixel, out var entry))
				{
                    image.SetPixel(x, y, entry);
                }
                    
            }
		}
		return image;
	}

    public Image RecolorClosest(Texture2D tex, Map<Color, float> recolor)
    {
        var image = tex.GetImage();
        var size = tex.GetSize();

        var start = Enumerable.ElementAt(recolor.OrderT2, 0);
        var mid = Enumerable.ElementAt(recolor.OrderT2, MidTone);
        var end = Enumerable.ElementAt(recolor.OrderT2, recolor.OrderT2.Count()-1);

        var negDict = recolor.OrderT2.GetViewBetween(start, mid);
        var posDict = recolor.OrderT2.GetViewBetween(mid, end);

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                var pixel = image.GetPixel(x, y);
                if (pixel.A > 0.3f)
				{
                    var avg = (pixel.R + pixel.G + pixel.B) / 3.0f;

					var b = avg < mid ? -1 : 1;
					var breadth  = Breadth * b;
					var final = Weight + breadth;
					
                    var closest = recolor.OrderT2.OrderBy(item => Math.Abs(avg - (item + final))).First();

                    image.SetPixel(x, y, recolor.Get(closest));
                }


            }
        }
        return image;
    }




    public Map<Color,float> MakeGrayscale(Texture2D tex)
    {
		var image = tex.GetImage();
		//var greySet = new HashSet<Color>();
		//var nonGreySet = new HashSet<Color>();
		var map = new Map<Color, float>();

        var data = image.GetData();
		var size = tex.GetSize();

		
		for (int x = 0; x < size.X; x++)
		{
			for (int y = 0; y < size.Y; y++)
			{
				var pixel = image.GetPixel(x, y);
				if(pixel.A ==1 && !map.Contains(pixel))
				{
                    
                    var avg = (pixel.R + pixel.G + pixel.B) / 3.0f;
					map.Add(pixel,avg);

                }


            }
		}
		return map;

    }

    public Dictionary<Color, Color> MakeGrayscale(string[] colors)
    {
        var dict = new Dictionary<Color, Color>();


		foreach (var color in colors)
		{
			var pixel = new Color(color);
            var avg = (pixel.R + pixel.G + pixel.B) / 3.0f;
            dict.Add(pixel, new Color(avg, avg, avg, pixel.A));
        }
        return dict;

    }


	public partial class ColorTracker : ColorPickerButton
	{
		[Signal]
		public delegate void ColorPickedEventHandler(ColorTracker tracker);
		public Color CurrentColor { get; set; }
		public Color OriginColor { get; set; }

		public ColorTracker TrackerPair { get; set; }
        public ColorTracker()
		{

		}

        public override void _Ready()
        {
			this.Connect(SignalName.ColorChanged, new Callable(this, nameof(OnChange)));
        }

		public void OnChange(Color color)
		{
			EmitSignal(SignalName.ColorPicked, this);
		}
    }
}
