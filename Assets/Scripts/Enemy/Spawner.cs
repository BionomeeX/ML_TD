using System.Collections.Generic;
using UnityEngine;

namespace MLTD.Enemy
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject _enemyPrefab;

        private List<GameObject> _instancied = new List<GameObject>();

        private const int _x = 3;
        private const int _y = 5;

        private void Start()
        {
            SpawnAll();
        }

        private void SpawnAll()
        {
            for (int x = -_x; x <= _x; x++)
            {
                for (int y = -_y; y <= _y; y++)
                {
                    var go = Instantiate(_enemyPrefab, transform.position + new Vector3(x, y), Quaternion.identity);
                    go.transform.parent = transform;
                    _instancied.Add(go);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var mid = transform.position;
            var up = transform.up * _y;
            var right = transform.right * _x;
            Gizmos.DrawLine(mid + up + right, mid - up + right);
            Gizmos.DrawLine(mid + up + right, mid + up - right);
            Gizmos.DrawLine(mid - up - right, mid + up - right);
            Gizmos.DrawLine(mid - up + right, mid - up - right);
        }
    }
}
