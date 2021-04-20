using MLTD.Enemy;
using System.Collections.Generic;
using UnityEngine;

namespace MLTD.Turret
{
    public class TurretZone : MonoBehaviour
    {
        [SerializeField]
        private int _xSize, _ySize;

        [SerializeField]
        [Range(0, 100)]
        private int _chance;

        [SerializeField]
        private GameObject _turretPrefab;

        private List<GameObject> _instanciated = new List<GameObject>();

        private void Awake()
        {
            Spawner.S.TurretZones.Add(this);
        }

        public void Regenerate()
        {
            foreach (var go in _instanciated)
            {
                Destroy(go);
            }
            _instanciated = new List<GameObject>();
            for (int x = -_xSize; x < _xSize; x++)
            {
                for (int y = -_ySize; y < _ySize; y++)
                {
                    if (Random.Range(0, 100) < _chance)
                    {
                        _instanciated.Add(Instantiate(_turretPrefab, new Vector2(x +.5f, y + .5f), Quaternion.identity));
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            var mid = transform.position;
            var up = transform.up * _ySize;
            var right = transform.right * _xSize;
            Gizmos.DrawLine(mid + up + right, mid - up + right);
            Gizmos.DrawLine(mid + up + right, mid + up - right);
            Gizmos.DrawLine(mid - up - right, mid + up - right);
            Gizmos.DrawLine(mid - up + right, mid - up - right);
        }
    }
}
