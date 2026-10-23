using System;
namespace Hexbound
{
    // No test framework dependency; callable in Editor or with the bundled Mono compiler.
    public static class RuleChecks
    {
        static int count;
        static void Check(bool condition,string message) { if (!condition) throw new Exception("FAIL: "+message); count++; }
        static DuelModel Close() { var d=new DuelModel(0,4); d.players[0].x=450; d.players[1].x=545; return d; }
        static void Step(DuelModel d,int frames,DuelInput a=default(DuelInput),DuelInput b=default(DuelInput))
        { for(int i=0;i<frames;i++) d.Tick(1f/60,a,b); }
        public static int Run()
        {
            count=0;
            var d=new DuelModel(0,4); Check(d.remaining==99 && d.round==1,"round starts at 99");
            Step(d,60); Check(Math.Abs(d.remaining-98)<.01,"timer progresses");
            d=Close(); d.Tick(1f/60,new DuelInput { punch=true },default(DuelInput)); Step(d,10);
            Check(d.players[1].health==93,"basic attack hits once"); Step(d,15); Check(d.players[1].health==93,"no repeated damage");
            d=Close(); d.players[1].x=900; d.Tick(1f/60,new DuelInput { punch=true },default(DuelInput)); Step(d,20);
            Check(d.players[1].health==100,"out of range attack misses");
            d=Close(); d.Tick(1f/60,new DuelInput { punch=true },new DuelInput { block=true }); Step(d,15,default(DuelInput),new DuelInput { block=true });
            Check(d.players[1].health>98 && d.players[1].health<100,"guard reduces damage");
            d=Close(); d.Tick(1f/60,new DuelInput { push=true },default(DuelInput)); Step(d,20);
            Check(d.players[1].health==100 && d.players[1].x>565,"push moves without damage");
            d=Close(); d.players[0].mana=0; d.Tick(.016f,new DuelInput { power=true,special=true },default(DuelInput));
            Check(d.players[0].actionTime==0,"insufficient arcana rejects spells");
            d=Close(); d.players[0].mana=100; d.Tick(.016f,new DuelInput { power=true },default(DuelInput));
            Check(d.players[0].mana==40,"ultimate costs 60");
            d=new DuelModel(0,4); d.Tick(.016f,new DuelInput { jump=true },default(DuelInput)); Step(d,15);
            Check(d.players[0].height>0,"jump lifts fighter"); float v=d.players[0].verticalSpeed;
            d.Tick(.016f,new DuelInput { jump=true },default(DuelInput)); Check(d.players[0].verticalSpeed<v,"no double jump");
            d=new DuelModel(0,4); d.elapsed=8; d.players[0].x=d.HoleX; d.Tick(.016f,default(DuelInput),default(DuelInput));
            Check(d.Finished && d.winner==1 && d.players[1].wins==1,"manhole awards opponent round");
            Step(d,30); Check(d.players[1].wins==1,"round awards score only once");
            d.NextRound(); Check(d.round==2 && d.remaining==99 && d.players[1].wins==1 && d.players[0].health==100,"next arena resets health and preserves score");
            d=new DuelModel(0,4); d.elapsed=8; d.players[0].x=d.HoleX; d.players[0].height=80;
            d.Tick(.016f,default(DuelInput),default(DuelInput)); Check(!d.Finished,"airborne fighter survives drain");
            d=new DuelModel(0,4); d.elapsed=98.99f; d.players[0].health=80; d.Tick(.02f,default(DuelInput),default(DuelInput));
            Check(d.winner==1 && d.reason=="TIME UP","timeout selects higher health");
            d=new DuelModel(0,4);
            for (int i=0;i<3;i++) { d.elapsed=98.99f; d.Tick(.02f,default(DuelInput),default(DuelInput)); if(i<2) d.NextRound(); }
            Check(d.MatchFinished && d.winner==-1 && d.players[0].wins==0,"three draws still finish match");
            d.NextRound(); Check(d.round==3,"cannot start fourth round");
            d=Close();
            for (int i=0;i<3;i++) { d.players[0].x=450; d.players[1].x=545; d.players[1].knockback=0;
                d.Tick(1f/60,new DuelInput { punch=true },default(DuelInput)); Step(d,20); }
            Check(d.players[1].health==68,"three attack chain ends with heavy combo");
            for (int id=0;id<8;id++) {
                d=new DuelModel(id,0); d.players[0].x=400; d.players[1].x=650;
                d.Tick(.016f,new DuelInput { special=true },default(DuelInput)); Step(d,100);
                Check(d.players[1].health<100,"spell hits for fighter "+id);
            }
            return count;
        }
    }
}
