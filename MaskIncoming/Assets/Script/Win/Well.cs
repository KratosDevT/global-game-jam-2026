using UnityEngine;
using UnityEngine.UI;

public class Well : MonoBehaviour
{
    [SerializeField] private GameObject promptUI;

    private Player playerRef;
    
    void Start()
    {
        
    }

    void Update()
    {

    }

    public void TryEscape()
    {
        Player p = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        if (p != null && p.HasKey())
        {
            Debug.Log("VITTORIA!");
            // Carica scena o mostra UI vittoria
        }
        else
        {
            Invoke("ResetPromptColor", 0.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRef = other.GetComponent<Player>();
            playerRef.SetCanEscape(true);
            
            if (promptUI != null) promptUI.SetActive(true);

            var promptText = promptUI.GetComponent<TMPro.TextMeshProUGUI>();
            
            if (promptText != null)
            {
                promptText.text = playerRef.HasKey() ? "Press [E] to escape!" : "The well is closed... You need a key";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (promptUI != null) promptUI.SetActive(false);
        }
    }
}
