using Unity.Collections;
using UnityEngine;

namespace Script.Level
{
    public class IllusoryTile : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer realRenderer;
        public SpriteRenderer fakeRenderer;
        public MeshRenderer fakeMeshRenderer;
        [Header("Data")]
        public Sprite realSprite;
        public Texture fakeTexture;
        
        [Header("Debug")]
        [SerializeField, ReadOnly] private int debugMask;
        [SerializeField, ReadOnly] private string debugPaths;

        private MaterialPropertyBlock _mpb;
        
        //debug
        private void Start()
        {
            _mpb = new MaterialPropertyBlock();
            fakeMeshRenderer.GetPropertyBlock(_mpb);
            _mpb.SetTexture("_MainTex", fakeTexture);
            fakeMeshRenderer.SetPropertyBlock(_mpb);
        }
        
        public void Setup(bool isIllusory, Sprite fakeSprite = null, float fakeRotation = 0f)
        {
            // Il Real Sprite è già corretto perché fa parte del Prefab istanziato
        
            if (isIllusory && fakeSprite != null)
            {
                fakeRenderer.gameObject.SetActive(true);
                fakeRenderer.sprite = fakeSprite;
            
                // Applichiamo una rotazione locale allo sprite finto
                // così è indipendente dalla rotazione della strada vera
                fakeRenderer.transform.localRotation = Quaternion.Euler(0, 0, fakeRotation);
            }
            else
            {
                fakeRenderer.gameObject.SetActive(false);
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