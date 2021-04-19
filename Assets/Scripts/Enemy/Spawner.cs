using MLTD.ML;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MLTD.Enemy
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject _enemyPrefab;

        [SerializeField]
        private Text _timeRemainding;

        private List<EnemyController> _instancied = new List<EnemyController>();

        private const int _x = 3;
        private const int _y = 5;

        private void Start()
        {
            StartCoroutine(SpawnAll());
        }

        private IEnumerator SpawnAll()
        {
            List<NN> networks = new List<NN>();
            while (true)
            {
                var maxSize = new Vector2(-transform.position.x + _x, transform.position.y + _y);
                for (int x = -_x; x <= _x; x++)
                {
                    for (int y = -_y; y <= _y; y++)
                    {
                        var go = Instantiate(_enemyPrefab, transform.position + new Vector3(x, y), Quaternion.identity);
                        go.transform.parent = transform;
                        var ec = go.GetComponent<EnemyController>();
                        ec.Init(networks.Count == 0 ? null : new NN(networks[Random.Range(0, networks.Count)]));
                        ec.WorldMaxSize = maxSize;
                        _instancied.Add(ec);
                    }
                }
                var timer = 10f;
                while (timer > 0)
                {
                    yield return new WaitForSeconds(1f);
                    _timeRemainding.text = $"Wave end in {timer} seconds";
                    timer--;
                }
                networks = GeneticAlgorithm.GeneratePool(_instancied.Select(ec => (ec.Network, ec.gameObject.transform.position.x)).ToList(), 100);
                foreach (var p in _instancied)
                {
                    Destroy(p.gameObject);
                }
                _instancied.RemoveAll(x => true);
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
