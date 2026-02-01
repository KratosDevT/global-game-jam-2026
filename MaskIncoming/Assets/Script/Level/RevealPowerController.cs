using UnityEngine;

namespace Script.Level
{
    public class RevealPowerController : MonoBehaviour
    {
        [Header("Reveal Settings")]
        public float radius = 3f;
        public float softness = 0.5f;
        [SerializeField] private bool bActive = false;
        
        void Update()
        {
            if (!bActive)
            {
                HandleOnMaskDeactivation();
                return;
            }
            
            HandleOnMaskActivation();
            Vector3 p = transform.position;
            Shader.SetGlobalVector(
                "_GlobalRevealPos",
                new Vector4(p.x, p.y, p.z, 1f)
            );
        }

        public void HandleOnMaskActivation()
        {
            bActive = true;
            Shader.SetGlobalFloat("_GlobalRevealEnabled", 1f);
            Shader.SetGlobalFloat("_GlobalRevealRadius", radius);
            Shader.SetGlobalFloat("_GlobalEdgeSoftness", softness);
            bActive = true;
        }

        public void HandleOnMaskDeactivation()
        {
            bActive = false;
            Shader.SetGlobalFloat("_GlobalRevealEnabled", 0f);
            bActive = false;
        }
    }
}