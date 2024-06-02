using Godot;
using MagicalMountainMinery.Data;
using static MagicalMountainMinery.Data.Load.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public Queue<Mineable> MiningTargets = new Queue<Mineable>();

        [Signal]
        public delegate void MiningHitEventHandler(Mineable mineable);

        public bool canMine { get; set; } = true;

        public AudioStreamPlayer MinerAudio {  get; set; }

        public double time;

        public float QueueSpeed = 1;
        public override void _Ready()
        {
            MinerAudio = new AudioStreamPlayer()
            {
                Bus = "Sfx"
            };
            this.AddChild(MinerAudio);
            this.CooldownBar = this.GetNode<TextureProgressBar>("CooldownBar");
            this.player = this.GetNode<AnimationPlayer>("Axe/AnimationPlayer");
            this.player.Connect(AnimationMixer.SignalName.AnimationFinished, new Callable(this, nameof(AnimationFinished)));

        }

        public void AnimationFinished(string anim)
        {
            var newtime = time - DateTime.Now.Millisecond;
            if (anim != "RESET")
            {
                //CooldownBar.Visible = true;
                //cooldown = 100;
                //EmitSignal(SignalName.MiningFinished, MiningTarget, this);
                player.Play("RESET");
            }
            else
            {
                var q = player.GetQueue();
                if (q.Count() > 0)
                {
                    player.Play(q[0]);
                    player.ClearCaches();
                    player.ClearQueue();
                }
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
            var rand = new Random().Next(3);
            MinerAudio.Stream = ResourceStore.OreHits[rand];
            MinerAudio.Play();
            if (MiningTargets.Count > 0)
            {

                EmitSignal(SignalName.MiningHit, MiningTargets.Dequeue());
            }
            else
            {
                EmitSignal(SignalName.MiningHit, null);
            }
        }

        public void Mine(IndexPos dir, Mineable target, float speed = 1f)
        {
            target.locked = true;
            MiningTargets.Enqueue(target);

           
            var str = "East";
            if (dir == IndexPos.Up)
                str = "North";
            else if (dir == IndexPos.Down)
                str = "South";
            else if (dir == IndexPos.Left)
                str = "West";
            time = DateTime.Now.Millisecond;

            if (!string.IsNullOrEmpty(player.CurrentAnimation))
            {
                if(player.CurrentAnimation != "RESET")
                {
                    player.Advance(1.5f);
                   // player.Play("RESET")
                }
                player.Queue(str + "Axe");//, ;
                QueueSpeed = speed * SIM_SPEED_RATIO;
                //if (player.CurrentAnimation == "RESET")
                //{
                    
                //}
                
                

            }
            else
                player.Play(str + "Axe",customSpeed: speed * SIM_SPEED_RATIO);//, speed * Runner.SIM_SPEED_RATIO);


            var speedy = player.GetPlayingSpeed();
            //player.Queue("RESET");
            //MiningTarget = target;
            //canMine = false;
            //cooldown = 100f;
        }

        public bool CanMine() { return canMine; }





    }



}
