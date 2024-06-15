using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Data.Load;
using MagicalMountainMinery.Main;
using static MagicalMountainMinery.Data.Load.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using static MagicalMountainMinery.Main.GameController;
using Label = Godot.Label;
using MagicalMountainMinery.Obj;

public partial class Runner : Node2D, IMain
{
    public MapLevel MapLevel { get; set; }

    
    //public Cart Cart;

    public TrackPlacer Placer;

    public NavBar NavBar { get; set; }

    public List<CartController> CartControllers { get; set; } = new List<CartController>();

    public bool ValidRun = true;

    public static GameEvent LastEvent { get; set; }

    public Shop Shop { get; set; }  
    public LevelCompleteUI LevelEndUI { get; set; }

    private bool pauseHandle = false;
    public bool PauseHandle
    {
        get => pauseHandle;
        set
        {
            //if(Placer!= null) 
                //Placer.PauseHandle = value;
            foreach (var controller in CartControllers)
                controller.PauseHandle = value;
            pauseHandle = value;

        }
    }

    public LoadHomeDelegate HomeCall { get; set; }
    public LevelCompleteDelegate LevelComplete { get; set; }

    public LoadLevelDelegate LoadLevel { get; set; }

    public delegate void GemUsedDelegate(GameResource resource);

    public MapSave CurrentMapSave { get; set; }
    public MapLoad CurrentMapData { get; set; }

    public TopBar TopBar { get; set; }
    public Camera Cam { get; set; }

    public override void _Ready()
    {

        Cam = new Camera();
        this.AddChild(Cam);
        //Cam.Zoom = new Vector2(0.1f, 0.1f);

        Placer = this.GetNode<TrackPlacer>("TrackPlacer");

        GemUsedDelegate used = GemUsed;
        Placer.GemUsed = used;

    }




    public void OnReset()
    {
        PreReset();
        LoadMapLevel(CurrentMapData);
    }

    public void OnRetry()
    {
        PreReset();
        LoadMapLevel(CurrentMapData, MapLevel);

    }

    public void _on_next_pressed()
    {
        PreReset();
        var next = ResourceStore.GetNextLevel(CurrentMapData);
        if (next != null)
            LoadLevel(next);

        

    }

    public void on_home_pressed()
    {
        PreReset();
        HomeCall();
    }

    private void PreReset()
    {
        ReloadUsedGems();
        if (Placer != null)
            Placer.PauseHandle = false;
        if (LevelEndUI != null)
            LevelEndUI.Visible = false;
    }

    public void StopLevelPressed()
    {
        this.GetNode<Control>("CanvasLayer/Container").Visible = false;
        EventDispatch.ClearAll();
        OnRetry();
    }

    public override void _ExitTree()
    {
        PreReset();
        base._ExitTree();
    }
    public void _on_map_pressed()
    {
        PreReset();
        HomeCall();
    }


    public void BtnEnable(BaseButton b, bool enable)
    {
        b.Modulate = enable ? Colors.White : new Color(1, 1, 1, 0.5f);
        b.Disabled = !enable;
    }

    public void PlayLevelPressed()
    {
        //BtnEnable(this.GetNode<TextureButton>("CanvasLayer/Container/Play_Pause"), true);
        //BtnEnable(this.GetNode<TextureButton>("CanvasLayer/Container/Play"), false);

        var thing = CartControllers[0].State == CartController.CartState.Moving ? CartController.CartState.Paused : CartController.CartState.Moving;
        foreach (var item in CartControllers)
        {
            item.State = thing;
        }


    }

    public void OnSimSpeedChange(float amount)
    {
        if (RunningVars.SIM_SPEED_STACK + amount < -2)
            return;
        if (RunningVars.SIM_SPEED_STACK + amount > 4)
            return;
        RunningVars.SIM_SPEED_STACK += amount;


        if (RunningVars.SIM_SPEED_STACK < 0)
        {
            var percent = RunningVars.SIM_SPEED_STACK * -2;
            RunningVars.SIM_SPEED_RATIO = percent == 0 ? 1 : 1 / percent;

        }
        else
        {
            RunningVars.SIM_SPEED_RATIO = 1 + RunningVars.SIM_SPEED_STACK;
        }
        NavBar.SpeedControl.GetNode<Label>("VBoxContainer/Label").Text = (RunningVars.SIM_SPEED_RATIO * 100) + "%";
    }




