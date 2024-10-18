using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerInput PlayerInput { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (PlayerInput == null)
        {
            PlayerInput = GetComponent<PlayerInput>();
        }
    }
    public static void ChangeActionMap(string name)
        => Instance.PlayerInput.SwitchCurrentActionMap(name);
}
