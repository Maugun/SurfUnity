using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

namespace P90brush
{
    public class GameLogic : MonoBehaviour
    {

        public CheckPointTest[] _checkPoints;
        public Text _statusText;
        private static float _currentTime;

        void Start() {

        }

        // Update is called once per frame
        void Update() {
            _currentTime += Time.deltaTime;

            UpdateUI();
        }

        private void UpdateUI() {
            StringBuilder sb = new StringBuilder(string.Format("CurrentTime: {0}\n", _currentTime.ToString("F2")));
            short count = 1;
            sb.Append("Checkpoints:\n");
            foreach (CheckPointTest checkPoint in _checkPoints) {
                sb.Append(string.Format("  {0} : {1}\n", count, checkPoint.IsValidated() ? checkPoint.GetPassedTime().ToString("F2") : "Not Passed"));
                count++;
            }
            _statusText.text = sb.ToString();
        }

        public static float GetCurrentTime() {
            return _currentTime;
        }
    }
}
