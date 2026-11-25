using UnityEngine;

/// <summary>
/// A collectible. Plays its sound through SfxPlayer (NOT its own AudioSource, which would be
/// destroyed with the pickup the same frame and never be heard), spawns a self-cleaning sparkle,
/// raises GameEvents.CoinCollected for anything that cares (score, wallet, HUD), and removes itself.
/// Works with 3D and 2D trigger colliders.
/// </summary>
public class Pickup : MonoBehaviour
{
    [SerializeField] int value = 1;
    [SerializeField] AudioClip pickupClip;
    [SerializeField] GameObject sparklePrefab;     // Particle System: Stop Action = Destroy
    [SerializeField] string playerTag = "Player";

    bool collected;

    void OnTriggerEnter(Collider other) => TryCollect(other.gameObject);
    void OnTriggerEnter2D(Collider2D other) => TryCollect(other.gameObject);

    void TryCollect(GameObject who)
    {
        if (collected || !who.CompareTag(playerTag)) return;
        collected = true;

        GameEvents.RaiseCoinCollected(value);
        if (SfxPlayer.Instance != null) SfxPlayer.Instance.Play(pickupClip);
        if (sparklePrefab != null) Instantiate(sparklePrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
