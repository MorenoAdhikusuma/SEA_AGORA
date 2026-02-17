using UnityEngine;

public class Trap : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Game_Manager.Instance.MC_Hit(other.gameObject);
        }
    }
}
