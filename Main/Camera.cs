﻿using Godot;
using MagicalMountainMinery.Design;

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
        public static float zoomspeed = 0.01f, upperLimit = 0.0001f, lowerLimit = 7f, currentzoom = 1, cameraSpeed = 1.5f;
        public static Vector2 MaxSize;

        public static bool UISCALECHANGE { get; set; } = false;
        public bool CanMod { get; set; } = true;
        public CamSettings Settings
        {
            get => camSettings;
            set
            {
                this.Zoom = new Vector2(value.Zoom, value.Zoom);
                this.Position = value.Position;
                camSettings = value;
            }
        }
        private CamSettings camSettings;
        public override void _Ready()
        {
            MaxSize = new Vector2(LimitRight - LimitLeft, LimitBottom - LimitTop);
            this.MakeCurrent();
            this.Position = new Vector2(0, 0);
            this.Zoom = new Vector2(1, 1);

            
            //CheckLimit();
        }


        public bool SetPan(Vector2 pan)
        {
            var pos = this.Position;
            this.Position += pan * cameraSpeed;
            return pos != this.Position;

        }


        public bool SetZoom(float scrollDir)
        {
            //get_viewport_rect().size / self.zoom
            if (CheckLimit(scrollDir))
                return false;
            //1280,720 / 0.2 = 
            if (scrollDir < 0 && currentzoom - zoomspeed >= upperLimit)
            {
                if(GetWindow().ContentScaleFactor % 1 == 0)
                {

                }
                else
                {


                    currentzoom -= 0.3333333333333333334f;
                    this.Zoom = new Vector2(currentzoom, currentzoom);
                    GD.Print("Setting Zoom to: ", Zoom.ToString(), " With pos: ", this.Position);
                    return true;
                    //var snap2 = 0.66666667;
                    //var next = currentzoom - snap1;
                    //var next = currentzoom - 0.5;
                    //int intZoom = (int)next;

                    //var dist1 = intZoom - snap1;
                    //var dist2 = intZoom - snap2;

                    //var next2 = next - intZoom;

                    //if (next2 == 0)
                }
                currentzoom -= zoomspeed;
                this.Zoom = new Vector2(currentzoom, currentzoom);
                
                
                GD.Print("Setting Zoom to: ", Zoom.ToString(), " With pos: ", this.Position);
                return true;

            }
            else if (scrollDir > 0 && currentzoom + zoomspeed <= lowerLimit)
            {
                if (GetWindow().ContentScaleFactor % 1 == 0)
                {

                }
                else
                {


                    currentzoom += 0.3333333333333333334f;
                    this.Zoom = new Vector2(currentzoom, currentzoom);
                    GD.Print("Setting Zoom to: ", Zoom.ToString(), " With pos: ", this.Position);
                    return true;
                }
                    currentzoom += zoomspeed;
                this.Zoom = new Vector2(currentzoom, currentzoom);
                GD.Print("Setting Zoom to: ", Zoom.ToString(), " With pos: ", this.Position);
                return true;
            }
            return false;
        }



        public bool CheckLimit(float scrollDir)
        {
            var size = GetViewportRect().Size;
            var pos = GetViewportRect().Position;
            var nextZoom = zoomspeed * scrollDir;
            var nextZoomvec = new Vector2(Zoom.X + nextZoom, Zoom.Y + nextZoom);

            var relativeSize = size / nextZoomvec;
            var relativePosition = this.Position - relativeSize / 2;




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
            return false;

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
            var scrollDir = 0.0f;
            var speedCheck = Vector2.Zero;
            //these will return 1s and 0s as it's a keyboard input. Otherwise, controller will do something else
            speedCheck.X = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
            speedCheck.Y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");


            var down = Input.IsActionJustReleased("scroll_down");
            var up = Input.IsActionJustReleased("scroll_up");
            if (down)
            {
                scrollDir = -1;
            }
            else if (up)
            {
                scrollDir = 1;
            }
            if (speedCheck != Vector2.Zero || scrollDir != 0)
            {
                SetPan(speedCheck);
                SetZoom(scrollDir);
            }

        }
    }
}
