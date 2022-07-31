using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace P90brush
{
    public class SlowMotion : MonoBehaviour
    {
        [Header("Slow Motion Params")]
        public float _duration = 4f;
        public float _transitionDuration = 2f;
        [Range(0f, 1f)]
        public float _timeScale = 0.5f;

        [Header("UI")]
        public Image _foregroundImage;

        private bool _isSlowMotion = false;
        private float _totalDuration = 0f;
        private float _timer = 0f;

        public bool IsSlowMotion {
            get { return _isSlowMotion; }
            set
            {
                if (value)
                {
                    _timer = 0f;
                    Time.timeScale = _timeScale;
                    Time.fixedDeltaTime = Time.timeScale * .02f;
                    _totalDuration = _duration + _transitionDuration;

                    _foregroundImage.color = Color.blue;
                    _foregroundImage.fillAmount = 1f;
                }
                else
                {
                    Time.timeScale = 1f;
                    //Time.fixedDeltaTime = Time.timeScale * .02f;

                    _foregroundImage.color = Color.blue;
                    _foregroundImage.fillAmount = 1f;
                }
                _isSlowMotion = value;
            }
        }

        void Start()
        {
            IsSlowMotion = false;
        }

        void Update()
        {
            Logic();
        }

        private void Logic()
        {
            if (IsSlowMotion)
            {
                _timer += Time.unscaledDeltaTime;

                // End
                if (_timer >= _totalDuration)
                {
                    IsSlowMotion = false;
                    return;
                }


                // Transition
                if (_timer >= _duration)
                {
                    Time.timeScale += (1f - _timeScale) / _transitionDuration * Time.unscaledDeltaTime;
                    Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
                    //Time.fixedDeltaTime = Time.timeScale * .02f;

                    _foregroundImage.color = Color.red;
                }

                // UI
                _foregroundImage.fillAmount = 1f - (_timer / _totalDuration);
            }
        }

        // TODO: AUDIO MANAGEMENT W/ TIMESCALE
    }
}
