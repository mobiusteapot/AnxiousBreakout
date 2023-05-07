using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SelectNextSphere : XRGrabInteractable
{
    public Action nextSphereSelected;
    [HideInInspector]
    public Material selectMaterial;
    private Material defaultMaterial;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterial = meshRenderer.sharedMaterial;
    }
    
    IEnumerator OnSelectedThenHighlight()
    {
        meshRenderer.material = selectMaterial;
        yield return new WaitForSeconds(1);
        meshRenderer.material = defaultMaterial;
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        nextSphereSelected?.Invoke();
        StartCoroutine(OnSelectedThenHighlight());
    }
}
