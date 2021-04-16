using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MLTD.Enemy
{
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D _rb;

        [SerializeField]
        private Text _goldDisplay;

        private int _goldAmount;

        private const float _goldWaitTime = 3f;
        private const int _goldIncrease = 5;

        private const float _speed = 10f;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            StartCoroutine(IncreaseGold());
        }

        private void FixedUpdate()
        {
            var hor = Input.GetAxis("Horizontal");
            var ver = Input.GetAxis("Vertical");
            _rb.velocity = new Vector2(hor, ver) * _speed;
        }

        private IEnumerator IncreaseGold()
        {
            while (true)
            {
                _goldDisplay.text = "Your Gold: " + _goldAmount;
                yield return new WaitForSeconds(_goldWaitTime);
                _goldAmount += _goldIncrease;
            }
        }
    }
}
