using Unity.Collections;
using UnityEngine;

namespace Script.Level
{
    public class IllusoryTile : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer realRenderer;
        public MeshRenderer fakeMeshRenderer;
        [Header("Data")]
        public Sprite realSprite;
        public Texture realButOnTopTexture;
        public Texture fakeTexture;
        
        [Header("Debug Path of Tiles")]
        [SerializeField, ReadOnly] private int debugMask;
        [SerializeField, ReadOnly] private string debugPaths;

        private MaterialPropertyBlock _mpb;
        
        //debug
        private void Start()
        {
            float randomFakeRot = Random.Range(0, 4) * 90f;
            _mpb = new MaterialPropertyBlock();
            fakeMeshRenderer.GetPropertyBlock(_mpb);
            _mpb.SetTexture("_MainTex", fakeTexture);
            _mpb.SetFloat("_UVRotation", 180f);
            fakeMeshRenderer.SetPropertyBlock(_mpb);
        }
        
        public void Setup(bool isIllusory, Texture inFakeTexture = null, float fakeRotation = 0f, float realRotation = 0f)
        {
            if (!(realRenderer != null && realSprite != null))
                return;
            
            // To be sure set also the real sprite
            realRenderer.sprite = realSprite;
        
            // Set the real/fake overlay
            fakeMeshRenderer.gameObject.SetActive(true);
            _mpb ??= new MaterialPropertyBlock();
            
            if (isIllusory && inFakeTexture != null)
            {
                fakeMeshRenderer.GetPropertyBlock(_mpb);
                _mpb.SetTexture("_MainTex", fakeTexture);
                _mpb.SetFloat("_UVRotation", fakeRotation);
                fakeMeshRenderer.SetPropertyBlock(_mpb);
            }
            else
            {
                // set the real overlay on top
                fakeMeshRenderer.GetPropertyBlock(_mpb);
                _mpb.SetTexture("_MainTex", realButOnTopTexture);
                _mpb.SetFloat("_UVRotation", realRotation);
                fakeMeshRenderer.SetPropertyBlock(_mpb);
            }
        }
        
        public void SetDebug(int mask)
        {
            debugMask = mask;
            debugPaths = MaskToString(mask);
            gameObject.name += $" [M:{debugPaths}]";
        }

        string MaskToString(int mask)
        {
            // N=1, E=2, S=4, W=8
            string s = "";
            if ((mask & 1) != 0) s += "N";
            if ((mask & 2) != 0) s += "E";
            if ((mask & 4) != 0) s += "S";
            if ((mask & 8) != 0) s += "W";
            return s == "" ? "X" : s;
        }
    }
}