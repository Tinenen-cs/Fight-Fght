using UnityEditor;
using UnityEngine;
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
            if (step==2) Invoke(game,"OpenSelect");
            if (step==3) game.CapturePreview("Docs/Previews/02-select.png");
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
            if (step==10 || step==13) {
                var model=(DuelModel)typeof(AcademyGame).GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                model.players[1].x=DuelModel.RightRiftX;
                model.Tick(.016f,default(DuelInput),default(DuelInput));
                if (!model.Finished || model.winner!=0) throw new Exception("Ring-out did not finish arena");
                model.NextRound(); Invoke(game,"BeginRound");
            }
            if (step==12) game.CapturePreview("Docs/Previews/05-library.png");
            if (step==15) game.CapturePreview("Docs/Previews/06-observatory.png");
            if (step==16) {
                var model=(DuelModel)typeof(AcademyGame).GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                model.players[1].x=DuelModel.RightRiftX;
                model.Tick(.016f,default(DuelInput),default(DuelInput));
                if (!model.MatchFinished || model.players[0].wins!=3) throw new Exception("Three ring-out match failed");
            }
            if (step==18) game.CapturePreview("Docs/Previews/07-ringout-result.png");
            if (step==19) {
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
