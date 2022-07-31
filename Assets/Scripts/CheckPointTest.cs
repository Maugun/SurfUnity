using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace P90brush
{
    public class CheckPointTest : MonoBehaviour
    {

        private bool _isValidated = false;
        private float _passedTime = 0f;
        // Start is called before the first frame update
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if (_isValidated && _passedTime == 0f) {
                _passedTime = GameLogic.GetCurrentTime();
            }
        }

        private void OnTriggerEnter(Collider other) {
            _isValidated = true;
        }


        public bool IsValidated() {
            return _passedTime != 0f;
        }

        public float GetPassedTime() {
            return _passedTime;
        }
    }
}