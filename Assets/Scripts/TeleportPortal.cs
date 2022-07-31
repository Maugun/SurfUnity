using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace P90brush
{
    public class TeleportPortal : MonoBehaviour
    {

        public Transform _outPortal;
        // Start is called before the first frame update
        void Start() {

        }

        private void OnTriggerEnter(Collider other) {
            ISurfControllable surfer = other.GetComponent<ISurfControllable>();
            if (surfer != null) {
                float mag = surfer.PlayerData.Velocity.magnitude;

                //Set Position
                surfer.PlayerData.Origin = _outPortal.transform.position;

                //Set Camera angle
                surfer.PlayerData.ViewAngles.x = (_outPortal.transform.eulerAngles.x - 360f) % 360f;
                surfer.PlayerData.ViewAngles.y = _outPortal.transform.eulerAngles.y;

                //Convert Velocity
                //Todo: Quick Math
            }
        }

        // Update is called once per frame
        void Update() {

        }
    }
}