using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
namespace Hexbound.Editor
{
    public sealed class PixelImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/")) return;
            var t=(TextureImporter)assetImporter;
            t.textureType=TextureImporterType.Default;
            t.filterMode=FilterMode.Point; t.textureCompression=TextureImporterCompression.Uncompressed;
            t.mipmapEnabled=false; t.alphaIsTransparency=true; t.npotScale=TextureImporterNPOTScale.None;
            t.maxTextureSize=2048; t.wrapMode=TextureWrapMode.Clamp;
        }
    }
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        static ProjectSetup() { EditorApplication.delayCall+=EnsureScene; }
        static void EnsureScene()
        {
            if (!File.Exists("Assets/Scenes/Main.unity")) CreateScene();
        }
        [MenuItem("Hexbound/Open Main Scene")]
        public static void OpenScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureScene(); EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        }
        public static void CreateScene()
        {
            // Additive construction preserves any open scene and unsaved user work.
            var previous=SceneManager.GetActiveScene();
            if (previous.isDirty && string.IsNullOrEmpty(previous.path)) {
                Debug.LogWarning("Save your untitled scene, then use Hexbound/Open Main Scene to finish setup."); return;
            }
            bool replaceEmpty=string.IsNullOrEmpty(previous.path);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,replaceEmpty?NewSceneMode.Single:NewSceneMode.Additive);
            var camera=new GameObject("Main Camera"); SceneManager.MoveGameObjectToScene(camera,scene);
            camera.tag="MainCamera";
            var cam=camera.AddComponent<Camera>(); cam.orthographic=true; cam.backgroundColor=new Color(.03f,.04f,.08f);
            cam.clearFlags=CameraClearFlags.SolidColor; camera.transform.position=new Vector3(0,0,-10);
            camera.AddComponent<AudioListener>();
            var game=new GameObject("Hexbound Game"); SceneManager.MoveGameObjectToScene(game,scene); game.AddComponent<AcademyGame>();
            Directory.CreateDirectory("Assets/Scenes"); EditorSceneManager.SaveScene(scene,"Assets/Scenes/Main.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Main.unity",true)};
            if (!replaceEmpty) {
                if (previous.IsValid()) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene,true);
            }
            PlayerSettings.companyName="Student Arcana Studio"; PlayerSettings.productName="Hexbound - Academy Duel";
            PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=720;
            PlayerSettings.runInBackground=false;
            AssetDatabase.SaveAssets();
        }
        public static void ValidateBatch()
        {
            EnsureScene();
            RuleChecks.Run();
            for (int i=0;i<8;i++) {
                Require(Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/sheet")!=null,"sprite "+i);
                Require(Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/avatar")!=null,"avatar "+i);
                Require(Resources.Load<Texture2D>("Effects/"+Roster.Files[i])!=null,"effect "+i);
                var sheet=Resources.Load<Texture2D>("Characters/"+Roster.Files[i]+"/sheet");
                Require(sheet.width==512 && sheet.height==1280,"128-pixel fighter atlas "+i);
            }
            Require(Resources.Load<Texture2D>("Stages/RuneRift")!=null,"rune rift");
            Require(Resources.Load<Texture2D>("UI/AcademyPanel")!=null,"academy panel skin");
            Require(Resources.Load<Texture2D>("UI/AcademyButton")!=null,"academy button skin");
            Require(Resources.LoadAll<AudioClip>("Audio").Length==16,"16 audio files");
            Debug.Log("HEXBOUND VALIDATION PASSED: rules and imported assets");
        }
        static void Require(bool pass,string name) { if (!pass) throw new System.Exception("Missing "+name); }
    }
}
