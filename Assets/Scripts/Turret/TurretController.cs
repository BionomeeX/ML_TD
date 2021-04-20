using MLTD.Enemy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLTD.Turret
{
    public class TurretController : MonoBehaviour
    {
        public List<EnemyController> Triggers { private set; get; } = new List<EnemyController>();

        private const float _reloadTimerRef = .25f;
        private float _reloadTimer = 0f;

        private void Update()
        {
            _reloadTimer -= Time.deltaTime;
            if (_reloadTimer <= 0f && Triggers.Count > 0)
            {
                Triggers.RemoveAll(x => !x.IsAlive());
                var closest = Triggers.OrderBy(x => Vector2.Distance(transform.position, x.transform.position)).FirstOrDefault();
                if (closest != null)
                {
                    closest.TakeDamage(1);
                    Debug.DrawLine(transform.position, closest.transform.position, Color.red, .2f);
                    _reloadTimer = _reloadTimerRef;
                }
            }
        }
    }
}
