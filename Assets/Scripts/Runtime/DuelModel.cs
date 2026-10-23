using System;
using System.Collections.Generic;

namespace Hexbound
{
    public enum Pose { Idle, Walk, Punch, Hurt, Knockout, Combo, Special, Power, Push, Jump }
    public struct DuelInput
    {
        public float move;
        public bool jump, punch, push, special, power, block;
    }
    public sealed class Fighter
    {
        public int character, wins, combo, facing, hitsTowardKnockdown;
        public float knockdownTime, knockdownElapsed;
        public bool IsKnockedDown => knockdownTime>0;
        public float x, height, verticalSpeed, knockback, mana = 35;
        public float actionTime, actionLength, stun, comboWindow, stepClock;
        public bool blocking, attackPending, fallen;
        public Pose pose;
        public Fighter(int character, float x, int facing) { this.character = character; this.x = x; this.facing = facing; }
    }
    public sealed class Spell
    {
        public int owner, character, direction;
        public float x, height, life = 2, speed, force;
        public bool ultimate;
    }
    public sealed class Burst
    {
        public int character;
        public float x, height, life = .45f;
        public bool ultimate;
    }
    // Pure C# rules: rendering and keyboard input live in AcademyGame.
    public sealed class DuelModel
    {
        public readonly Fighter[] players;
        public readonly List<Spell> spells = new List<Spell>();
        public readonly List<Burst> bursts = new List<Burst>();
        public readonly List<string> sounds = new List<string>();
        public float remaining = 99, elapsed;
        public int round = 1, winner = -2;
        public string reason = "";
        public bool Finished => winner != -2;
        public bool MatchFinished => Finished && round == 3;
        public bool RiftsOpen => true;
        public const float LeftRiftX=100, RightRiftX=1180;
        public static bool OverRift(float x) => Math.Abs(x-LeftRiftX)<36 || Math.Abs(x-RightRiftX)<36;
        readonly int startingArena;
        public int ArenaIndex => (startingArena+round-1)%3;
        public DuelModel(int p1, int p2, int arena=0) { startingArena=Math.Max(0,Math.Min(2,arena)); players = new[] { new Fighter(p1, 410, 1), new Fighter(p2, 870, -1) }; }
        public void NextRound()
        {
            if (!Finished || round >= 3) return;
            round++; remaining = 99; elapsed = 0; winner = -2; reason = ""; spells.Clear(); bursts.Clear();
            for (int i = 0; i < 2; i++) {
                int score = players[i].wins, id = players[i].character;
                players[i] = new Fighter(id, i == 0 ? 410 : 870, i == 0 ? 1 : -1) { wins = score };
            }
        }
        public void Tick(float dt, DuelInput a, DuelInput b)
        {
            sounds.Clear();
            for (int i = bursts.Count-1; i >= 0; i--) { bursts[i].life -= dt; if (bursts[i].life <= 0) bursts.RemoveAt(i); }
            if (Finished) return;
            elapsed += dt; remaining = Math.Max(0, 99-elapsed);
            UpdateFighter(0, a, dt); UpdateFighter(1, b, dt);
            // Resolve active windows only after both players have updated their guards and positions.
            ResolveAttack(0); ResolveAttack(1);
            for (int i = spells.Count-1; i >= 0; i--) {
                var s = spells[i]; s.x += s.direction*s.speed*dt; s.life -= dt;
                Fighter target = players[1-s.owner];
                if (Math.Abs(s.x-target.x)<40 && Math.Abs(s.height-target.height)<75) {
                    Hit(target,s.direction*s.force,s.character,s.ultimate); spells.RemoveAt(i);
                } else if (s.life <= 0 || s.x < 0 || s.x > 1280) spells.RemoveAt(i);
            }
            // Solid bodies; fighters can cross by jumping over one another.
            var p = players[0]; var q = players[1]; float gap = q.x-p.x;
            if (Math.Abs(gap)<55 && Math.Abs(p.height-q.height)<70) {
                float sign = gap>=0 ? 1 : -1, separation=(55-Math.Abs(gap))/2;
                p.x-=separation*sign; q.x+=separation*sign;
            }
            bool fall0 = RiftsOpen && OverRift(p.x) && p.height<12;
            bool fall1 = RiftsOpen && OverRift(q.x) && q.height<12;
            if (fall0 || fall1) {
                p.fallen=fall0; q.fallen=fall1;
                sounds.Add("fall"); Finish(fall0 && fall1 ? -1 : fall0 ? 1 : 0,"RIFT RING OUT");
            } else if (remaining<=0) Finish(-1,"TIME UP / NO RING OUT");
        }
        void UpdateFighter(int index, DuelInput input, float dt)
        {
            var p=players[index]; var enemy=players[1-index];
            p.facing=enemy.x>=p.x ? 1 : -1;
            p.comboWindow=Math.Max(0,p.comboWindow-dt); if (p.comboWindow==0) p.combo=0;
            p.mana=Math.Min(100,p.mana+dt*4);
            p.stun=Math.Max(0,p.stun-dt);
            p.x+=p.knockback*dt; p.knockback*=Math.Max(0,1-7*dt);
            if (p.height>0 || p.verticalSpeed>0) {
                p.height+=p.verticalSpeed*dt; p.verticalSpeed-=1100*dt;
                if (p.height<=0) { p.height=0; p.verticalSpeed=0; sounds.Add("step"); }
            }
            if(p.IsKnockedDown) {
                p.knockdownElapsed+=dt; p.knockdownTime=Math.Max(0,p.knockdownTime-dt);
                p.blocking=false; p.attackPending=false; p.actionTime=0; p.combo=0; p.comboWindow=0;
                p.pose=p.IsKnockedDown?Pose.Knockout:Pose.Idle;
                p.x=Math.Max(65,Math.Min(1215,p.x)); return;
            }
            p.blocking=input.block && p.height==0 && p.stun==0 && p.actionTime<=0;
            if (p.actionTime>0) { p.actionTime=Math.Max(0,p.actionTime-dt); }
            if (p.stun>0) p.pose=Pose.Hurt;
            else if (p.actionTime<=0) {
                p.pose=Pose.Idle;
                if (input.jump && p.height==0) { p.verticalSpeed=510; sounds.Add("jump"); }
                float movement=input.move*(p.blocking ? 75 : 235); p.x+=movement*dt;
                if (Math.Abs(movement)>1) {
                    p.pose=Pose.Walk; p.stepClock-=dt;
                    if (p.stepClock<=0 && p.height==0) { sounds.Add("step"); p.stepClock=.24f; }
                }
                if (p.height>0) p.pose=Pose.Jump;
                if (!p.blocking) {
                    if (input.power && p.mana>=60) { p.mana-=60; Start(p,Pose.Power,.75f,"power"); }
                    else if (input.special && p.mana>=20) { p.mana-=20; Start(p,Pose.Special,.48f,"cast"); }
                    else if (input.push) Start(p,Pose.Push,.45f,"punch");
                    else if (input.punch) {
                        p.combo=p.comboWindow>0 ? p.combo%3+1 : 1; p.comboWindow=.85f;
                        Start(p,p.combo==3 ? Pose.Combo : Pose.Punch,.30f,"punch");
                    }
                }
            }
            p.x=Math.Max(65,Math.Min(1215,p.x));
        }
        void Start(Fighter p, Pose pose, float duration, string sound)
        { p.pose=pose; p.actionLength=p.actionTime=duration; p.attackPending=true; sounds.Add(sound); }
        void ResolveAttack(int index)
        {
            var p=players[index]; var enemy=players[1-index];
            if (p.stun>0 || p.IsKnockedDown) { p.attackPending=false; return; }
            if (!p.attackPending || p.actionTime>p.actionLength*.55f) return;
            p.attackPending=false;
            if (p.pose==Pose.Special || p.pose==Pose.Power) {
                bool big=p.pose==Pose.Power;
                // Eight spell profiles: flame, ice, lunar, thorn, lightning, earth, gravity, blood rune.
                float[] speed={470,560,380,430,760,320,300,520};
                float[] force={190,150,260,230,180,390,440,210};
                spells.Add(new Spell { owner=index,character=p.character,direction=p.facing,x=p.x+p.facing*45,height=p.height,
                    speed=speed[p.character],force=force[p.character]*(big?1.7f:1),ultimate=big });
            } else if (Math.Abs(enemy.x-p.x)<115 && (enemy.x-p.x)*p.facing>=0 && Math.Abs(enemy.height-p.height)<70) {
                bool push=p.pose==Pose.Push, combo=p.pose==Pose.Combo;
                Hit(enemy,p.facing*(push ? 760 : combo ? 590 : 320),p.character,combo);
                p.mana=Math.Min(100,p.mana+7);
            }
        }
        void Hit(Fighter target,float force,int character,bool big)
        {
            bool guard=target.blocking;
            target.knockback=force*(guard?.35f:1); target.stun=guard?.08f:.22f;
            target.attackPending=false; target.actionTime=0;
            if(!guard && !target.IsKnockedDown && ++target.hitsTowardKnockdown>=4) {
                target.hitsTowardKnockdown=0; target.knockdownTime=1.4f; target.knockdownElapsed=0; target.blocking=false;
            }
            target.pose=target.IsKnockedDown?Pose.Knockout:Pose.Hurt;
            bursts.Add(new Burst { character=character,x=target.x,height=target.height,ultimate=big });
            sounds.Add(guard ? "block" : "hit");
        }
        void Finish(int player,string cause)
        {
            winner=player; reason=cause;
            if (player>=0) players[player].wins++;
            foreach (var p in players) { p.attackPending=false; if (p.fallen) p.pose=Pose.Knockout; }
            sounds.Add("win");
        }
    }
}
