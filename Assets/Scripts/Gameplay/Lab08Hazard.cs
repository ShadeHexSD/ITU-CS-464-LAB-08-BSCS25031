using UnityEngine;

/// <summary>A moving hazard. It damages the player through Health, which raises the events.</summary>
public class Lab08Hazard : MonoBehaviour
{
    [SerializeField] Health target;
    [SerializeField] int damage = 1;
    [SerializeField] Vector3 travel = new Vector3(0f, 0f, 3f);
    [SerializeField] float speed = 1f;

    Vector3 start;

    void Start() => start = transform.position;

    void Update()
    {
        if (Time.timeScale <= 0f) return;
        transform.position = start + travel * Mathf.Sin(Time.time * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) target?.TakeDamage(damage);
    }
}
