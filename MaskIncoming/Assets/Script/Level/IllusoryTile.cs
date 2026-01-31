using UnityEngine;

namespace Script.Level
{
    public class IllusoryTile : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer realRenderer; // Assegna "Real_Layer"
        public SpriteRenderer fakeRenderer; // Assegna "Fake_Layer" (con materiale Shader)
    
        [Header("Data")]
        public Sprite realSprite;
        public Sprite fakeSprite;

        public void Setup(bool isIllusory)
        {
            // Setup real maze sprite
            if (realRenderer != null && realSprite != null)
                realRenderer.sprite = realSprite;

            // Handle illusion
            if (isIllusory && fakeRenderer != null && fakeSprite != null)
            {
                fakeRenderer.gameObject.SetActive(true); 
                fakeRenderer.sprite = fakeSprite;
            }
            else if (fakeRenderer != null)
            {
                fakeRenderer.gameObject.SetActive(false);
            }
        }
    }
}