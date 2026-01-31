using UnityEngine;

public class CollectItem : MonoBehaviour
{
    [SerializeField] private GameEvent OnKeyCollected;
    [SerializeField] private GameEvent OnSalvationToolCollected;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Item raccolto!");

            if (OnKeyCollected)
                OnKeyCollected.Raise();

            if (OnSalvationToolCollected)
                OnSalvationToolCollected.Raise();

            Destroy(gameObject);
        }

    }
}
