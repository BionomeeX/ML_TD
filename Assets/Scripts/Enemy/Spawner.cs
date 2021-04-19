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

        private int _waveCount = 1;

        private void Start()
        {
            StartCoroutine(SpawnAll());
        }

        private IEnumerator SpawnAll()
        {
            int bestOfMaxCount = 20;
            List<NN> networks = new List<NN>();
            List<(NN network, float score)> networks_BestOf = new List<(NN network, float score)>(bestOfMaxCount);

            while (true)
            {
                var maxSize = new Vector2(-transform.position.x + _x, transform.position.y + _y);
                int count = 0;
                for (int x = -_x; x <= _x; x++)
                {
                    for (int y = -_y; y <= _y; y++)
                    {
                        var go = Instantiate(_enemyPrefab, transform.position + new Vector3(x, y), Quaternion.identity);
                        go.transform.parent = transform;
                        var ec = go.GetComponent<EnemyController>();
                        ec.Init(networks.Count == 0 ? null : new NN(networks[ count ]));
                        ec.WorldMaxSize = maxSize;
                        _instancied.Add(ec);
                        count++;
                    }
                }
                var timer = 10f;
                while (timer > 0)
                {
                    yield return new WaitForSeconds(1f);
                    _timeRemainding.text = $"Wave ${_waveCount} end in {timer} seconds";
                    timer--;
                }

                List<(NN network, float score)> oldgen = _instancied.Select(ec => (ec.Network, ec.gameObject.transform.position.x)).ToList();
                networks_BestOf.AddRange(oldgen);
                networks_BestOf.Sort(delegate
                ((NN network, float score) a, (NN network, float score) b)
                {
                    return b.score.CompareTo(a.score);
                });
                networks_BestOf = networks_BestOf.Take(bestOfMaxCount).ToList();

                networks = GeneticAlgorithm.GeneratePool(oldgen, networks_BestOf, _instancied.Count);

                foreach (var p in _instancied)
                {
                    Destroy(p.gameObject);
                }
                _instancied.RemoveAll(x => true);
                _waveCount++;
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
