using UnityEngine;

namespace Script.Level
{
    public class RevealPowerController : MonoBehaviour
    {
        [Header("Reveal Settings")]
        public float radius = 3f;
        public float softness = 0.5f;
        private bool bActive = false;
        
        
        void Update()
        {
            if(!bActive) return;
            Vector3 pos = transform.position;
            Shader.SetGlobalVector("_RevealCenterWS", pos);
        }

        public void HandleOnMaskActivation()
        {
            bActive = true;
            Shader.SetGlobalFloat("_RevealEnabled", 1f);
            Shader.SetGlobalFloat("_RevealRadius", radius);
            Shader.SetGlobalFloat("_RevealSoftness", softness);
        }

        public void HandleOnMaskDeactivation()
        {
            bActive = false;
            Shader.SetGlobalFloat("_RevealEnabled", 0f);
        }
    }
}