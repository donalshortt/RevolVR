using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Click : MonoBehaviour
{
    private XRSimpleInteractable interactable;
    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();


    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEnter);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEnter);
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log($"Selected object: {gameObject.name}");
    }
}