    public void ValidLevelComplete()
    {

        //everytime we complete a level, make sure to remove all gems used on that level for good
        Placer.GemCopy = new List<GameResource>();



        //gather any bonus stars completed
        var sum = 0;
        foreach (var target in MapLevel.LevelTargets)
        {
            sum += target.BonusConsCompleted;
        }
        bool complete = false;
        //gather any gems and add to profile
        var gemstotal = new List<Mineable>();
        foreach (var control in CartControllers)
        {
            var gems = control.GatheredNodes.Where(node => ResourceStore.ShopResources.Any(item => item == node.ResourceSpawn)).ToList();
            gemstotal.AddRange(gems);
            foreach (var item in gems)
            {
                item.ResourceSpawn.Amount = 1;
                AddGemToProfile(item.ResourceSpawn);
            }
            //var GEMS = control.Cart.StoredResources.Where(res => ResourceStore.ShopResources.Any(item => item.ResourceType == res.Key)).ToList();

            //foreach (var item in GEMS)
            //{
            //    AddGemToProfile(item.Value.GameResource);
            //}
            control.Cart.ClearResources();
        }

        var indexes = gemstotal.Select(gem => gem.Index).ToList();
        int bonusStarDiff = sum;
        int totalStars = 0;


        //here we check if the player has alreaddy completed this level, but also
        //check if they have completed more bonus stars
        if (CurrentMapSave != null)
        {
            bonusStarDiff = sum - CurrentMapSave.BonusStarsCompleted;
            if (bonusStarDiff > 0)
            {
                CurrentMapSave.BonusStarsCompleted = bonusStarDiff;
            }
            else
                bonusStarDiff = 0;
            //CurrentMapSave.BonusStarsCompleted = Math.Max(CurrentMapSave.BonusStarsCompleted, sum);
            complete = true;


            //if we have already completed the level, then just add any extra collected
            if (CurrentMapSave.GemsCollected != null)
                CurrentMapSave.GemsCollected.AddRange(indexes);
            else
                CurrentMapSave.GemsCollected = indexes;

        }
        else
        {
            CurrentMapSave = new MapSave()
            {
                BonusStarsCompleted = sum,
                Completed = true,
                LevelIndex = -100,
                //if new level complete, just add the things here
                GemsCollected = indexes

            };
            totalStars += CurrentMapData.Difficulty;
        }

        CurrentProfile.StarCount += totalStars + bonusStarDiff;
        NavBar.StarLabel.Text = CurrentProfile.StarCount.ToString();
        LevelComplete(CurrentMapSave, CurrentMapData);

        LevelEndUI.Show(complete, CurrentMapSave.BonusStarsCompleted);
    }

    public void HandleShopEntry(ShopEntry entry)
    {
        //cant afford poor boi
        if (CurrentProfile.StarCount < entry.GameResource.Amount)
        {
            Shop.PlayString(false);
            return;
        }

        if (entry.GameResource.ResourceType == ResourceType.Track)
        {
            MapLevel.AllowedTracks++;
            Placer.UpdateUI();
        }
        else
        {
            AddGemToProfile(entry.GameResource);
        }

        Shop.PlayString(true);
        CurrentProfile.StarCount -= entry.GameResource.Amount;
        NavBar.StarLabel.Text = CurrentProfile.StarCount.ToString();


    }

    public void AddGemToProfile(GameResource resource)
    {
        var existing = CurrentProfile.StoredGems.FirstOrDefault(item => item == resource);
        if (existing != null)
        {
            if (existing.Amount > 0)
            {
                var noddy = this.GetNode<ResIcon>("CanvasLayer/GemBox/" + existing.ResourceType.ToString());
                (noddy as ResIcon).UpdateAmount(existing.Amount + 1);
                return;
            }
            else
            {
                CurrentProfile.StoredGems.Remove(existing);
            }
        }
        var btn = LoadScene<ResIcon>("res://UI/GemIcon.tscn");
        var dex = ResourceStore.ShopResources.IndexOf(resource);
        var copy = new GameResource(ResourceStore.ShopResources[dex]);
        copy.Amount = 1;
        CurrentProfile.StoredGems.Add(copy);
        btn.AddResource(copy);
        this.GetNode<VBoxContainer>("CanvasLayer/GemBox").AddChild(btn);

    }

    public void RemoveGemFromProfile(GameResource resource)
    {
        var existing = CurrentProfile.StoredGems.FirstOrDefault(item => item == resource);
        if (existing != null)
        {
            var amount = existing.Amount - 1;
            var noddy = this.GetNode<ResIcon>("CanvasLayer/GemBox/" + existing.ResourceType.ToString());
            existing.Amount = amount;
            (noddy as ResIcon).UpdateAmount(amount);
            if (amount <= 0)
            {
                noddy.QueueFree();
                CurrentProfile.StoredGems.Remove(resource);
            }



        }
    }

