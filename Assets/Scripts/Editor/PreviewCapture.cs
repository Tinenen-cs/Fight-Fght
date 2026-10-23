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
            if (step==1) game.CapturePreview("Docs/Previews/01-title.png");
            if (step==2) Invoke(game,"OpenSelect");
            if (step==3) game.CapturePreview("Docs/Previews/02-select.png");
            if (step==4) Invoke(game,"StartMatch");
            if (step==7) game.CapturePreview("Docs/Previews/03-fight.png");
            if (step==8) {
                var model=(DuelModel)typeof(AcademyGame).GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                if (model==null) throw new Exception("No runtime model");
                Debug.Log("HEXBOUND RUNTIME SMOKE PASSED: title, selection, fight rendered");
                SessionState.SetBool("Hexbound.Capture",false); EditorApplication.update-=Update;
                EditorApplication.Exit(0);
            }
            step++;
        }
    }
}
