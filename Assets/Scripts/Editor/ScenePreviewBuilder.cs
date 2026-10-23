using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
namespace Hexbound.Editor
{
    public static class ScenePreviewBuilder
    {
        const string RootName="Editor Scene Preview";
        const string SpriteFolder="Assets/Art/UI/EditorPreview";
        [MenuItem("Hexbound/Prepare Visible Scene Preview")]
        public static void Prepare()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            var old=GameObject.Find(RootName);
            if (old!=null) Object.DestroyImmediate(old);
            Directory.CreateDirectory(SpriteFolder); AssetDatabase.Refresh();
            var root=new GameObject(RootName); root.AddComponent<EditorScenePreview>();
            Add(root,"Academy Courtyard",Sprite("Courtyard","Stages/Witchwood",new Rect(0,0,640,360),new Vector2(.5f,.5f),50),Vector3.zero,0);
            var ids=new[]{"Lyra","Alden"};
            for (int i=0;i<2;i++) {
                float x=i==0?410:870;
                var fighter=Add(root,"Player "+(i+1)+" Preview",Sprite(ids[i],"Characters/"+ids[i]+"/sheet",new Rect(0,1152,128,128),new Vector2(.5f,12f/128),100f/1.5f),new Vector3((x-640)/100,-1.97f,0),2);
                fighter.flipX=i==1;
            }
            var rift=Sprite("RuneRift","Stages/RuneRift",new Rect(0,128,128,128),new Vector2(.5f,.5f),100f/1.125f);
            foreach(float x in new[]{DuelModel.LeftRiftX,DuelModel.RightRiftX})
                Add(root,"Forbidden Rune Rift",rift,new Vector3((x-640)/100,-1.87f,0),1);
            var cam=Camera.main; cam.orthographic=true; cam.orthographicSize=3.6f; cam.transform.position=new Vector3(0,0,-10);
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            if (!Application.isBatchMode) FocusGame();
            Debug.Log("HEXBOUND EDITOR PREVIEW READY: saved courtyard, both fighters and edge rifts visible without Play");
        }
        static Sprite Sprite(string name,string resource,Rect rect,Vector2 pivot,float pixelsPerUnit)
        {
            string path=SpriteFolder+"/"+name+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path); if (existing!=null) return existing;
            var texture=Resources.Load<Texture2D>(resource);
            var sprite=UnityEngine.Sprite.Create(texture,rect,pivot,pixelsPerUnit,0,SpriteMeshType.FullRect);
            sprite.name=name; AssetDatabase.CreateAsset(sprite,path); return sprite;
        }
        static SpriteRenderer Add(GameObject root,string name,Sprite sprite,Vector3 position,int order)
        {
            var go=new GameObject(name); go.transform.SetParent(root.transform); go.transform.localPosition=position;
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=order;return renderer;
        }
        public static void FocusGame()
        {
            var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(type).Show();
        }
        public static void ValidateEditPreview()
        {
            Prepare();
            var root=GameObject.Find(RootName);
            if (root==null || !root.activeSelf || root.GetComponentsInChildren<SpriteRenderer>().Length!=5)
                throw new System.Exception("Expected saved background, two fighters and two rifts before Play");
            var camera=Camera.main; var target=new RenderTexture(1280,720,24);
            var previous=RenderTexture.active; camera.targetTexture=target;
            camera.Render(); RenderTexture.active=target;
            var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1280,720),0,0); texture.Apply();
            Directory.CreateDirectory("Docs/Previews");
            File.WriteAllBytes("Docs/Previews/00-editor-scene.png",texture.EncodeToPNG());
            camera.targetTexture=null; RenderTexture.active=previous;
            Object.DestroyImmediate(texture); Object.DestroyImmediate(target);
            Debug.Log("HEXBOUND EDIT PREVIEW VALIDATION PASSED: visible before Play");
        }
    }
}
