using UnityEngine;
namespace Hexbound
{
    // Saved SpriteRenderers make Main visible before Play; gameplay draws its own view.
    public sealed class EditorScenePreview : MonoBehaviour
    {
        void Awake() { gameObject.SetActive(false); }
    }
}
