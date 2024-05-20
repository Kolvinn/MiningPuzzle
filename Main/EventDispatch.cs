using Godot;
using MagicalMountainMinery.Data;
using System.Collections.Generic;
using System.Linq;

namespace MagicalMountainMinery.Main
{
    public partial class EventDispatch : Node2D
    {
        private static Queue<EventType> eventTypes = new Queue<EventType>();

        private static HashSet<IGameObject> interactables = new HashSet<IGameObject>();

        private static Vector2 mousePos;

        private static Vector2 LastPressedPosition;

        private static bool mouseMoveWait;

        private static List<IUIComponent> hoverList = new List<IUIComponent>();

        private static List<GameEvent> gameEvents = new List<GameEvent>();

        private static EventType Last { get; set; } = EventType.Nill;

        public override void _Ready()
        {

            //this.SetProcessInput(false);
        }

        public static List<GuiOverride> overrides { get; set; } = new List<GuiOverride>();
        public static void WithinOverride(GuiOverride control)
        {
            if (overrides.Contains(control)) { return; }
            overrides.Add(control);
        }

        public static void ExitOverride(GuiOverride control)
        {
            if (overrides.Contains(control)) { overrides.Remove(control); }

        }

        public override void _PhysicsProcess(double delta)
        {
            if (overrides.Count > 0)
            {
                if (hoverList.Count > 0 && Input.IsActionJustPressed("left_click"))
                {
                    LastPressedPosition = GetGlobalMousePosition();
                    eventTypes.Enqueue(EventType.Left_Action);
                }
                return;
            }

            mousePos = GetGlobalMousePosition();
            //Need to fetch context
            if (Input.IsActionJustPressed("left_click"))
            {

                LastPressedPosition = GetGlobalMousePosition();
                // mouseMoveWait = true;
                //SetProcessInput(true);
                eventTypes.Enqueue(EventType.Left_Action);
            }
            else if (Input.IsActionJustReleased("left_click"))
            {
                //this.SetProcessInput(false);

                //GD.Print("release in dispatch");
                //if (mouseMoveWait)
                //{
                //    eventTypes.Enqueue(EventType.Drag_End);
                //    mouseMoveWait = false;
                //}
                eventTypes.Enqueue(EventType.Left_Release);
            }
            else if (Input.IsActionJustPressed("right_click"))
            {
                eventTypes.Enqueue(EventType.Right_Action);
            }
            else if (Input.IsActionJustReleased("right_click"))
            {
                eventTypes.Enqueue(EventType.Right_Release);
            }

            else if (Input.IsActionJustPressed("level_toggle"))
            {
                eventTypes.Enqueue(EventType.Level_Toggle);
            }
            else if (Input.IsActionJustPressed("north_cart"))
            {
                eventTypes.Enqueue(EventType.North_Cart);
            }
            else if (Input.IsActionJustPressed("south_cart"))
            {
                eventTypes.Enqueue(EventType.South_Cart);
            }
            else if (Input.IsActionJustPressed("east_cart"))
            {
                eventTypes.Enqueue(EventType.East_Cart);
            }
            else if (Input.IsActionJustPressed("west_cart"))
            {
                eventTypes.Enqueue(EventType.West_Cart);
            }
            else if (Input.IsActionJustPressed("rotate"))
            {
                eventTypes.Enqueue(EventType.Rotate);
            }
            else if (Input.IsActionJustPressed("space"))
            {
                eventTypes.Enqueue(EventType.Space);
            }
            else if (Input.IsActionJustPressed("escape"))
            {
                eventTypes.Enqueue(EventType.Escape);
            }

        }

        public static void PushGameEvent(GameEvent gameEvent)
        {
            gameEvents.Add(gameEvent);
        }
        public static GameEvent PopGameEvent()
        {
            if (gameEvents.Count > 0)
            {
                var hover = gameEvents.First();
                gameEvents.Remove(hover);
                return hover;
            }
            return new GameEvent();
        }

        /// <summary>
        /// Fetches the last event generated via key,mouse,movement input. Lasts for one physics frame
        /// from when it is picked by the GameController
        /// </summary>
        /// <returns></returns>
        public static EventType FetchLastInput()
        {
            return Last;
        }

        public void SetLastInput()
        {
            Last = eventTypes.Count == 0 ? EventType.Nill : eventTypes.Dequeue();
        }

        public static IGameObject FetchInteractable()
        {
            return interactables.Count == 0 ? null : interactables.First();
        }

        public static void HoverUI(IUIComponent comp)
        {

            GD.Print("entering btn");
            hoverList.Add(comp);
        }
        public static void ExitUI(IUIComponent comp)
        {
            GD.Print("exiting btn");
            hoverList.Remove(comp);
        }
        //public static IUIComponent GetHover()
        //{


        //    if (hoverList.Count > 0)
        //    {
        //        GD.Print("returning btn");
        //        var hover = hoverList.First();
        //        hoverList.Remove(hover);
        //        return hover;
        //    }
        //    return null;
        //}

        /// <summary>
        /// Clears current Iucomponent queue. Useful for switching scenes or when buttons cannot be exited
        /// </summary>
        public static void ClearUIQueue()
        {
            hoverList.Clear();
        }

        public static IUIComponent PeekHover()
        {
            if (hoverList.Count > 0)
            {
                return hoverList.First();
            }
            return null;
        }

        /// <summary>
        /// Consumes the given hover only if the hover is top level
        /// </summary>
        /// <param name="comp"></param>
        public static void ConsumeHover(IUIComponent comp)
        {
            if (hoverList.Count > 0 && hoverList[0] == comp)
                hoverList.RemoveAt(0);
        }

        public static void Entered(IGameObject interactable)
        {
            interactables.Add(interactable);
        }

        public static void Exited(IGameObject interactable)
        {
            if (interactables.Contains(interactable))
                interactables.Remove(interactable);
        }

        public static Vector2 MousePos()
        {
            return mousePos;

        }


        //public override void _Input(InputEvent @event)
        //{
        //    if (@event is InputEventMouseMotion input)
        //    {
        //        eventTypes.Enqueue(EventType.Drag_Start);
        //        this.SetProcessInput(false);


        //    }

        //}

        public Vector2 GetMouseDirection(Vector2 dragVec)
        {
            var Xmult = dragVec.X >= 0 ? 1 : -1;
            var Ymult = dragVec.Y >= 0 ? 1 : -1;

            var abs = dragVec.Abs();

            //west or east, so can return X. 1-1 will prio west east
            if (abs.X >= abs.Y)
            {
                return new Vector2(Xmult, 0);
            }
            else
            {
                return new Vector2(Xmult, 0);
            }
        }

        public static void ClearAll()
        {
            overrides.Clear();
            eventTypes.Clear();
            hoverList.Clear();
            interactables.Clear();
        }
    }
}
