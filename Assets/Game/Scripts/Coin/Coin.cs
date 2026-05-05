using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    [Header("Float")]
    [SerializeField] private float bobHeight = 0.25f;
    [SerializeField] private float bobSpeed = 2.2f;
    [SerializeField] private float rotateSpeed = 150f;

    [Header("Collect Animation")]
    [SerializeField] private float riseDuration = 0.35f;
    [SerializeField] private float riseHeight = 1.2f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem collectParticles;
    [SerializeField] private AudioClip      collectSound;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Events")]
    public UnityEvent OnCollected;

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
        if (_collected || !other.CompareTag("Player")) return;
        if (other.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.OnCoinCollected();
        }
        Collect();
    }

    public void Collect()
    {
        if (_collected) return;
        _collected = true;

        OnCollected.Invoke();

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position, sfxVolume);

        if (collectParticles != null)
        {
            collectParticles.transform.SetParent(null);
            collectParticles.Play();
            Destroy(collectParticles.gameObject, collectParticles.main.duration + collectParticles.main.startLifetime.constantMax);
        }

        StartCoroutine(CollectAnimation());
    }

    private IEnumerator CollectAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);

            // ease-out cubic
            float e = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = startPos + Vector3.up * (riseHeight * e);
            transform.localScale = startScale * (1f - e);

            yield return null;
        }

        Destroy(gameObject);
    }
}
