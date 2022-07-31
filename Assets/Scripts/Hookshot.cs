using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace P90brush
{
    public class Hookshot : MonoBehaviour
    {
        public enum HookStatus { None, Free, Retracting, ElasticExpanding, ElasticCollapsing }

        public Camera _playerCam;
        public LineRenderer _lr;
        public float _maxDistance = 50f;
        public float _minDistance = 0.5f;

        public float _defaultLength = 6f;
        public float _defaultLengthPercent = .8f;
        public float _retractingStrength = 150f;

        public float _elasticExpandStrength = 140f;
        public float _elasticCollapseStrength = 60f;
        [Range(0, 100)]
        public short _elasticSmoothPercentage = 20;

        private HookStatus _status = HookStatus.None;

        void Start() {
            _lr.enabled = false;
        }

        void Update() {
        }


        public void CheckForRelease(ISurfControllable surfer) {
            if (!surfer.InputData.HookPressed) {
                if (surfer.InputData.HookPressedLastUpdate) {//Player has Release the Hook Button
                    ReleaseHook(surfer);
                }

                return; //Player didn't Pressed the Hook Button
            }
        }


        public void CatchMovement(ISurfControllable surfer, float deltaTime) {
            CheckForRelease(surfer);//To be sure

            if (!surfer.PlayerData.IsHooked()) {
                return;//Player has No Point where Hook is Attached
            }

            //Update LineRenderer
            _lr.SetPosition(0, _lr.transform.position);
            _lr.SetPosition(1, surfer.PlayerData.HookedPosition);



            float hookDistance = Vector3.Distance(surfer.PlayerData.Origin, surfer.PlayerData.HookedPosition);

            if (!surfer.InputData.HookRetractPressed) {
                if (hookDistance < _defaultLength) {
                    _status = HookStatus.Free;
                    return; //Nothing to do
                }
            }


            //float strength; //Strength to apply to the hook velocity
            //if (surfer.InputData.HookRetractPressed) {
            //    _status = HookStatus.Retracting;
            //    strength = _retractingStrength;
            //} else {
            //    strength = ComputeStrength(surfer, hookDistance);
            //    strength = SmoothStrength(strength, hookDistance);
            //    //strength = SmoothStrength(hookDistance);
            //}

            float strength = ComputeStrength(surfer, hookDistance);

            Debug.Log("Hook Strength: " + strength);

            if (strength != 0f) {
                Vector3 playerToHookPosition = surfer.PlayerData.Origin - surfer.PlayerData.HookedPosition;
                playerToHookPosition = playerToHookPosition.normalized; //Normalized ??

                surfer.PlayerData.Velocity -= playerToHookPosition * strength * deltaTime;
            }


            // Check if we reached the Hoocked Position
            if (hookDistance <= _minDistance) {
                ReleaseHook(surfer);
            }
        }

        private float ComputeStrength(ISurfControllable surfer, float hookDistance) {
            float strength = 0f; // Strength to apply to the hook velocity

            float distancePercent = hookDistance * _defaultLengthPercent;

            if (surfer.InputData.HookRetractPressed) {
                _status = HookStatus.Retracting;
                strength = _retractingStrength + _elasticCollapseStrength;
            } else {
                // Vector from Player To Hook Position
                Vector3 playerToHookPosition = surfer.PlayerData.Origin - surfer.PlayerData.HookedPosition;

                // Compare direction of the Vector of the Player Velocity with the Vector from Player To Hook Position
                float dot = Vector3.Dot(playerToHookPosition, surfer.PlayerData.Velocity);

                if (dot > 0)        // Vectors are in opposit direction
                {
                    _status = HookStatus.ElasticExpanding; // We are expanding the elastic part
                    strength = _elasticExpandStrength;
                } else                // Vectors are in same direction
                  {
                    _status = HookStatus.ElasticCollapsing; // We are collapsing the elastic part
                    strength = _elasticCollapseStrength;
                }
            }

            if (_defaultLength > distancePercent && _status == HookStatus.Retracting)
                _defaultLength = distancePercent;


            return strength;
        }


        //private float ComputeStrength(ISurfControllable surfer, float hookDistance) {
        //    //Vector from Player To Hook Position
        //    Vector3 playerToHookPosition = surfer.PlayerData.Origin - surfer.PlayerData.HookedPosition;

        //    //Compare direction of the Vector of the Player Velocity with the Vector from Player To Hook Position
        //    float dot = Vector3.Dot(playerToHookPosition, surfer.PlayerData.Velocity);

        //    if (dot > 0) {//Vectors are in opposit direction
        //        _status = HookStatus.ElasticExpanding; //We are expanding the elastic part
        //        return _elasticExpandStrength;
        //    } else {//Vectors are in same direction
        //        _status = HookStatus.ElasticCollapsing; //We are collapsing the elastic part
        //        return _elasticCollapseStrength;
        //    }

        //}
        private float SmoothStrength(float strength, float hookDistance) {
            float hookPercentage = hookDistance * 100 / _defaultLength;
            Debug.Log("HookPercentage :" + hookPercentage);
            if (hookPercentage > 100/* - _elasticSmoothPercentage*/ && hookPercentage < 100 + _elasticSmoothPercentage) {//Trigger the smooth area
                float delta = hookPercentage > 100 ? (hookPercentage - 100) : (100 - hookPercentage); //Delta Percentage (between 0 and hookPercentage)
                delta /= _elasticSmoothPercentage; //Compute

                //Ease in interpolation -> Check to remove this
                delta = 1f - Mathf.Cos(delta * Mathf.PI * 0.5f);
                delta *= delta;

                return Mathf.Lerp(strength * 0.1f, strength, delta);//Smooth the strength
            }

            return strength;
        }

        /*private float SmoothStrength(float hookDistance) {
            /*float _retractingSmoothLength = _defaultLength * _retractingSmoothPercentage / 100;
            if (hookDistance > _defaultLength - _retractingSmoothLength && hookDistance < _defaultLength + _retractingSmoothLength) {//Trigger the smooth area
                float delta = hookDistance - _defaultLength;
                if (delta < 0f) {
                    delta *= -1f;
                }

                float f = delta / _retractingSmoothLength;
                f = 1f - Mathf.Cos(f * Mathf.PI * 0.5f);
                f *= f;

                return Mathf.Lerp(_retractingStrength, _retractingStrength * 0.4f, f);
            }*/

        /*float hookPercentage = hookDistance * 100 / _defaultLength;
        Debug.Log("HookPercentage :"+ hookPercentage);
        if (hookPercentage > 100 && hookPercentage < 100 + _elasticSmoothPercentage) {//Trigger the smooth area
            float s = Mathf.Lerp(_retractingStrength, _retractingStrength * 0.2f, (hookPercentage - 100) / _elasticSmoothPercentage);
            Debug.Log("Strength: "+ s);
            return s;
        }


        if (hookDistance > _defaultLength) {
            return _retractingStrength;
        }

        return 0f;

    }*/


        public void TriggerHook(ISurfControllable surfer) {
            if (Physics.Raycast(_playerCam.transform.position, _playerCam.transform.forward, out RaycastHit hit, _maxDistance)) {
                //_isHooked = true;
                surfer.PlayerData.HookedPosition = hit.point;
                _lr.enabled = true;
                _lr.SetPosition(0, _lr.transform.position);
                _lr.SetPosition(1, surfer.PlayerData.HookedPosition);
            }
            _defaultLength = Vector3.Distance(surfer.PlayerData.Origin, surfer.PlayerData.HookedPosition) /* _defaultLengthPercent*/;
        }

        private void ReleaseHook(ISurfControllable surfer) {
            _status = HookStatus.None;
            _lr.enabled = false;
            surfer.PlayerData.HookedPosition = Vector3.zero;
        }

        public HookStatus GetStatus() {
            return _status;
        }
    }
}