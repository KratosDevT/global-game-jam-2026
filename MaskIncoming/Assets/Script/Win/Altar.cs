using UnityEngine;

public class Altar : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject promptUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player p = other.GetComponent<Player>();
            p.SetCanSacrifice(true);

            if (promptUI != null) promptUI.SetActive(true);

            var promptText = promptUI.GetComponent<TMPro.TextMeshProUGUI>();
            
            if (promptText != null)
            {
                int tools = p.GetSalvationToolsCount();
                if (tools >= 3)
                    promptText.text = "Press [E] to the final sacrifice!";
                else
                    promptText.text = "You need souls... (" + tools + "/3)";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().SetCanSacrifice(false);
            if (promptUI != null) promptUI.SetActive(false);
        }
    }
}