    public void GemUsed(GameResource resource)
    {
        RemoveGemFromProfile(resource);
    }

  
    public bool HandleEvent(EventType env, IUIComponent obj)
    {
        var list = new List<EventType>() { EventType.Speed_Increase, EventType.Speed_Decrease,
            EventType.Start_Mining,EventType.Toggle_Shop, EventType.Reset_Level,EventType.Stop_Mining,EventType.Pause};

        if (env is EventType.Start_Mining && !Placer.PauseHandle)
            _on_start_pressed();
        else if (env is EventType.Stop_Mining)
            StopLevelPressed();
        else if (env is EventType.Pause)
            PlayLevelPressed();
        else if (env is EventType.Home)
            on_home_pressed();
        else if (env is EventType.Speed_Increase)
            OnSimSpeedChange(1f);
        else if (env is EventType.Speed_Decrease)
            OnSimSpeedChange(-1f);
        else if (env is EventType.Reset_Level)
            OnReset();
        else if (env is EventType.Toggle_Shop)
        {
            HandleShopVisible(env, obj);
        }

        return list.Contains(env);
    }


    public override void _PhysicsProcess(double delta)
    {

        var obj = EventDispatch.PeekHover();
        var env = EventDispatch.FetchLastInput();

        if (!ValidRun)
            return;
        if (PauseHandle)
        {
            if(Shop.Visible)
                HandleShopVisible(env, obj);
            return;
        } 
        if (Placer.HandleSpecial || !HandleEvent(env, obj))
        {
            Placer?.Handle(env, obj);
        }

        //var fin = CartControllers.Count > 0 && CartControllers.All(item => item.State == CartController.CartState.Stopped && item.Cart.StoredResources.Count ==0);
        var success = MapLevel.LevelTargets.All(item => item.CompletedAll);
        //if (MapLevel.LevelTargets?.Count > 1)
        //    success = MapLevel.LevelTargets.All(item => item.CompletedAll);
        //else if(MapLevel.LevelTargets?.Count == 1)
        //    success = MapLevel.LevelTargets.All(item => item.CompletedAll) && fin;
        //success = success && fin ;

        if (success)
        {
            foreach (var control in CartControllers)
                control.Finished = true;

            ValidLevelComplete();
            EventDispatch.PushEventFlag(GameEventType.LevelComplete);
            ValidRun = false;
        }

    }

    /// <summary>
    /// Toggles shop visibility and pauses the current handling of Runner.
    /// Will reject all non Shop Toggle actions if shop is visible.
    /// </summary>
    /// <param name="env"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public void HandleShopVisible(EventType env, IUIComponent obj)
    {
        if (Shop.Visible)
        {
            if (!string.IsNullOrEmpty(obj?.UIID) && obj.UIID.Contains("ShopEntry") && env == EventType.Left_Release)
            {
                HandleShopEntry(obj as ShopEntry);
                return;
            }
            var entry = obj is not null && obj.UIID.Contains("ShopEntry") && env == EventType.Left_Release;
            var shop = env == EventType.Toggle_Shop;
            if (!(entry || shop))
                return;
           
        }
        PauseHandle = Shop.Visible = !Shop.Visible;
        EventDispatch.ClearAll();

    }


    public string GetOrientationString(IndexPos pos)
    {
        if (pos == IndexPos.Up) return "North";
        else if (pos == IndexPos.Down) return "South";
        else if (pos == IndexPos.Left) return "West";
        else return "East";
    }

    public void Free(Node node)
    {
        if (IsInstanceValid(node))
            node.QueueFree();
    }



    public void _on_start_pressed()
    {
        if (Placer.MasterTrackList.Count == 0)
            return;

        Placer.PauseHandle = true;
        //if(CartControllers.Any(item=>item.State == ))
        this.GetNode<Control>("CanvasLayer/Container").Visible = true;
        BtnEnable(NavBar.MiningIcon, false);
        EventDispatch.ExitUI(NavBar.MiningIcon as IUIComponent);
        EventDispatch.PushEventFlag(GameEventType.MiningStart);
        //BtnEnable(StartButton, false);

        var colors = new List<Color>() { Colors.AliceBlue, Colors.RebeccaPurple, Colors.Yellow, Colors.Green };
        int i = 0;

        foreach (var control in CartControllers)
        {
            control.Start(colors[i++], Placer.MasterTrackList, control.StartT.Direction1, control.StartT);
        }


    }


}



