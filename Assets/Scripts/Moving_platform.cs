using UnityEngine;

public class Moving_platform : MonoBehaviour
{
      public float speed = 2f;
    public int startpoints;
    public Transform[] points;

   private int i;

    private void Start()
    {
        transform.position = points[startpoints].position;
    }

    private void Update()
    {
       if(Vector2.Distance(transform.position, points[i].position) < 0.1f)
        {
            i++;
            if(i >= points.Length)
            {
                i = 0;
            }
        }

        transform.position = Vector2.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(transform);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(null);
        }
    }
}
