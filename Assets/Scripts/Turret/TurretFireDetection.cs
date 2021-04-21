using MLTD.Enemy;
using UnityEngine;

namespace MLTD.Turret
{
    public class TurretFireDetection : MonoBehaviour
    {
        private TurretController tc;

        private void Start()
        {
            tc = transform.parent.GetComponent<TurretController>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                tc.Triggers.Add(collision.GetComponent<EnemyController>());
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Enemy"))
            {
                tc.Triggers.Remove(collision.GetComponent<EnemyController>());
            }
        }
    }
}
