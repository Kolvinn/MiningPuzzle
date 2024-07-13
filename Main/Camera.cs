using Godot;
using MagicalMountainMinery.Design;
using System;
using System.Collections.Generic;

namespace MagicalMountainMinery.Main
{
    public record struct CamSettings
    {
        public float Zoom { get; set; }
        public Vector2 Position { get; set; }
        // public Vector2 HardLimit { get;set; }
        public CamSettings(float zoom, Vector2 position)
        {
            Zoom = zoom;
            Position = position;
        }
    }
    public partial class Camera : Camera2D
    {
        public static float upperLimit = 0.3f, lowerLimit = 2f, currentzoom = 1 ,cameraSpeed = 2f;


        public static Vector2 MaxSize { get; set; }

        public static bool UISCALECHANGE { get; set; } = false;
        public bool CanMod { get; set; } = true;

        public Rect2 RelativeBounds { get; set; }
        public List<Vector2> RelativePoints { get; set; }

        public bool moving = false;
        public CamSettings Settings
        {
            get => camSettings;
            set
            {
                this.Zoom = new Vector2(value.Zoom, value.Zoom);
                this.Position = value.Position;
                camSettings = value;
                currentzoom = value.Zoom;
            }
        }
        private CamSettings camSettings;

        public static bool Disabled { get; set; }
        public override void _Ready()
        {
            MaxSize = new Vector2(LimitRight - LimitLeft, LimitBottom - LimitTop);
            this.MakeCurrent();
            //this.Position = new Vector2(0, 0);
            //this.Zoom = new Vector2(1, 1);

            
            //CheckLimit();
        }


        public bool SetPan(Vector2 pan)
        {
            var pos = this.Position;
            this.Position += pan * cameraSpeed;
            return pos != this.Position;

        }
        public void ZoomFinished()
        {

            EventDispatch.PushEventFlag(Data.GameEventType.CameraMove);

            //var cuttoff = (int)Zoom.X;
            var cuttoff = (float)Math.Abs(Math.Floor(Zoom.X) - Zoom.X);
            if (cuttoff < 0.1f)
            {
                //round to whole if within bounds (due to eventual rounding errors)
                Zoom = new Vector2(cuttoff, cuttoff);
            }

            currentzoom = Zoom.X;
            GD.Print("setting zoom to: ", currentzoom);
            moving = false;
        }

        public void SetTween(Vector2 zoom)
        {
            moving = true;
            var t = GetTree().CreateTween();
            t.TweenProperty(this, "zoom", zoom, 0.2f).
                SetTrans(Tween.TransitionType.Sine).
                SetEase(Tween.EaseType.Out);
            t.Connect(Tween.SignalName.Finished, Callable.From(ZoomFinished));
        }

        public bool SetZoom(float scrollDir)
        {
            //get_viewport_rect().size / self.zoom
            var scale = GetTree().Root.ContentScaleFactor;
            //1280,720 / 0.2 = 
            if (scrollDir < 0)
            {
                var amount = -0.3333333333333333334f;
                if (Math.Floor(scale) == scale)
                     amount = -0.5f;

                if (!CheckLimit(amount))
                    SetTween(new Vector2(currentzoom + amount, currentzoom + amount));

            }
            else
            {
                var amount = 0.3333333333333333334f;
                if (Math.Floor(scale) == scale)
                    amount = 0.5f;

                if (!CheckLimit(amount))
                    SetTween(new Vector2(currentzoom + amount, currentzoom + amount));
            }
            //if (scrollDir < 0 && currentzoom - zoomspeed >= upperLimit)
            //{
            //    if(GetWindow().ContentScaleFactor % 1 == 0)
            //    {

            //    }
            //    else
            //    {


            //        currentzoom -= 0.3333333333333333334f;
            //        this.Zoom = new Vector2(currentzoom, currentzoom);
            //        GD.Print("Setting Zoom to: ", Zoom.ToString(), " With pos: ", this.Position);
            //        return true;
            //        //var snap2 = 0.66666667;
            //        //var next = currentzoom - snap1;
            //        //var next = currentzoom - 0.5;
            //        //int intZoom = (int)next;

            //        //var dist1 = intZoom - snap1;
            //        //var dist2 = intZoom - snap2;

            //        //var next2 = next - intZoom;

            //        //if (next2 == 0)
            //    }
            //    currentzoom -= zoomspeed;
            //    this.Zoom = new Vector2(currentzoom, currentzoom);
                
                
            //    GD.Print("Setting Zoom to: ", Zoom.ToString(), " With pos: ", this.Position);
            //    return true;

            //}
            //else if (scrollDir > 0 && currentzoom + zoomspeed <= lowerLimit)
            //{
            //    if (GetWindow().ContentScaleFactor % 1 == 0)
            //    {

            //    }
            //    else
            //    {


            //        currentzoom += 0.3333333333333333334f;
            //        this.Zoom = new Vector2(currentzoom, currentzoom);
            //        GD.Print("Setting Zoom to: ", Zoom.ToString(), " With pos: ", this.Position);
            //        return true;
            //    }
            //        currentzoom += zoomspeed;
            //    this.Zoom = new Vector2(currentzoom, currentzoom);
            //    GD.Print("Setting Zoom to: ", Zoom.ToString(), " With pos: ", this.Position);
            //    return true;
            //}
            return false;
        }



