using UnityEngine;

public class GrassBendController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float bendRadius = 1.5f;
    [SerializeField] private float recoverySpeed = 2.5f;  // скорость выпрямления

    private static readonly int PlayerPosID = Shader.PropertyToID("_PlayerPos");

    // "фантомная" позиция: двигается к игроку мгновенно, а обратно — плавно
    private Vector3 _virtualPos = new Vector3(99999f, 0f, 99999f);
    private bool _inRange;

    void Update()
    {
        Vector3 pp = player.position;
        float dist = Vector2.Distance(
            new Vector2(pp.x, pp.z),
            new Vector2(_virtualPos.x, _virtualPos.z)
        );

        bool nowInRange = Vector2.Distance(
            new Vector2(pp.x, pp.z),
            new Vector2(transform.position.x, transform.position.z)
        ) < bendRadius * 4f; // глобальный AABB-гвард

        if (nowInRange)
        {
            // игрок рядом — фантом идёт за ним мгновенно
            _virtualPos = pp;
            _inRange = true;
        }
        else if (_inRange)
        {
            // игрок ушёл — плавно выводим фантом за радиус
            Vector3 away = new Vector3(99999f, 0f, 99999f);
            _virtualPos = Vector3.MoveTowards(_virtualPos, away, recoverySpeed * Time.deltaTime * 1000f);

            if ((_virtualPos - pp).magnitude > bendRadius * 2f)
                _inRange = false;
        }

        Shader.SetGlobalVector(PlayerPosID, _virtualPos);
    }
}