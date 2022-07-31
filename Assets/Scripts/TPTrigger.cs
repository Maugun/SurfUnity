using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPTrigger : MonoBehaviour
{
    public Vector3 _tpPos;
    public List<string> _tagList = new List<string>();

    private Transform _player;
    private bool _tpPlayer;

    private void Start()
    {
        _player = GameObject.Find("Player").transform;
        _tpPlayer = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.Equals(_player))
        {
            Debug.Log("player");
            _tpPlayer = true;
            return;
        }

        foreach (string tag in _tagList)
        {
            if (other.transform.root.tag == tag)
            {
                Debug.Log("other");
                other.transform.position = _tpPos;
                // TODO: Add Rotation ?
                break;
            }
        } 
    }

    private void LateUpdate()
    {
        if (_tpPlayer)
        {
            Debug.Log("late");
            _player.position = _tpPos;
            // TODO: Add Rotation ?
            _tpPlayer = false;
        }
    }
}
