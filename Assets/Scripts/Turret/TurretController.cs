using MLTD.Enemy;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLTD.Turret
{
    public class TurretController : MonoBehaviour
    {
        public List<EnemyController> Triggers { private set; get; } = new List<EnemyController>();

        private const int _reloadTimerRef = 25;
        private int _reloadTimer = 0;

        private void FixedUpdate()
        {
            _reloadTimer++;
            if (_reloadTimer >= _reloadTimerRef && Triggers.Count > 0)
            {
                Triggers.RemoveAll(x => !x.IsAlive());
                var closest = Triggers.OrderBy(x => Vector2.Distance(transform.position, x.transform.position)).FirstOrDefault();
                if (closest != null)
                {
                    closest.TakeDamage(1);
                    Debug.DrawLine(transform.position, closest.transform.position, Color.red, .2f);
                    _reloadTimer = 0;
                }
            }
        }
    }
}
