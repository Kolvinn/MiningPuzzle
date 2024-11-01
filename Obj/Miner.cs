﻿using Godot;
using MagicalMountainMinery.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalMountainMinery.Obj
{
    public partial class Miner : Node2D
    {
        [Signal]
        public delegate void MiningFinishedEventHandler(Mineable mineable, Miner miner);

        private float cooldown = 0;

        public int Accuracy;
        public int Power;
        public int Fitness; //i.e., cooldown or 'Recovery'

        public float cooldownMultiplier = 100f; //this should be multiplied by fps

        public TextureProgressBar CooldownBar { get; set; }
        public AnimationPlayer player { get; set; }

        public Mineable MiningTarget;

        [Signal]
        public delegate void MiningHitEventHandler(Mineable mineable);

        public bool canMine { get; set; } = true;

        
        public override void _Ready()
        {
            this.CooldownBar = this.GetNode<TextureProgressBar>("CooldownBar");
            this.player = this.GetNode<AnimationPlayer>("Axe/AnimationPlayer");
            this.player.Connect(AnimationPlayer.SignalName.AnimationFinished, new Callable(this, nameof(AnimationFinished)));
        }

        public void AnimationFinished(string anim)
        {
            
            if (anim != "RESET")
            {
                //CooldownBar.Visible = true;
                //cooldown = 100;
                //EmitSignal(SignalName.MiningFinished, MiningTarget, this);
                //player.Play("RESET");
            }

        }

        public override void _PhysicsProcess(double delta)
        {
            //if (cooldown > 0)
            //{
            //    var val = 1 * cooldownMultiplier;
            //    cooldown -= val;
            //    CooldownBar.Value += val;
            //}
            //else if(CooldownBar.Value > 99 && !canMine)
            //{
            //    CooldownBar.Visible = false;
            //    CooldownBar.Value = 0;
            //    canMine = true;

            //}

        }


        public void Hit()
        {

            GD.Print("hit mining target: ",MiningTarget);
            EmitSignal(SignalName.MiningHit, MiningTarget);
        }

        public void Mine(IndexPos dir, Mineable target, float speed = 2f)
        {
            target.locked = true;
            this.MiningTarget = target;
            if (!string.IsNullOrEmpty(player.CurrentAnimation))
            {
                player.Stop();
                player.ClearQueue();
                player.Play("RESET");
            }
            var str = "East";
            if (dir == IndexPos.Up)
                str = "North";
            else if (dir == IndexPos.Down)
                str = "South";
            else if (dir == IndexPos.Left)
                str = "West";


            GD.Print("playing animation: ", str, " for target: ", MiningTarget);
            player.Play(str, speed);
            player.Queue("RESET");
            //MiningTarget = target;
            //canMine = false;
            //cooldown = 100f;
        }

        public bool CanMine () { return canMine; }


        


    }



}
