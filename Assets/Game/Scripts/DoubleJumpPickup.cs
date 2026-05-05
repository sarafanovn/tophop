using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoubleJumpPickup : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 1.8f;
    [SerializeField] private float rotateSpeed = 90f;

    [SerializeField] private ParticleSystem collectParticles;
    [SerializeField] private AudioClip      collectSound;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    private Vector3 _basePos;
    private bool _collected;

    private void Start()
    {
        _basePos = transform.position;
        GetComponent<Collider>().isTrigger = true;
    }

    private void Update()
    {
        if (_collected) return;

        transform.position = _basePos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;

        if (!other.TryGetComponent(out PlayerController player)) return;

        _collected = true;
        player.ActivateDoubleJump(duration);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position, sfxVolume);

        if (collectParticles != null)
        {
            collectParticles.transform.SetParent(null);
            collectParticles.Play();
            Destroy(collectParticles.gameObject, collectParticles.main.duration + collectParticles.main.startLifetime.constantMax);
        }

        Destroy(gameObject);
    }
}
