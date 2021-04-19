using UnityEngine;

namespace MLTD.Turret
{
    public class TurretManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _turret;

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var finalPos = new Vector2((int)pos.x + .5f, (int)pos.y + .5f);
                Instantiate(_turret, finalPos, Quaternion.identity);
            }
        }
    }
}
