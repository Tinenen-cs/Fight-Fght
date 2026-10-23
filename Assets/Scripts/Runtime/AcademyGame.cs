using UnityEngine;
using System.Collections.Generic;

namespace Hexbound
{
    public sealed class AcademyGame : MonoBehaviour
    {
        enum ScreenState { Title, Select, Fight }
        ScreenState screen;
        readonly int[] selected={0,4};
        readonly bool[] ready={false,false};
        Texture2D[] sheets=new Texture2D[8], avatars=new Texture2D[8], effects=new Texture2D[8], stages=new Texture2D[3];
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        AudioSource music, sfx;
        DuelModel model;
        float countdown, resultTime, animationTime;
        bool paused, help, muted;
        int lastSecond;
        Font font;
        GUIStyle textStyle;
        readonly Color ink=new Color32(13,16,30,255), cream=new Color32(242,227,201,255), gold=new Color32(211,176,109,255);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { if (FindFirstObjectByType<AcademyGame>()==null) new GameObject("Hexbound Game").AddComponent<AcademyGame>(); }
        void Awake()
        {
            Application.targetFrameRate=60;
            font=Resources.Load<Font>("UI/Tiny5");
            for (int i=0;i<8;i++) {
                sheets[i]=Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/sheet");
                avatars[i]=Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/avatar");
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
            if (Key(KeyCode.M)) { muted=!muted; AudioListener.volume=muted?0:1; PlayerPrefs.SetInt("Hexbound.Mute",muted?1:0); }
            if (Key(KeyCode.H)) help=!help;
            if (Key(KeyCode.Escape) && screen==ScreenState.Fight && !model.Finished) paused=!paused;
            if (!paused) animationTime+=Time.deltaTime;
            if (screen==ScreenState.Title) { if (Key(KeyCode.Return)) OpenSelect(); return; }
            if (screen==ScreenState.Select) {
                SelectKeys(0,KeyCode.A,KeyCode.D,KeyCode.W,KeyCode.S,KeyCode.F);
                SelectKeys(1,KeyCode.LeftArrow,KeyCode.RightArrow,KeyCode.UpArrow,KeyCode.DownArrow,KeyCode.J);
                if (Key(KeyCode.Escape)) { ready[0]=ready[1]=false; Sound("select"); }
                if (ready[0] && ready[1]) StartMatch();
                return;
            }
            if (paused) return;
            if (model.Finished) {
                resultTime+=Time.deltaTime;
                if (resultTime>1.2f && Key(KeyCode.Return)) {
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
            model.Tick(Mathf.Min(Time.deltaTime,.0333f),ReadInput(0),ReadInput(1));
            foreach (string sound in model.sounds) Sound(sound);
            if (model.Finished) resultTime=0;
            int second=Mathf.CeilToInt(model.remaining);
            if (second<=10 && second!=lastSecond) { Sound("bell"); lastSecond=second; }
        }
        void SelectKeys(int p,KeyCode left,KeyCode right,KeyCode up,KeyCode down,KeyCode confirm)
        {
            if (ready[p]) return;
            int old=selected[p];
            if (Key(left)) selected[p]=(selected[p]+7)%8;
            if (Key(right)) selected[p]=(selected[p]+1)%8;
            if (Key(up)||Key(down)) selected[p]=(selected[p]+4)%8;
            if (old!=selected[p]) Sound("select");
            if (Key(confirm)) { ready[p]=true; Sound("confirm"); }
        }
        DuelInput ReadInput(int p)
        {
            return new DuelInput {
                move=(Input.GetKey(p==0?KeyCode.D:KeyCode.RightArrow)?1:0)-(Input.GetKey(p==0?KeyCode.A:KeyCode.LeftArrow)?1:0),
                jump=Key(p==0?KeyCode.W:KeyCode.UpArrow),block=Input.GetKey(p==0?KeyCode.S:KeyCode.DownArrow),
                punch=Key(p==0?KeyCode.F:KeyCode.J),push=Key(p==0?KeyCode.G:KeyCode.K),
                special=Key(p==0?KeyCode.R:KeyCode.U),power=Key(p==0?KeyCode.T:KeyCode.I)
            };
        }
        void OpenSelect() { screen=ScreenState.Select; ready[0]=ready[1]=false; paused=false; help=false; PlayMusic("Menu"); Sound("confirm"); }
        void StartMatch() { model=new DuelModel(selected[0],selected[1]); screen=ScreenState.Fight; help=false; BeginRound(); }
        void BeginRound() { countdown=3; lastSecond=3; resultTime=0; Sound("bell"); PlayMusic(Roster.StageFiles[model.round-1]); }
        void OnGUI()
        {
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,Vector3.one*scale);
            if (textStyle==null) textStyle=new GUIStyle(GUI.skin.label);
            textStyle.font=font; textStyle.fontStyle=FontStyle.Normal;
            Box(0,0,1280,720,ink);
            if (screen==ScreenState.Title) DrawTitle();
            else if (screen==ScreenState.Select) DrawSelect();
            else DrawFight();
            Label(30,690,800,22,"H  CONTROLS     M  "+(muted?"SOUND OFF":"SOUND ON")+"     LOCAL TWO-PLAYER",16,new Color(.62f,.63f,.71f));
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
        bool Button(float x,float y,float w,float h,string value)
        {
            Rect rect=new Rect(x,y,w,h); bool hover=rect.Contains(Event.current.mousePosition);
            Box(x,y,w,h,gold); Box(x+2,y+2,w-4,h-4,hover?new Color(.24f,.20f,.29f):ink);
            Label(x,y,w,h,value,22,cream,TextAnchor.MiddleCenter);
            return GUI.Button(rect,GUIContent.none,GUIStyle.none);
        }
        void DrawTitle()
        {
            Picture(stages[0],new Rect(0,0,1280,720)); Box(0,0,1280,720,new Color(.025f,.035f,.075f,.72f));
            Label(0,72,1280,30,"A DUEL AT THE EDGE OF THE FORBIDDEN",20,gold,TextAnchor.MiddleCenter);
            Label(0,132,1280,105,"HEXBOUND",104,cream,TextAnchor.MiddleCenter);
            Label(0,239,1280,55,"A C A D E M Y   D U E L",30,gold,TextAnchor.MiddleCenter);
            Box(465,315,350,2,gold);
            Label(330,345,620,74,"Eight apprentices. Three forbidden arenas.\nOne open drain can change everything.",24,cream,TextAnchor.MiddleCenter);
            for (int i=0;i<8;i++) DrawSprite(i,Pose.Idle,Mathf.FloorToInt(animationTime*6)%4,new Rect(280+i*90,439,90,90),i<4?1:-1);
            if (Button(465,555,350,58,"ENTER THE ACADEMY  /  ENTER")) OpenSelect();
            Label(0,627,1280,30,"2 PLAYERS  /  SHARED KEYBOARD  /  99 SECONDS",18,gold,TextAnchor.MiddleCenter);
        }
        void DrawSelect()
        {
            Picture(stages[1],new Rect(0,0,1280,720)); Box(0,0,1280,720,new Color(.025f,.035f,.075f,.89f));
            Label(40,24,400,26,"HEXBOUND / THE DUELING SOCIETY",18,gold);
            Label(0,60,1280,66,"CHOOSE YOUR APPRENTICE",46,cream,TextAnchor.MiddleCenter);
            Label(0,129,1280,25,"FOUR WITCHES  /  FOUR WIZARDS  /  EIGHT FORBIDDEN ARTS",18,gold,TextAnchor.MiddleCenter);
            SelectPanel(0,35); SelectPanel(1,945);
            Label(590,215,100,48,"VS",40,gold,TextAnchor.MiddleCenter);
            for (int i=0;i<8;i++) {
                float x=355+(i%4)*145, y=325+(i/4)*143;
                Color border=selected[0]==i?new Color(.35f,.8f,1):selected[1]==i?new Color(1,.4f,.55f):new Color(.25f,.26f,.36f);
                Box(x,y,135,133,border); Box(x+2,y+2,131,129,ink);
                Picture(avatars[i],new Rect(x+27,y+5,80,80));
                Label(x,y+84,135,23,Roster.Names[i],20,cream,TextAnchor.MiddleCenter);
                string tags=(selected[0]==i?"P1 ":"")+(selected[1]==i?"P2":"");
                Label(x,y+108,135,20,tags,16,border,TextAnchor.MiddleCenter);
                if (GUI.Button(new Rect(x,y,135,133),GUIContent.none,GUIStyle.none)) {
                    int p=ready[0]?1:0; if (!ready[p]) { selected[p]=i; Sound("select"); }
                }
            }
            Label(340,632,600,34,"BOTH PLAYERS LOCK IN TO BEGIN  /  ESC TO UNLOCK",18,gold,TextAnchor.MiddleCenter);
        }
        void SelectPanel(int p,float x)
        {
            int id=selected[p]; Color c=Roster.Colors[id];
            Box(x,182,300,432,new Color(.06f,.07f,.12f)); Box(x,182,300,3,c);
            Label(x+15,192,270,30,"PLAYER "+(p+1)+(ready[p]?"  /  READY":"  /  SELECTING"),21,c);
            DrawSprite(id,Pose.Idle,Mathf.FloorToInt(animationTime*6)%4,new Rect(x+48,224,204,204),p==0?1:-1);
            Label(x+15,418,270,43,Roster.Names[id],40,cream);
            Label(x+15,463,270,28,Roster.Titles[id],18,c);
            Label(x+15,495,270,44,Roster.Powers[id],19,cream);
            Label(x+15,539,270,23,p==0?"A/D/W/S  SELECT   F  LOCK":"ARROWS  SELECT   J  LOCK",16,gold);
            if (Button(x+15,570,270,34,ready[p]?"UNLOCK":"LOCK IN")) { ready[p]=!ready[p]; Sound("confirm"); }
        }
        void DrawFight()
        {
            Picture(stages[model.round-1],new Rect(0,0,1280,720));
            // Ground corresponds to the stage art's upper stone walkway.
            float hole=model.HoleX;
            Box(hole-43,554,86,15,new Color(.13f,.14f,.18f));
            Box(hole-33,557,66,9,model.HoleOpen?Color.black:new Color(.35f,.35f,.42f));
            for (int i=0;i<6 && !model.HoleOpen;i++) Box(hole-29+i*11,558,3,7,ink);
            if (model.HoleOpen || model.HoleWarning) {
                Color warning=model.HoleOpen?new Color(1,.36f,.4f):gold;
                Label(hole-110,588,220,26,model.HoleOpen?"OPEN / RING OUT":"DRAIN OPENING",18,warning,TextAnchor.MiddleCenter);
                Box(hole-48,550,8,5,warning); Box(hole+40,550,8,5,warning);
            }
            for (int i=0;i<2;i++) {
                var p=model.players[i];
                Box(p.x-33,557,66,6,new Color(0,0,0,.4f));
                int frame=p.pose==Pose.Knockout?Mathf.Min(3,(int)(resultTime*8)):p.actionTime>0?Mathf.Clamp((int)((1-p.actionTime/p.actionLength)*4),0,3):(int)(animationTime*8)%4;
                DrawSprite(p.character,p.pose,frame,new Rect(p.x-96,570-p.height-192,192,192),p.facing);
                Label(p.x-50,350-p.height,100,25,"P"+(i+1)+(p.blocking?" GUARD":""),18,i==0?new Color(.45f,.85f,1):new Color(1,.5f,.6f),TextAnchor.MiddleCenter);
                if (p.combo>1 && p.comboWindow>0) Label(p.x-65,317-p.height,130,28,p.combo+" HIT CHAIN",20,gold,TextAnchor.MiddleCenter);
            }
            foreach (var spell in model.spells) DrawEffect(spell.character,spell.x,480-spell.height,spell.ultimate?144:86,(int)(animationTime*18)%8);
            foreach (var burst in model.bursts) DrawEffect(burst.character,burst.x,480-burst.height,burst.ultimate?160:110,Mathf.Clamp((int)((.45f-burst.life)/.45f*8),0,7));
            DrawHud(0); DrawHud(1);
            Box(577,20,126,99,ink); Box(577,20,126,3,gold);
            Label(577,22,126,66,Mathf.CeilToInt(model.remaining).ToString("00"),54,model.remaining<=10?new Color(1,.4f,.4f):cream,TextAnchor.MiddleCenter);
            Label(577,87,126,25,"ROUND "+model.round+" / 3",16,gold,TextAnchor.MiddleCenter);
            Label(0,133,1280,30,Roster.Stages[model.round-1],20,cream,TextAnchor.MiddleCenter);
            Box(0,639,1280,42,new Color(.035f,.04f,.075f,.92f));
            Label(30,645,570,27,"P1   A/D MOVE   W JUMP   S GUARD   F PUNCH   G PUSH   R SPELL   T POWER",16,cream);
            Label(655,645,610,27,"P2   ARROWS MOVE/JUMP/GUARD   J PUNCH   K PUSH   U SPELL   I POWER",16,cream);
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
            var p=model.players[side]; float x=side==0?30:735; Color accent=Roster.Colors[p.character];
            Box(x,20,515,100,new Color(.04f,.045f,.08f,.95f));
            float avatarX=side==0?x:x+435; Picture(avatars[p.character],new Rect(avatarX,26,80,80));
            float bx=side==0?x+90:x+10;
            Label(bx,22,415,25,"P"+(side+1)+"  "+Roster.Names[p.character],23,cream,side==0?TextAnchor.MiddleLeft:TextAnchor.MiddleRight);
            Box(bx,53,415,22,new Color(.27f,.13f,.2f));
            float hp=415*p.health/100; Box(side==0?bx:bx+415-hp,53,hp,22,accent);
            Label(bx,51,415,24,Mathf.CeilToInt(p.health)+" / 100",16,ink,TextAnchor.MiddleCenter);
            Box(bx,81,415,7,new Color(.18f,.19f,.28f)); Box(side==0?bx:bx+415-415*p.mana/100,81,415*p.mana/100,7,new Color(.5f,.55f,1));
            Label(bx,91,415,24,"WINS "+p.wins+"     ARCANA "+(int)p.mana+" / 100"+(p.mana>=60?"   POWER READY":""),16,gold);
        }
        void DrawSprite(int id,Pose pose,int frame,Rect rect,int facing)
        {
            if (sheets[id]==null) return;
            Rect uv=new Rect(frame/4f,1-((int)pose+1)/10f,.25f,.1f);
            if (facing<0) { uv.x+=uv.width; uv.width=-uv.width; }
            GUI.DrawTextureWithTexCoords(rect,sheets[id],uv,true);
        }
        void DrawEffect(int id,float x,float y,float size,int frame)
        { if (effects[id]!=null) GUI.DrawTextureWithTexCoords(new Rect(x-size/2,y-size/2,size,size),effects[id],new Rect(frame/8f,0,.125f,1),true); }
        void Overlay(string title,string subtitle,bool unused)
        {
            Box(220,260,840,148,new Color(.035f,.04f,.075f,.95f)); Box(220,260,840,3,gold);
            Label(235,275,810,67,title,42,cream,TextAnchor.MiddleCenter);
            Label(235,346,810,40,subtitle,21,gold,TextAnchor.MiddleCenter);
        }
        void DrawHelp()
        {
            Box(180,160,920,420,new Color(.035f,.04f,.075f,.98f)); Box(180,160,920,3,gold);
            Label(220,180,840,52,"THE DUELIST'S HANDBOOK",35,cream);
            Label(220,245,840,245,"P1: A / D move, W jump, S guard, F punch, G shove, R spell, T power.\nP2: Left / Right move, Up jump, Down guard, J punch, K shove, U spell, I power.\n\nTap punch three times with a short pause between strikes to finish a combo.\nSpells cost 20 arcana; power attacks cost 60. Arcana regenerates.\nWatch the drain warning. Jump across an open manhole; shove your rival in.\nPlay all 3 arenas. Each round lasts 99 seconds. Most round wins takes the match.\nESC pauses. M mutes. H closes this panel.",23,cream);
            if (Button(455,510,370,45,"CLOSE HANDBOOK")) help=false;
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
