using Godot;
using MagicalMountainMinery.Data;
using MagicalMountainMinery.Obj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Design
{
    public partial class ObjectTextEdit : TextEdit
    {
        public static bool IS_FOCUS = false;
        public override void _Ready()
        {
            this.Connect(SignalName.TextChanged, Callable.From(OnTextChanged));
            this.Connect(SignalName.FocusExited, Callable.From(OnFocusExit));
            this.Connect(SignalName.FocusEntered, Callable.From(OnFocus));
            this.CustomMinimumSize = this.Size = new Vector2(64, 64);
            this.Set("theme_override_font_sizes/font_size", 16);
            this.Position = new(-32, -32);
            this.MouseFilter = MouseFilterEnum.Stop;
        }
        public void OnFocusExit()
        {
            this.Size = CustomMinimumSize;
            IS_FOCUS = false;
        }

        public void OnFocus()
        {
            IS_FOCUS = true;
            var rent = this.GetParent();
            if(rent is LevelTarget levelTarget)
                    this.Size = new Vector2(100, 50);
        }
        public LevelTarget ConvertToTarget()
        {
            var rent = this.GetParent();
            if (rent is LevelTarget levelTarget)
            {
                var newT = new LevelTarget();
                newT.Index = levelTarget.Index;
                newT.Position = levelTarget.Position;

                List<string>[] cons;
                var array = this.Text.Split('\n');
                cons = new List<string>[array.Length];
                //levelTarget.Conditions.Clear();
                //levelTarget.Batches.Clear();
                for (int i = 0; i < array.Length; i++)
                {
                    DoTarget(array[0].Split(' ').ToList(), newT, i);
                }

                return newT;
            }
            return null;
        }
        public void DoTarget(List<string> entries, LevelTarget target, int index)
        {
            ResourceType parsedEnumValue = ResourceType.Stone;
            ConCheck check = ConCheck.gt;
            int t = 0;
            if (Enum.TryParse(entries[0], true, out parsedEnumValue)
                && Enum.TryParse(entries[1], true, out check)
                && int.TryParse(entries[2], out t))
            {
                if (entries.Count() == 3)
                {
                    var con = new Condition(parsedEnumValue, t, check);
                    target.Conditions.Add(con);

                }
                else if (entries.Count() == 4)
                {
                    var con = new Condition(parsedEnumValue, t, check);
                    target.Conditions.Add(con);
                    target.Batches.Add(index);
                }
            }
            //this is a bonus condition, so treat the same but add to bonus cons
            else if (entries[0] == "*")
            {
                if (Enum.TryParse(entries[1], true, out parsedEnumValue)
                && Enum.TryParse(entries[2], true, out check)
                && int.TryParse(entries[3], out t))
                {
                    if (entries.Count() == 4)
                    {
                        var con = new Condition(parsedEnumValue, t, check);
                        target.BonusConditions.Add(con);

                    }
                }
            }




        }


        public void OnTextChanged()
        {

            try
            {
                var rent = this.GetParent();
                if (rent is Mineable mine)
                {
                    mine.ResourceSpawn.Amount = int.Parse(this.Text);

                }
                else if (rent is Track track)
                {

                }


            }
            catch (Exception e) { }

        }


    }
}
