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

        private static Queue<GameEventType> EventFlags = new Queue<GameEventType>();

        private static EventType Last { get; set; } = EventType.Nill;

        private static GameEventType LastFlag { get; set; } = GameEventType.Nil;

        public static Dictionary<string, EventType> Events { get; private set; } = new Dictionary<string, EventType>()
        {
            { "Start Mining", EventType.Start_Mining },
            { "Stop Mining", EventType.Stop_Mining },
            { "Sim Speed +", EventType.Speed_Increase },
            { "Sim Speed -", EventType.Speed_Decrease },
            { "Zoom In", EventType.Zoom_In },
            { "Zoom Out", EventType.Zoom_Out },
            { "Settings", EventType.Settings },
            { "Home", EventType.Home },
            { "Toggle Shop", EventType.Toggle_Shop },
            { "Reset Level", EventType.Reset_Level },
            { "Pause", EventType.Pause },
            { "Rotate", EventType.Rotate }
        };

        public override void _Ready()
        {

            //this.SetProcessInput(false);
        }
        /// <summary>
        /// Returns true if the given string is a stored event that the given event matches
        /// </summary>
        /// <param name="event"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static bool MatchEvent(string @event, EventType env)
        {
            if (Events.ContainsKey(@event))
            {
                return Events[@event] == env;
            }
            return false;
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
        public static bool TryClickEvent(IUIComponent comp)
        {
            if(string.IsNullOrEmpty(comp?.UIID))
                return false;
            var s = hoverList.First().UIID;
            if (Events.ContainsKey(s))
            {
                
                eventTypes.Enqueue(Events[s]);
                return true;
            }
            return false;

        }
        public override void _PhysicsProcess(double delta)
        {
            //IGNORE ALL mouse clicks when inside UI override
            if (overrides.Count > 0 && (Input.IsActionJustPressed("left_click")
                || Input.IsActionJustReleased("left_click")
                || Input.IsActionJustPressed("right_click")
                || Input.IsActionJustReleased("right_click")))
            {
                if (hoverList.Count > 0 && Input.IsActionJustPressed("left_click"))
                {
                    if(!TryClickEvent(PeekHover()))
                        eventTypes.Enqueue(EventType.Left_Action);
                    LastPressedPosition = GetGlobalMousePosition();
                }
                else if (hoverList.Count > 0 && Input.IsActionJustReleased("left_click"))
                {
                    LastPressedPosition = GetGlobalMousePosition();
                    eventTypes.Enqueue(EventType.Left_Release);
                }
                return;
            }
            //Need to fetch context
            //TODO reject inputs that shop and settings dont want
            else if (Input.IsActionJustPressed("left_click"))
            {
                if (!TryClickEvent(PeekHover()))
                    eventTypes.Enqueue(EventType.Left_Action);
                else
                {
                    LastPressedPosition = GetGlobalMousePosition();
                    //eventTypes.Enqueue(EventType.Left_Action);
                }
            }
            else if (Input.IsActionJustReleased("left_click"))
            {
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
            else 
            { 
                foreach(var entry in Events)
                {
                    if(Input.IsActionJustPressed(entry.Key))
                    {
                        eventTypes.Enqueue(entry.Value);
                    }
                }
            }
            mousePos = GetGlobalMousePosition();

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
        public  static EventType FetchLastInput()
        {
            return Last;
        }

        public void SetLastInput()
        {
            Last = eventTypes.Count == 0 ? EventType.Nill : eventTypes.Dequeue();
        }
        public void SetLastFlag()
        {
            LastFlag = EventFlags.Count > 0 ? EventFlags.Dequeue() : GameEventType.Nil;
        }

        public static GameEventType FetchLastFlag()
        {
            return LastFlag;
        }
        public static IGameObject FetchInteractable()
        {
            return interactables.Count == 0 ? null : interactables.First();
        }

        public static void HoverUI(IUIComponent comp)
        {
            hoverList.Add(comp);
        }
        public static void ExitUI(IUIComponent comp)
        {
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

        public static void PushEventFlag(GameEventType eventType)
        {
            EventFlags.Enqueue(eventType);
        }

        public GameEventType PopEventFlag()
        {
            return EventFlags.Count > 0 ? EventFlags.Dequeue() : GameEventType.Nil;
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

        public static void FrameDisableLastInput()
        {
            Last = EventType.Nill;
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
            Last = EventType.Nill;
            
        }
    }
}
