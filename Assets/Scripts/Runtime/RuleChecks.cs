using System;
namespace Hexbound
{
    public static class RuleChecks
    {
        static int count;
        static void Check(bool condition,string message) { if (!condition) throw new Exception("FAIL: "+message); count++; }
        static DuelModel Close() { var d=new DuelModel(0,4); d.players[0].x=300; d.players[1].x=395; return d; }
        static void Step(DuelModel d,int frames,DuelInput a=default(DuelInput),DuelInput b=default(DuelInput))
        { for(int i=0;i<frames;i++) d.Tick(1f/60,a,b); }
        public static int Run()
        {
            count=0;
            var d=new DuelModel(0,4); Check(d.remaining==99 && d.round==1,"99 second round");
            Step(d,60); Check(Math.Abs(d.remaining-98)<.01,"timer progresses");
            Check(typeof(Fighter).GetField("health")==null,"no health or life pool exists");
            d=Close(); d.Tick(1f/60,new DuelInput { punch=true },default(DuelInput)); Step(d,10);
            Check(d.players[1].knockback>0 && d.players[1].stun>0,"punch knocks back");
            float mana=d.players[0].mana; Step(d,15); Check(d.players[0].mana-mana<2,"one contact per attack");
            Check(!d.Finished && !d.players[1].fallen,"contact alone cannot eliminate");
            d=Close(); d.players[1].x=900; d.Tick(1f/60,new DuelInput { punch=true },default(DuelInput)); Step(d,20);
            Check(d.players[1].x==900,"out of range attack misses");
            d=Close(); d.Tick(1f/60,new DuelInput { punch=true },new DuelInput { block=true }); Step(d,10,default(DuelInput),new DuelInput { block=true });
            Check(d.players[1].knockback>0 && d.players[1].knockback<112,"guard reduces but does not remove knockback");
            d=Close(); d.Tick(1f/60,new DuelInput { push=true },default(DuelInput)); Step(d,20);
            Check(d.players[1].x>430 && !d.Finished,"shove moves opponent without elimination");
            d=Close(); d.players[0].mana=0; d.Tick(.016f,new DuelInput { power=true,special=true },default(DuelInput));
            Check(d.players[0].actionTime==0,"insufficient arcana rejects spells");
            d=Close(); d.players[0].mana=100; d.Tick(.016f,new DuelInput { power=true },default(DuelInput));
            Check(d.players[0].mana==40,"ultimate costs 60");
            d=new DuelModel(0,4); d.Tick(.016f,new DuelInput { jump=true },default(DuelInput)); Step(d,15);
            Check(d.players[0].height>0,"jump lifts fighter"); float v=d.players[0].verticalSpeed;
            d.Tick(.016f,new DuelInput { jump=true },default(DuelInput)); Check(d.players[0].verticalSpeed<v,"no double jump");
            for (int stage=1;stage<=3;stage++) {
                d=new DuelModel(0,4); d.round=stage;
                Check(d.RiftsOpen,"rune rift open in arena "+stage);
                d.players[0].x=DuelModel.RightRiftX; d.Tick(.016f,default(DuelInput),default(DuelInput));
                Check(d.Finished && d.winner==1 && d.players[0].fallen && d.players[1].wins==1,"rune rift awards round in arena "+stage);
            }
            d=new DuelModel(0,4); d.players[0].x=DuelModel.RightRiftX; d.Tick(.016f,default(DuelInput),default(DuelInput));
            Step(d,30); Check(d.players[1].wins==1,"score awarded once");
            d.NextRound(); Check(d.round==2 && d.remaining==99 && d.players[1].wins==1 && !d.players[0].fallen,"next arena resets fall and preserves score");
            d=new DuelModel(0,4); d.players[0].x=DuelModel.RightRiftX; d.players[0].height=80;
            d.Tick(.016f,default(DuelInput),default(DuelInput)); Check(!d.Finished,"airborne fighter survives rune rift");
            d=new DuelModel(0,4); d.elapsed=98.99f; d.Tick(.02f,default(DuelInput),default(DuelInput));
            Check(d.winner==-1 && d.players[0].wins==0 && d.players[1].wins==0,"timeout is draw, no life comparison");
            d=new DuelModel(0,4);
            for (int i=0;i<3;i++) { d.elapsed=98.99f; d.Tick(.02f,default(DuelInput),default(DuelInput)); if(i<2) d.NextRound(); }
            Check(d.MatchFinished && d.winner==-1,"three draws finish match");
            d.NextRound(); Check(d.round==3,"no fourth arena");
            d=Close();
            for(int i=0;i<3;i++) { d.players[0].x=300; d.players[1].x=395; d.players[1].knockback=0;
                d.Tick(1f/60,new DuelInput { punch=true },default(DuelInput)); Step(d,20); }
            Check(d.players[0].combo==3 && d.players[1].x>440,"combo finishes with stronger knockback");
            for (int id=0;id<8;id++) {
                d=new DuelModel(id,0); d.players[0].x=DuelModel.RightRiftX-290; d.players[1].x=DuelModel.RightRiftX-44;
                d.Tick(.016f,new DuelInput { special=true },default(DuelInput)); Step(d,120);
                Check(d.Finished && d.winner==0 && d.players[1].fallen,"spell can ring out for fighter "+id);
                d=new DuelModel(id,0); d.players[0].x=DuelModel.RightRiftX-290; d.players[1].x=DuelModel.RightRiftX-44; d.players[0].mana=100;
                d.Tick(.016f,new DuelInput { power=true },default(DuelInput)); Step(d,120);
                Check(d.Finished && d.winner==0,"power can ring out for fighter "+id);
            }
            foreach (bool shove in new[]{false,true}) {
                d=new DuelModel(0,4); d.players[0].x=DuelModel.RightRiftX-145; d.players[1].x=DuelModel.RightRiftX-48;
                d.Tick(.016f,new DuelInput { punch=!shove,push=shove },default(DuelInput)); Step(d,60);
                Check(d.winner==0,"melee/shove can ring out");
            }
            d=new DuelModel(0,4); d.players[0].x=DuelModel.LeftRiftX+48; d.players[1].x=DuelModel.LeftRiftX+145;
            d.Tick(.016f,default(DuelInput),new DuelInput { push=true }); Step(d,60);
            Check(d.winner==1,"P2 can shove left into rune rift");
            for(int stage=1;stage<=3;stage++) {
                d=new DuelModel(0,4); d.round=stage;
                d.players[0].x=DuelModel.LeftRiftX;
                d.Tick(.016f,default(DuelInput),default(DuelInput));
                Check(d.winner==1 && d.players[0].fallen,"left edge trap works in arena "+stage);
                d=new DuelModel(0,4); d.round=stage; d.players[0].x=610; d.players[1].x=670;
                Step(d,120);
                Check(!d.Finished && !d.players[0].fallen && !d.players[1].fallen,"center remains safe in arena "+stage);
            }
            d=new DuelModel(0,4); d.players[0].x=DuelModel.LeftRiftX; d.players[1].x=DuelModel.RightRiftX;
            d.Tick(.016f,default(DuelInput),default(DuelInput));
            Check(d.Finished && d.winner==-1 && d.players[0].fallen && d.players[1].fallen,"simultaneous falls into opposite rifts draw");
            Check(d.players[0].wins==0 && d.players[1].wins==0,"simultaneous ring-out awards no win");
            return count;
        }
    }
}
