using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _body;
    [SerializeField] private InputAction _moveAction;
    private bool _isLinked = false;
    private void Awake()
    {
        if (_body == null)
        {
            _body = GetComponent<Rigidbody>();
        }
    }

    private void OnEnable()
    {
        LinkControls(true);
    }
    private void OnDisable()
    {
        LinkControls(false);
    }
    public void LinkControls(bool linkUp)
        => StartCoroutine(linkUp ? Link() : UnLink());
    private IEnumerator Link()
    {
        bool unableToLink = false;
        while (!_isLinked && !unableToLink)
        {
            if (GameManager.Instance != null)
            {
                try
                {
                    GameManager.Instance.PlayerInput.actions["Move"].performed += OnMove;
                    GameManager.Instance.PlayerInput.actions["Move"].canceled  += OnMove;
                    _isLinked = true;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    _isLinked = false;
                }
            }
            else
            {
                _isLinked = false;
                unableToLink = true;
            }
            yield return null;
        }
    }
    private IEnumerator UnLink()
    {
        while (_isLinked)
        {
            if (GameManager.Instance != null)
            {
                try
                {
                    GameManager.Instance.PlayerInput.actions["Move"].performed -= OnMove;
                    GameManager.Instance.PlayerInput.actions["Move"].canceled  -= OnMove;
                    _isLinked = false;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    _isLinked = true;
                }
            }
            else
            {
                _isLinked = false;
            }
            yield return null;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {

    }
}
