using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MLTD.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController S;

        private void Awake()
        {
            S = this;
        }

        private Rigidbody2D _rb;

        [SerializeField]
        private Text _goldDisplay;

        public int GoldAmount { set; get; }

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

        public void UpdateGoldText()
        {
            _goldDisplay.text = "Your Gold: " + GoldAmount;
        }

        private IEnumerator IncreaseGold()
        {
            while (true)
            {
                _goldDisplay.text = "Your Gold: " + GoldAmount;
                yield return new WaitForSeconds(_goldWaitTime);
                GoldAmount += _goldIncrease;
            }
        }
    }
}