        public bool CheckLimit(float change)
        {
            return false;//TODO get rid of this
            if (currentzoom + change < upperLimit)
                return true;
            if (currentzoom + change > lowerLimit)
                return true;



            var rect = GetRelativeRect(change);
            var relativePosition = rect.Position;
            var relativeSize = rect.Size;

            if (relativePosition.X < LimitLeft || relativePosition.Y < LimitTop
                || relativePosition.X + relativeSize.X > LimitRight
                || relativePosition.Y + relativeSize.Y > LimitBottom)
            {
                GD.Print("relative pos: ", relativePosition);
                GD.Print("relative size", relativeSize);
                GD.Print("Zoom", Zoom);
                GD.Print("cant go beyond limit");
                return true;
            }
            var nextZoomvec = Zoom.X + change;
            UpdateBoundingBox(relativePosition, relativeSize, nextZoomvec);
            return false;

        }

        public void UpdateBoundingBox(Vector2 relativePosition, Vector2 relativeSize, float nextZoom)
        {
            RelativeBounds = new Rect2(relativePosition, relativeSize);

            //get the relative pixel distance of top bar of a unzoomed camera (i.e, 1-1 UI pixel to world pixel)
            var change = GetTree().Root.ContentScaleFactor * NavBar.GlobalHeight;
            //now get relative world pos based on zoom pixel
            change = change / nextZoom ;
            RelativePoints = new List<Vector2>()
            {
                new Vector2(RelativeBounds.Position.X, RelativeBounds.Position.Y + change ), //top left
                new Vector2(RelativeBounds.Position.X, RelativeBounds.Size.Y + RelativeBounds.Position.Y ),//- change), //bot left
                new Vector2(RelativeBounds.Size.X + RelativeBounds.Position.X, RelativeBounds.Size.Y + RelativeBounds.Position.Y ),//- change), //bot right
                new Vector2(RelativeBounds.Size.X + RelativeBounds.Position.X, RelativeBounds.Position.Y + change), //top right
                new Vector2(RelativeBounds.Position.X, RelativeBounds.Position.Y + change ),  //IMPORTANT ADDD BACK  IN BECAUSE WE MUST CHECK THE 3-4 CONNECTION AGAIN

            };
        }

        public Rect2 GetRelativeRect(float zoomAdd = 0)
        {
            var size = GetViewportRect().Size;
            var pos = GetViewportRect().Position;
           // var nextZoom = zoomspeed * scrollDir;
            var nextZoomvec = new Vector2(Zoom.X + zoomAdd, Zoom.Y + zoomAdd);

            var relativeSize = size / nextZoomvec;
            var relativePosition = this.Position - relativeSize / 2;
            return new Rect2(relativePosition, relativeSize);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (CanMod && !ObjectTextEdit.IS_FOCUS) 
                HandleBaseInput();
            if(this.IsCurrent() && UISCALECHANGE)
            {
                UISCALECHANGE = false;
                SnapCamera();

            }
        }

        public void SnapCamera()
        {
            if (GetWindow().ContentScaleFactor % 1 == 0)
            {
                //either 1 or 2, so result is either 1 or 0.5f
                currentzoom = 1 / GetWindow().ContentScaleFactor;
                this.Zoom = new Vector2(currentzoom, currentzoom);

            }
            else
            {
                var zoom = currentzoom % 1;
                var snap1 = 0.333333333333333333334;
                var snap2 = 0.666666666666666666667;
                currentzoom = 1.33333333333333333334f;
                this.Zoom = new Vector2(currentzoom, currentzoom);

            }
        }

        public void HandleBaseInput()
        {
            if (moving || Disabled)
                return;
            var scrollDir = 0.0f;
            var speedCheck = Vector2.Zero;
            //these will return 1s and 0s as it's a keyboard input. Otherwise, controller will do something else
            //speedCheck.X = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
            //speedCheck.Y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");


            //var down = Input.IsActionJustReleased("scroll_down");
            //var up = Input.IsActionJustReleased("scroll_up");
            if (EventDispatch.FetchLastInput() == Data.EventType.Zoom_Out)
            {
                scrollDir = -1;
            }
            else if (EventDispatch.FetchLastInput() == Data.EventType.Zoom_In)
            {
                scrollDir = 1;
            }
            if (speedCheck != Vector2.Zero || scrollDir != 0)
            {
               // SetPan(speedCheck);
                SetZoom(scrollDir);
            }

        }
    }
}
