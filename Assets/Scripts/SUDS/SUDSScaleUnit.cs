using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SUDSScaleUnit : XRGrabInteractable
{
    public int sudsUnitValue;
    public Action<SUDSScaleUnit> sudsUnitSelected;
    [HideInInspector]
    public Material selectMaterial;
    private Material defaultMaterial;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterial = meshRenderer.sharedMaterial;
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        sudsUnitSelected?.Invoke(this);
        StartCoroutine(OnSelectedThenHighlight());
    }

    IEnumerator OnSelectedThenHighlight()
    {
        meshRenderer.material = selectMaterial;
        yield return new WaitForSeconds(0.7f);
        meshRenderer.material = defaultMaterial;
    }

}
