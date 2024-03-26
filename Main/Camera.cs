using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Main
{
    public partial class Camera : Camera2D
    {
        public static float zoomspeed = 0.1f, upperLimit = 0.001f, lowerLimit = 4f, currentzoom = 1, cameraSpeed = 1.6f;

        public bool SetPan(Vector2 pan)
        {
            var pos = this.Position;
            this.Position += pan * cameraSpeed;
            return pos != this.Position;

        }
        public bool SetZoom(float scrollDir)
        {

            if (scrollDir < 0 && currentzoom - zoomspeed >= upperLimit)
            {
                currentzoom -= zoomspeed;
                this.Zoom = new Vector2(currentzoom, currentzoom);
                return true;

            }
            else if (scrollDir > 0 && currentzoom + zoomspeed <= lowerLimit)
            {
                currentzoom += zoomspeed;
                this.Zoom = new Vector2(currentzoom, currentzoom);
                return true;
            }
            return false;
        }

        public override void _PhysicsProcess(double delta)
        {
            HandleBaseInput();
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
