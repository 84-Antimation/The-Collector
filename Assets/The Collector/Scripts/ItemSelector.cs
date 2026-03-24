using UnityEngine;

namespace FoxX.TheCollector
{
    public class ItemSelector : MonoBehaviour
    {
        [Header(" Rendering ")]
        [SerializeField] private Renderer renderer;

        public void Select(Material outlineMaterial)
        {
            Material baseMaterial = renderer.material;
            renderer.materials = new Material[2] { baseMaterial, outlineMaterial };
        }

        public void Deselect()
        {
            renderer.materials = new Material[] { renderer.materials[0] };
        }

        public void DisableShadows()
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        
        public void EnableShadows()
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}