using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _body;
    [SerializeField][ReadOnly] private bool _isLinked = false;
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
    {
        if (linkUp)
        {
            StartCoroutine(Link());
        }
        else
        {
            UnLink();
        }
    }
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
    private void UnLink()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerInput.actions["Move"].performed -= OnMove;
            GameManager.Instance.PlayerInput.actions["Move"].canceled  -= OnMove;
        }
        _isLinked = false;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {

    }
}
