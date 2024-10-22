using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerInput PlayerInput { get; private set; }
    public InputActionMap[] ActionMaps { get; private set; }
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

        int mapCount = Instance.PlayerInput.actions.actionMaps.Count;
        ActionMaps = new InputActionMap[mapCount];
        for (int i = 0; i < mapCount; i++)
        {
            ActionMaps[i] = Instance.PlayerInput.actions.actionMaps[i];
        }
    }
    private void Start()
    {
        // this is required to "reset" the action maps as for some reason they decide to have multiple actions on different maps be active and listening at the same time so if we do this where we change it to something different then back it will reset it.
        StartCoroutine(ResetActionMap());
    }
    private IEnumerator ResetActionMap()
    {
        yield return null;
        ChangeActionMap(Instance.PlayerInput.actions.actionMaps[1].name);
        yield return null;
        ChangeActionMap(Instance.PlayerInput.actions.actionMaps[0].name);
    }
    public void ChangeActionMap(string name)
        => Instance.PlayerInput.SwitchCurrentActionMap(name);
}
