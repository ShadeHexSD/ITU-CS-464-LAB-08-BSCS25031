using UnityEngine;

/// <summary>Follows the player from a fixed offset. Put it on the camera, not on the shake pivot.</summary>
public class Lab08FollowCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 12f, -12f);
    [SerializeField] float smooth = 6f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 wanted = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, wanted, 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime));
        transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
    }
}
