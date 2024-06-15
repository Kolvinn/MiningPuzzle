using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Main;
using System;
using System.Collections.Generic;

public partial class Shop : GuiOverride
{

    public List<ShopEntry> Items { get; set; } = new List<ShopEntry>();

    public static AudioStreamPlayer player { get; set; }

    public RichTextLabel CatLabel { get; set; }

    public Random stringRand { get; set; }
    public List<string> EntryStrings { get; set; } = new List<string>()
    {
        "What can I get for you?",
        "Something here that piques your interest?",
        "Y'know, I lost a ball of yarn made of pure gold once. I wonder where it went...",
        "If you can bring me bat wings, I have something special I can give you."
    };

    public List<string> PurchaseStrings { get; set; } = new List<string>()
    {
        "An excellent choice.",
        "Good choice.",
        "Thank you, come again.",
        "Aren't I benevolent"
    };

    public List<string> FailStrings { get; set; } = new List<string>()
    {
        "Maybe you should try again with some stars.",
        "Have you tried being good at the game?",
        "You don't have enough stars to purchase this.",
        "Try passing some levels first."
    };

    public override void _Ready()
    {
        stringRand = new Random();
        player = new AudioStreamPlayer()
        {
            Bus = "Sfx",
            Stream = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/UI/Minimalist3.wav"),
            Autoplay = false
        };
        this.AddChild(player);
        this.CatLabel = this.GetNode<RichTextLabel>("Label");
        this.Connect(SignalName.VisibilityChanged, Callable.From(OnVisChange));
    }

    public void OnVisChange()
    {
        if(this.Visible)
        {
            CatLabel.Text = EntryStrings[stringRand.Next(0, EntryStrings.Count)];
        }
    }
    public void AddGameResource(GameResource resource)
    {
        var entry = Runner.LoadScene<ShopEntry>("res://UI/ShopEntry.tscn");
        Items.Add(entry);
        var box = this.GetNode<VBoxContainer>("PanelContainer/MarginContainer/ScrollContainer/VBoxContainer");
        box.AddChild(entry);
        entry.AddResource(resource);

    }

    public void PlayString(bool success)
    {
        if(success)
            CatLabel.Text = PurchaseStrings[stringRand.Next(0, EntryStrings.Count)];
        else
            CatLabel.Text = FailStrings[stringRand.Next(0, EntryStrings.Count)];
    }
    public static void Play()
    {
        if (player.Playing)
            player.Stop();
        player.Play();
    }
}
