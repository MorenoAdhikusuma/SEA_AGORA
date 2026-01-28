using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator anim;
    public int health = 3;

     void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Die()
{
    health--;
    anim.SetTrigger("Hit");
    Debug.Log("Health: " + health);

    if (health <= 0)
    {
        //TODO : INI BELUM KE TRIGGER ANIM NYA 
        anim.SetTrigger("Death");
        Destroy(gameObject, 1f); // Delay to allow death animation to play
    }
}

}
