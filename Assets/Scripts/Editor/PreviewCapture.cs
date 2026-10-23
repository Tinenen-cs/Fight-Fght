using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System;
using System.IO;
using System.Reflection;
namespace Hexbound.Editor
{
    // Optional automated visual smoke pass. Run in a graphical Editor session.
    [InitializeOnLoad]
    public static class PreviewCapture
    {
        static double next;
        static int step;
        static Keyboard startKeyboard;
        static PreviewCapture() {
            if (SessionState.GetBool("Hexbound.Capture",false)) EditorApplication.update+=Update;
        }
        public static void Start()
        {
            ProjectSetup.OpenScene();
            SessionState.SetBool("Hexbound.Capture",true);
            EditorApplication.update-=Update; EditorApplication.update+=Update;
            EditorApplication.isPlaying=true;
        }
        static void Invoke(AcademyGame game,string method) => typeof(AcademyGame).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
        static void Update()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            if (EditorApplication.timeSinceStartup<next) return;
            var game=UnityEngine.Object.FindFirstObjectByType<AcademyGame>(); if (game==null) return;
            next=EditorApplication.timeSinceStartup+2;
            var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            var window=EditorWindow.GetWindow(type); window.position=new Rect(0,30,960,600); window.Focus(); window.Repaint();
            Directory.CreateDirectory("Docs/Previews");
            if (step==0) {
                var preview=UnityEngine.Object.FindFirstObjectByType<EditorScenePreview>(FindObjectsInactive.Include);
                if(preview!=null && preview.gameObject.activeSelf) throw new Exception("Edit preview remained active in Play mode");
            }
            if (step==1) game.CapturePreview("Docs/Previews/01-title.png");
            if (step==2) {
                startKeyboard=InputSystem.AddDevice<Keyboard>();
                InputSystem.QueueStateEvent(startKeyboard,new KeyboardState(Key.Space));
            }
            if (step==3) {
                var state=typeof(AcademyGame).GetField("screen",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                if(state.ToString()!="Select") throw new Exception("Keyboard start did not advance title in normal Play mode");
                InputSystem.RemoveDevice(startKeyboard); game.CapturePreview("Docs/Previews/02-select.png");
            }
            if (step==4) {
                var ready=(bool[])typeof(AcademyGame).GetField("ready",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                ready[0]=ready[1]=true;
            }
            if (step==5) {
                var screen=typeof(AcademyGame).GetField("screen",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                if(screen.ToString()!="StageSelect") throw new Exception("Both locks did not advance to stage selection");
                game.CapturePreview("Docs/Previews/08-stage-select.png");
            }
            if (step==6) Invoke(game,"StartMatch");
            if (step==9) game.CapturePreview("Docs/Previews/03-fight.png");
            if(step==10) {
                var effectModel=(DuelModel)typeof(AcademyGame).GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                effectModel.Tick(.016f,new DuelInput { special=true },default(DuelInput));
                for(int i=0;i<16;i++) effectModel.Tick(.016f,default(DuelInput),default(DuelInput));
                if(effectModel.spells.Count==0) throw new Exception("Spell preview did not create a projectile");
                game.CapturePreview("Docs/Previews/10-spell-controls.png");
            }
            if (step==11 || step==14) {
                var model=(DuelModel)typeof(AcademyGame).GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                model.players[1].x=DuelModel.RightRiftX;
                model.Tick(.016f,default(DuelInput),default(DuelInput));
                if (!model.Finished || model.winner!=0) throw new Exception("Ring-out did not finish arena");
                model.NextRound(); Invoke(game,"BeginRound");
                var music=(AudioSource)typeof(AcademyGame).GetField("music",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                if(music.clip==null || music.clip.name!=Roster.StageFiles[model.ArenaIndex]) throw new Exception("Stage music did not switch to the selected arena track");
            }
            if (step==13) game.CapturePreview("Docs/Previews/05-library.png");
            if (step==16) game.CapturePreview("Docs/Previews/06-observatory.png");
            if (step==17) {
                var model=(DuelModel)typeof(AcademyGame).GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                model.players[1].x=DuelModel.RightRiftX;
                model.Tick(.016f,default(DuelInput),default(DuelInput));
                if (!model.MatchFinished || model.players[0].wins!=3) throw new Exception("Three ring-out match failed");
            }
            if (step==19) game.CapturePreview("Docs/Previews/07-ringout-result.png");
            if(step==20) {
                Invoke(game,"StartMatch");
                typeof(AcademyGame).GetField("countdown",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(game,0f);
                var growth=(DuelModel)typeof(AcademyGame).GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                growth.elapsed=50;growth.Tick(.016f,default(DuelInput),default(DuelInput));
                if(growth.RiftArtSize!=172) throw new Exception("Half-time portal art did not grow");
            }
            if(step==21) game.CapturePreview("Docs/Previews/12-half-time-portals.png");
            if(step==22) {
                var growth=(DuelModel)typeof(AcademyGame).GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                growth.elapsed=85;growth.Tick(.016f,default(DuelInput),default(DuelInput));
                if(growth.RiftArtSize!=200) throw new Exception("Final portal art did not grow");
                game.CapturePreview("Docs/Previews/13-final-portals.png");
            }
            if (step==24) {
                var model=(DuelModel)typeof(AcademyGame).GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                if (model==null) throw new Exception("No runtime model");
                Debug.Log("HEXBOUND RUNTIME SMOKE PASSED: academy title, selection, all three arenas with both edge traps, and three-ring-out match result");
                SessionState.SetBool("Hexbound.Capture",false); EditorApplication.update-=Update;
                EditorApplication.Exit(0);
            }
            step++;
        }
    }
}
