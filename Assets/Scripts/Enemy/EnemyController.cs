using MLTD.ML;
using UnityEngine;

namespace MLTD.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private readonly Vector2[] _directions = new[]
        {
            new Vector2(1f, .1f).normalized,
            new Vector2(1f, .2f).normalized,
            new Vector2(1f, 0f).normalized,
            new Vector2(1f, -.1f).normalized,
            new Vector2(1f, -.2f).normalized
        };

        private const float _distance = 20f;

        private void FixedUpdate()
        {
            foreach (var dir in _directions)
            {
                Debug.DrawRay(transform.position + transform.right, dir * _distance, Color.red);
                var hit = Physics2D.Raycast(transform.position + transform.right, dir, _distance);
                if (hit.collider != null)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.blue);
                }
            }

            InputData data = new InputData();
        }
    }
}
