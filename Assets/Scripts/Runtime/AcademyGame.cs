using UnityEngine;
using System.Collections.Generic;

namespace Hexbound
{
    public sealed class AcademyGame : MonoBehaviour
    {
        enum ScreenState { Title, Select, StageSelect, Fight }
        ScreenState screen;
        readonly int[] selected={0,4};
        readonly bool[] ready={false,false};
        Texture2D[] portraits=new Texture2D[8], showcases=new Texture2D[8], abilityIcons=new Texture2D[8];
        int selectedArena;
        Texture2D[] sheets=new Texture2D[8], avatars=new Texture2D[8], effects=new Texture2D[8], stages=new Texture2D[3];
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        Texture2D riftSheet, academyPanel, academyButton;
        AudioSource music, sfx;
        DuelModel model;
        AcademyInputManager controls;
        Texture2D runeDisc;
        float countdown, resultTime, animationTime;
        bool paused, help, muted;
        int lastSecond;
        Font font, titleFont;
        GUIStyle textStyle;
        readonly Color ink=new Color32(13,16,30,255), cream=new Color32(242,227,201,255), gold=new Color32(211,176,109,255);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { if (FindFirstObjectByType<AcademyGame>()==null) new GameObject("Hexbound Game").AddComponent<AcademyGame>(); }
        void Awake()
        {
            Application.targetFrameRate=60;
            controls=new AcademyInputManager();
            runeDisc=CreateRuneDisc();
            riftSheet=Resources.Load<Texture2D>("Stages/RuneRift");
            academyPanel=Resources.Load<Texture2D>("UI/AcademyPanel");
            academyButton=Resources.Load<Texture2D>("UI/AcademyButton");
            font=Resources.Load<Font>("UI/Tiny5"); titleFont=Resources.Load<Font>("UI/CinzelDecorative");
            for (int i=0;i<8;i++) {
                sheets[i]=Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/sheet");
                avatars[i]=Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/avatar");
                portraits[i]=Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/portraits");
                showcases[i]=Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/showcase");
                abilityIcons[i]=Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/abilities");
                effects[i]=Resources.Load<Texture2D>("Effects/"+Roster.Files[i]);
            }
            for (int i=0;i<3;i++) stages[i]=Resources.Load<Texture2D>("Stages/"+Roster.StageFiles[i]);
            foreach (var clip in Resources.LoadAll<AudioClip>("Audio/SFX")) clips[clip.name]=clip;
            music=gameObject.AddComponent<AudioSource>(); sfx=gameObject.AddComponent<AudioSource>();
            music.loop=true; music.volume=.5f; sfx.volume=.55f;
            if (FindFirstObjectByType<AudioListener>()==null) gameObject.AddComponent<AudioListener>();
            muted=PlayerPrefs.GetInt("Hexbound.Mute",0)==1; AudioListener.volume=muted?0:1;
            PlayMusic("Menu");
        }
        void PlayMusic(string track) { music.clip=Resources.Load<AudioClip>("Audio/Music/"+track); music.Play(); }
        void Sound(string name) { if (clips.TryGetValue(name,out var clip)) sfx.PlayOneShot(clip); }
        bool Key(KeyCode code) => Input.GetKeyDown(code);
        void Update()
        {
            controls.Poll(Time.unscaledTime);
            if(controls.ControllerDisconnected && screen==ScreenState.Fight && !model.Finished) paused=true;
            if (screen==ScreenState.Title) { animationTime+=Time.deltaTime; if (controls.StartPressed) OpenSelect(); return; }
            if (Key(KeyCode.M)) { muted=!muted; AudioListener.volume=muted?0:1; PlayerPrefs.SetInt("Hexbound.Mute",muted?1:0); }
            if (controls.Any("Help")) help=!help;
            if (controls.Any("Pause") && screen==ScreenState.Fight && !model.Finished) paused=!paused;
            if (!paused) animationTime+=Time.deltaTime;
            if (screen==ScreenState.StageSelect) {
                if (controls.Navigate(0).x<0||controls.Navigate(1).x<0) { selectedArena=(selectedArena+2)%3; Sound("select"); }
                if (controls.Navigate(0).x>0||controls.Navigate(1).x>0) { selectedArena=(selectedArena+1)%3; Sound("select"); }
                if (controls.Any("Confirm")) StartMatch();
                else if (controls.Any("Cancel")) OpenSelect();
                return;
            }
            if (screen==ScreenState.Select) {
                SelectKeys(0);
                SelectKeys(1);
                for(int p=0;p<2;p++) if(controls.Press(p,"Cancel")) { ready[p]=false; Sound("select"); }
                if (ready[0] && ready[1]) OpenStageSelect();
                return;
            }
            if(paused && controls.Any("Cancel")) { OpenSelect(); return; }
            if (paused || help) return;
            if (model.Finished) {
                resultTime+=Time.deltaTime;
                if (resultTime>1.2f && controls.Any("Confirm")) {
                    if (model.MatchFinished) OpenSelect();
                    else { model.NextRound(); BeginRound(); }
                }
                return;
            }
            if (countdown>0) {
                countdown-=Time.deltaTime;
                int sec=Mathf.CeilToInt(countdown);
                if (sec!=lastSecond) { Sound("bell"); lastSecond=sec; }
                return;
            }
            model.Tick(Mathf.Min(Time.deltaTime,.0333f),controls.Read(0),controls.Read(1));
            foreach (string sound in model.sounds) Sound(sound);
            if (model.Finished) resultTime=0;
            int second=Mathf.CeilToInt(model.remaining);
            if (second<=10 && second!=lastSecond) { Sound("bell"); lastSecond=second; }
        }
        void SelectKeys(int p)
        {
            if (ready[p]) return;
            int old=selected[p]; var direction=controls.Navigate(p);
            if(direction.x<0) selected[p]=(selected[p]+7)%8;
            if(direction.x>0) selected[p]=(selected[p]+1)%8;
            if(direction.y!=0) selected[p]=(selected[p]+4)%8;
            if(old!=selected[p]) Sound("select");
            if(controls.Press(p,"Confirm")) { ready[p]=true; Sound("confirm"); }
        }
        void OpenSelect() { screen=ScreenState.Select; ready[0]=ready[1]=false; paused=false; help=false; PlayMusic("Menu"); Sound("confirm"); }
        void OpenStageSelect() { screen=ScreenState.StageSelect; help=false; Sound("confirm"); }
        void StartMatch() { model=new DuelModel(selected[0],selected[1],selectedArena); screen=ScreenState.Fight; help=false; BeginRound(); }
        void BeginRound() { countdown=3; lastSecond=3; resultTime=0; Sound("bell"); PlayMusic(Roster.StageFiles[model.ArenaIndex]); }
        void OnGUI()
        {
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            if (textStyle==null) textStyle=new GUIStyle(GUI.skin.label);
            textStyle.font=font; textStyle.fontStyle=FontStyle.Normal;
            Box(0,0,1280,720,ink);
            if (screen==ScreenState.Title) DrawTitle();
            else if (screen==ScreenState.Select) DrawSelect();
            else if (screen==ScreenState.StageSelect) DrawStageSelect();
            else DrawFight();
            if (screen!=ScreenState.Title && screen!=ScreenState.Fight) Label(30,690,800,22,"H  CONTROLS     M  "+(muted?"SOUND OFF":"SOUND ON")+"     KEYBOARD / XBOX / PS4",16,new Color(.62f,.63f,.71f));
            if (help) DrawHelp();
        }
        void Box(float x,float y,float w,float h,Color color)
        { GUI.color=color; GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture); GUI.color=Color.white; }
        void Label(float x,float y,float w,float h,string value,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft)
        {
            textStyle.fontSize=size; textStyle.normal.textColor=color; textStyle.alignment=align; textStyle.wordWrap=true;
            GUI.Label(new Rect(x,y,w,h),value,textStyle);
        }
        void Picture(Texture2D tex,Rect rect,Color? tint=null)
        { if (tex==null) return; GUI.color=tint??Color.white; GUI.DrawTexture(rect,tex,ScaleMode.StretchToFill); GUI.color=Color.white; }
        // Nine-slice preserves the illustrated wood/brass corners at every panel size.
        void Ornament(Texture2D texture,Rect rect,float corner=18)
        {
            if (texture==null) { Box(rect.x,rect.y,rect.width,rect.height,ink); return; }
            float c=Mathf.Min(corner,Mathf.Min(rect.width,rect.height)/2);
            float[] dx={rect.x,rect.x+c,rect.xMax-c,rect.xMax};
            float[] dy={rect.y,rect.y+c,rect.yMax-c,rect.yMax};
            float sourceCorner=texture==academyButton?24:48;
            float u=sourceCorner/texture.width,v=sourceCorner/texture.height;
            float[] sx={0,u,1-u,1},sy={0,v,1-v,1};
            for(int row=0;row<3;row++) for(int col=0;col<3;col++)
                GUI.DrawTextureWithTexCoords(new Rect(dx[col],dy[row],dx[col+1]-dx[col],dy[row+1]-dy[row]),texture,
                    new Rect(sx[col],1-sy[row+1],sx[col+1]-sx[col],sy[row+1]-sy[row]),true);
        }
        void Frame(float x,float y,float w,float h) => Ornament(academyPanel,new Rect(x,y,w,h),h<70?8:18);
        bool Button(float x,float y,float w,float h,string value)
        {
            Rect rect=new Rect(x,y,w,h); bool hover=rect.Contains(Event.current.mousePosition);
            GUI.color=hover?new Color(1.18f,1.1f,.95f):Color.white;
            Ornament(academyButton,rect,12); GUI.color=Color.white;
            Label(x,y,w,h,value,h<45?18:22,cream,TextAnchor.MiddleCenter);
            return GUI.Button(rect,GUIContent.none,GUIStyle.none);
        }
        void DrawTitle()
        {
            Picture(stages[2],new Rect(0,0,1280,720)); Box(0,0,1280,720,new Color(.015f,.02f,.055f,.78f));
            Label(0,175,1280,35,"T H E   F O R B I D D E N   T R I A L S",20,gold,TextAnchor.MiddleCenter);
            textStyle.font=titleFont!=null?titleFont:font;
            Label(0,230,1280,120,"HEXBOUND",96,cream,TextAnchor.MiddleCenter);
            Label(0,350,1280,52,"ACADEMY DUEL",29,gold,TextAnchor.MiddleCenter);
            textStyle.font=font;
            Box(490,428,300,1,gold);
            Label(0,488,1280,40,"PRESS ANY KEY OR CLICK TO BEGIN",25,new Color(1,.9f,.7f,.7f+.3f*Mathf.Sin(animationTime*2)),TextAnchor.MiddleCenter);
            Label(0,648,1280,28,"TWO DUELISTS  /  THREE FORBIDDEN ARENAS  /  ONE ACADEMY",17,gold,TextAnchor.MiddleCenter);
        }
        void DrawSelect()
        {
            Picture(stages[2],new Rect(0,0,1280,720)); Box(0,0,1280,720,new Color(.02f,.02f,.06f,.68f));
            // Large opposing portraits and compact central roster follow the reference composition.
            Box(0,0,625,720,new Color(.38f,.06f,.12f,.23f));
            Box(655,0,625,720,new Color(.04f,.16f,.4f,.23f));
            SelectPanel(0,30); SelectPanel(1,930);
            Box(637,118,2,270,new Color(.7f,.6f,.4f,.35f));
            Label(0,24,1280,28,"HEXBOUND / THE DUELING SOCIETY",18,gold,TextAnchor.MiddleCenter);
            textStyle.font=titleFont!=null?titleFont:font;
            Label(0,60,1280,55,"Choose your apprentice",30,cream,TextAnchor.MiddleCenter);
            textStyle.font=font;
            Label(555,246,170,65,"VS",52,gold,TextAnchor.MiddleCenter);
            Frame(396,444,488,202);
            for (int i=0;i<8;i++) {
                float x=408+(i%4)*118, y=455+(i/4)*92;
                Picture(avatars[i],new Rect(x+18,y+1,70,70));
                Label(x,y+69,108,20,Roster.ShortNames[i],15,cream,TextAnchor.MiddleCenter);
                for(int p=0;p<2;p++) if(selected[p]==i) {
                    Color c=p==0?new Color(.4f,.85f,1):new Color(1,.4f,.55f);
                    Box(x,y+p*87,108,3,c); Box(x+(p==0?0:105),y,3,90,c);
                    Label(x+p*54,y,54,20,"P"+(p+1)+(ready[p]?" OK":""),15,c,TextAnchor.MiddleCenter);
                }
                if (GUI.Button(new Rect(x,y,108,90),GUIContent.none,GUIStyle.none)) {
                    int p=ready[0]?1:0; if (!ready[p]) { selected[p]=i; Sound("select"); }
                }
            }
            Label(390,650,500,28,"BOTH LOCK IN TO CHOOSE AN ARENA / ESC UNLOCKS",16,gold,TextAnchor.MiddleCenter);
        }
        void SelectPanel(int p,float x)
        {
            int id=selected[p]; Color c=p==0?new Color(.4f,.85f,1):new Color(1,.4f,.55f);
            // The original high resolution pixel concept is used for the full-height showcase.
            Rect art=new Rect(p==0?70:700,112,510,510);
            if(showcases[id]!=null) {
                Rect uv=new Rect(p==0?0:1,0,p==0?1:-1,1);
                GUI.DrawTextureWithTexCoords(art,showcases[id],uv,true);
            } else DrawSprite(id,Pose.Idle,(int)(animationTime*6)%4,art,p==0?1:-1);
            float y=365;
            Box(x,y,320,84,new Color(.025f,.025f,.06f,.78f));
            Box(x,y,320,2,c);
            Label(x,y+5,320,26,"PLAYER "+(p+1)+(ready[p]?" / OATH SEALED":" / SELECTING"),19,c,p==0?TextAnchor.MiddleLeft:TextAnchor.MiddleRight);
            Label(x,y+34,320,43,Roster.ShortNames[id],38,cream,p==0?TextAnchor.MiddleLeft:TextAnchor.MiddleRight);
            Frame(x,460,320,147);
            Label(x+14,470,292,25,Roster.Names[id],20,cream);
            Label(x+14,498,292,24,Roster.Titles[id],17,Roster.Colors[id]);
            Label(x+14,527,292,42,Roster.Powers[id]+" / "+Roster.Weapons[id],18,cream);
            Label(x+14,574,292,25,controls.Prompt(p,"Move")+" SELECT / "+controls.Prompt(p,"Confirm")+" LOCK",16,gold);
            if (Button(x,620,320,40,ready[p]?"UNSEAL OATH":"SEAL OATH / LOCK IN")) { ready[p]=!ready[p]; Sound("confirm"); }
        }
        void DrawStageSelect()
        {
            Picture(stages[selectedArena],new Rect(0,0,1280,720)); Box(0,0,1280,720,new Color(.02f,.025f,.06f,.72f));
            Label(0,55,1280,65,"CHOOSE THE OPENING ARENA",42,cream,TextAnchor.MiddleCenter);
            Label(0,128,1280,30,"THREE ARENAS / 99 SECONDS EACH / ONLY RIFTS ELIMINATE",20,gold,TextAnchor.MiddleCenter);
            for(int i=0;i<3;i++) {
                float x=55+i*400; Frame(x,240,370,266);
                Picture(stages[i],new Rect(x+10,250,350,197));
                if(selectedArena==i) Box(x,236,370,4,gold);
                Label(x+10,450,350,24,Roster.Stages[i],20,cream,TextAnchor.MiddleCenter);
                Label(x+10,478,350,22,Roster.LightModes[i],18,gold,TextAnchor.MiddleCenter);
                if(GUI.Button(new Rect(x,240,370,266),GUIContent.none,GUIStyle.none)) { selectedArena=i; Sound("select"); }
            }
            if(Button(440,555,400,54,"BEGIN THE DUEL / ENTER")) StartMatch();
            Label(0,625,1280,28,"STICK / D-PAD / ARROWS SELECT / SOUTH BUTTON OR ENTER CONFIRM / EAST BUTTON BACK",18,gold,TextAnchor.MiddleCenter);
        }
        float GroundY => CombatHudLayout.GroundY;
        void DrawFight()
        {
            Picture(stages[model.ArenaIndex],new Rect(0,0,1280,720));
            // Ground corresponds to the stage art's upper stone walkway.
            foreach (float hole in new[]{DuelModel.LeftRiftX,DuelModel.RightRiftX}) {
                DrawRift(hole);

            }
            for (int i=0;i<2;i++) {
                var p=model.players[i];
                Box(p.x-33,GroundY,66,6,new Color(0,0,0,.4f));
                int frame=p.pose==Pose.Knockout?Mathf.Min(3,(int)((p.fallen?resultTime:p.knockdownElapsed)*8)):p.actionTime>0?Mathf.Clamp((int)((1-p.actionTime/p.actionLength)*4),0,3):(int)(animationTime*8)%4;
                if (!p.fallen || resultTime<.8f) {
                    float fall=p.fallen?Mathf.Clamp01(resultTime/.8f):0;
                    float size=192*(1-fall*.8f);
                    DrawSprite(p.character,p.pose,frame,new Rect(p.x-size/2,GroundY+18-p.height-size+fall*40,size,size),p.facing);
                }
                if(!p.fallen && p.actionTime>0 && (p.pose==Pose.Special || p.pose==Pose.Power))
                    DrawEffect(p.character,p.x+p.facing*38,GroundY-87-p.height,p.pose==Pose.Power?136:88,(int)(animationTime*16)%8);
                if (!p.fallen && (p.IsKnockedDown || p.blocking)) Label(p.x-50,GroundY-207-p.height,100,25,p.IsKnockedDown?"DOWN":"GUARD",18,i==0?new Color(.45f,.85f,1):new Color(1,.5f,.6f),TextAnchor.MiddleCenter);
                if (p.combo>1 && p.comboWindow>0) Label(p.x-65,GroundY-240-p.height,130,28,p.combo+" HIT CHAIN",20,gold,TextAnchor.MiddleCenter);
            }
            foreach (var spell in model.spells) DrawEffect(spell.character,spell.x,GroundY-77-spell.height,spell.ultimate?144:86,(int)(animationTime*18)%8);
            foreach (var burst in model.bursts) DrawEffect(burst.character,burst.x,GroundY-77-burst.height,burst.ultimate?160:110,Mathf.Clamp((int)((.45f-burst.life)/.45f*8),0,7));
            DrawHud(0); DrawHud(1);
            Frame(579,18,122,110);
            Label(579,21,122,25,"VS",17,gold,TextAnchor.MiddleCenter);
            Label(579,42,122,59,Mathf.CeilToInt(model.remaining).ToString("00"),54,model.remaining<=10?new Color(1,.4f,.4f):cream,TextAnchor.MiddleCenter);
            Label(579,99,122,23,"ROUND "+model.round+" / 3",16,gold,TextAnchor.MiddleCenter);
            DrawControlCluster(0); DrawControlCluster(1);
            if (countdown>0) Overlay(Mathf.CeilToInt(countdown).ToString(),"PREPARE YOUR SPELLS",false);
            else if (model.Finished) {
                string result=model.winner<0?"ROUND DRAW":Roster.Names[model.players[model.winner].character]+" WINS";
                if (model.MatchFinished) {
                    int a=model.players[0].wins,b=model.players[1].wins;
                    result=a==b?"MATCH DRAW":"PLAYER "+(a>b?1:2)+" / ACADEMY CHAMPION";
                }
                Overlay(result,model.reason+"  /  SCORE "+model.players[0].wins+" : "+model.players[1].wins,false);
                if (resultTime>1.2f && Button(460,430,360,48,model.MatchFinished?"CHOOSE AGAIN / ENTER":"NEXT ARENA / ENTER")) {
                    if (model.MatchFinished) OpenSelect(); else { model.NextRound(); BeginRound(); }
                }
            }
            if (paused) {
                Overlay("DUEL PAUSED","ESC TO RESUME",false);
                if (Button(460,430,360,48,"RETURN TO CHARACTER SELECT")) OpenSelect();
            }
        }
        void DrawHud(int side)
        {
            var p=model.players[side]; bool left=side==0; Color accent=Roster.Colors[p.character];
            float x=left?22:714, head=left?25:1163, bar=left?119:722;
            Frame(x,20,544,93);
            int reaction=p.IsKnockedDown||p.fallen?2:p.stun>0?1:model.Finished&&model.winner==side?3:0;
            if(portraits[p.character]!=null) GUI.DrawTextureWithTexCoords(new Rect(head+4,27,82,82),portraits[p.character],new Rect((reaction%2)*.5f,1-(reaction/2+1)*.5f,.5f,.5f),true);
            Label(bar,27,438,25,Roster.Names[p.character],22,cream,left?TextAnchor.MiddleLeft:TextAnchor.MiddleRight);
            int remaining=p.IsKnockedDown||p.fallen?0:DuelModel.KnockdownHits-p.hitsTowardKnockdown;
            for(int i=0;i<DuelModel.KnockdownHits;i++) {
                float sx=bar+(left?i:4-i)*87;
                Box(sx,58,82,22,gold); Box(sx+2,60,78,18,i<remaining?accent:new Color(.09f,.08f,.13f));
                if(i<remaining) Box(sx+4,62,74,3,new Color(1,1,1,.4f));
            }
            Box(bar,84,433,5,new Color(.12f,.13f,.2f));
            Box(left?bar:bar+433-433*p.mana/100,84,433*p.mana/100,5,new Color(.45f,.65f,1));
            for(int i=0;i<3;i++) Picture(runeDisc,new Rect(left?bar+360+i*24:bar+48-i*24,6,18,18),i<p.wins?gold:new Color(.22f,.23f,.29f));
        }
        static Texture2D CreateRuneDisc()
        {
            var t=new Texture2D(96,96,TextureFormat.RGBA32,false); t.filterMode=FilterMode.Point;
            for(int y=0;y<96;y++) for(int x=0;x<96;x++) {
                float r=Vector2.Distance(new Vector2(x,y),new Vector2(47.5f,47.5f));
                t.SetPixel(x,y,r>47?Color.clear:r>43||r>37&&r<39?Color.white:new Color(.075f,.09f,.15f,.94f));
            }
            t.Apply(); return t;
        }
        void DrawControlCluster(int side)
        {
            var fighter=model.players[side];
            string[] actions={"Punch","Push","Spell","Power"};
            var input=controls.Read(side);
            bool[] active={input.punch,input.push,input.special,input.power};
            for(int i=0;i<4;i++) {
                Rect rect=CombatHudLayout.Ability(side,i);
                bool affordable=i<2 || fighter.mana>=(i==2?20:60);
                GUI.color=active[i]?new Color(1.4f,1.4f,1.4f):affordable?Color.white:new Color(.4f,.4f,.48f);
                if(abilityIcons[fighter.character]!=null) GUI.DrawTextureWithTexCoords(rect,abilityIcons[fighter.character],new Rect((i%2)*.5f,1-(i/2+1)*.5f,.5f,.5f),true);
                GUI.color=Color.white;
                string key=controls.Prompt(side,actions[i]).Replace("TRIANGLE","TRI").Replace("SQUARE","SQ").Replace("CIRCLE","O").Replace("CROSS","X");
                Box(rect.x+8,rect.y+34,32,14,new Color(.02f,.025f,.045f,.85f));
                Label(rect.x+8,rect.y+34,32,14,key,12,cream,TextAnchor.MiddleCenter);
            }
        }
        void OnDestroy() { controls?.Dispose(); if(runeDisc!=null) Destroy(runeDisc); }
        void DrawSprite(int id,Pose pose,int frame,Rect rect,int facing)
        {
            if (sheets[id]==null) return;
            Rect uv=new Rect(frame/4f,1-((int)pose+1)/10f,.25f,.1f);
            if (facing<0) { uv.x+=uv.width; uv.width=-uv.width; }
            GUI.DrawTextureWithTexCoords(rect,sheets[id],uv,true);
        }
        void DrawRift(float x)
        {
            if (riftSheet==null) { Box(x-36,GroundY-8,72,16,Color.black); return; }
            int frame=(int)(animationTime*6)%4;
            GUI.DrawTextureWithTexCoords(CombatHudLayout.Portal(x),riftSheet,
                new Rect((frame%2)/2f,1-(frame/2+1)/2f,.5f,.5f),true);
        }
        void DrawEffect(int id,float x,float y,float size,int frame)
        { if (effects[id]!=null) GUI.DrawTextureWithTexCoords(new Rect(x-size/2,y-size/2,size,size),effects[id],new Rect((frame%4)/4f,1-(frame/4+1)/2f,.25f,.5f),true); }
        void Overlay(string title,string subtitle,bool unused)
        {
            Frame(220,260,840,148);
            Label(235,275,810,67,title,42,cream,TextAnchor.MiddleCenter);
            Label(235,346,810,40,subtitle,21,gold,TextAnchor.MiddleCenter);
        }
        void DrawHelp()
        {
            Frame(140,120,1000,525);
            Label(180,135,920,52,"THE DUELIST'S HANDBOOK",35,cream);
            Label(180,195,920,370,"P1: A / D move, W jump, S guard, F punch, G shove, R spell, T power.\nP2: Left / Right move, Up jump, Down guard, J punch, K shove, U spell, I power.\n\nTap punch three times with a short pause between strikes to finish a combo.\nSpells cost 20 arcana; power attacks cost 60. Arcana regenerates.\nFive unguarded hits knock a duelist down temporarily, never eliminate.\nEvery attack knocks back. Drive your rival into a rune rift at either stage edge.\nPlay all 3 arenas. 99 seconds per round. No ring out means a draw. Most round wins takes the match.\nXbox: stick/D-pad move, A jump, LB guard, X punch, B push, Y spell, RB power.\nPS4: Cross jump, L1 guard, Square punch, Circle push, Triangle spell, R1 power.\nMenu/Options pauses; View/Share or H closes help. M mutes.",20,cream);
            if (Button(455,580,370,45,"CLOSE HANDBOOK")) help=false;
        }
        void OnApplicationFocus(bool focus) { if (!focus && screen==ScreenState.Fight && model!=null && !model.Finished) paused=true; }
#if UNITY_EDITOR
        public void CapturePreview(string path) { StartCoroutine(CaptureFrame(path)); }
        System.Collections.IEnumerator CaptureFrame(string path)
        {
            yield return new WaitForEndOfFrame();
            var texture=ScreenCapture.CaptureScreenshotAsTexture();
            System.IO.File.WriteAllBytes(path,texture.EncodeToPNG());
            Destroy(texture);
        }
#endif
    }
}
