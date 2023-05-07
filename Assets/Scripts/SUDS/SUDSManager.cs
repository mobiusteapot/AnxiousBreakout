using System.Collections;
using UnityEngine;

public class SUDSManager : MonoBehaviour
{
    
    public PartyEvent SUDSpartyEvent;
    public float delayBeforeSelectActive = 1f;
    public float delayAfterSelect = 1f;
    public GameObject instructionCube, sudsScale, levelOfNervousness;
    [Tooltip("Material to display after SUDS has been selected")]
    public Material selectMaterial;


    #region SUDS Fake Calibration
    [Space(20)]
    [Header("Fake Calibration")]

    public GameObject nextSphere;
    public GameObject helloPleaseTapInstruction;

    public void FakeCalibration(bool enabledStatus)
    {
        nextSphere.SetActive(enabledStatus);
        helloPleaseTapInstruction.SetActive(enabledStatus);
    }


    public void SelectNextSphere()
    {
        StartCoroutine(DelayNextSphereBeforeClose(delayAfterSelect));
    }

    public void CloseStarterUI()
    {
        FakeCalibration(false);

        sudsState = SUDSState.Genuine;
        StartSUDS(transform.position);
    }

    #endregion



    public enum SUDSState
    {
        FakeCalibration,
        Genuine
    }
    static SUDSState sudsState = SUDSState.FakeCalibration;

    

    private void OnEnable()
    {
        foreach (var item in FindObjectsOfType<SUDSScaleUnit>())
        {
            // Start disabled then enable
            item.GetComponent<SphereCollider>().enabled = false;
            item.selectMaterial = selectMaterial;
            item.sudsUnitSelected += SUDS_UnitSelected;            
        }
        SelectNextSphere nextSphereComponent = nextSphere.GetComponent<SelectNextSphere>();
        nextSphereComponent.nextSphereSelected += SelectNextSphere;
        nextSphereComponent.selectMaterial = selectMaterial;
    }



    private void OnDisable()
    {
        foreach (var item in FindObjectsOfType<SUDSScaleUnit>())
        {
            item.sudsUnitSelected -= SUDS_UnitSelected;
        }

        nextSphere.GetComponent<SelectNextSphere>().nextSphereSelected -= SelectNextSphere;

    }


    public void StartSUDS(Vector3 positionToPreserve = default)
    {
        //Get player position and spawn it in front of the player at a distance, unless giving override
        if (positionToPreserve != default)
        {
            transform.position = positionToPreserve;
        }
        else
        {
            Vector3 projectedVector = Vector3.ProjectOnPlane(PlayerManager.Instance.playerCenterEyeTransform.forward, Vector3.up);
            projectedVector = (projectedVector.normalized);            
            transform.position = PlayerManager.Instance.playerCenterEyeTransform.position + projectedVector;
        }

        StartCoroutine(ActivateButtonsAfterDelay(delayBeforeSelectActive));


        instructionCube.SetActive(true);


        switch (sudsState)
        {
            case SUDSState.FakeCalibration:                
                FakeCalibration(true);
                SUDSScaleEffect(false);
                break;

            case SUDSState.Genuine:
                OpenSUDS();
                break;         
        }        
    }



    void OpenSUDS()
    {
        SUDSScaleEffect(true);
    }


    void CloseSUDS()
    {
        instructionCube.SetActive(false);

        SUDSScaleEffect(false);

        PartySceneSingleton.Instance.currentPartyEvent.isComplete = true;
    }


    void SUDSScaleEffect(bool visiblity)
    {
        sudsScale.SetActive(visiblity);
        levelOfNervousness.SetActive(visiblity);        
    }

    
    void SUDS_UnitSelected(SUDSScaleUnit sudsScaleSelected)
    {
        StartCoroutine(DelaySudsBeforeClose(delayAfterSelect));
    }
    IEnumerator DelaySudsBeforeClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseSUDS();
        yield break;
    }
    IEnumerator DelayNextSphereBeforeClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseStarterUI();
        yield break;
    }

    IEnumerator ActivateButtonsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var item in FindObjectsOfType<SUDSScaleUnit>())
        {
            item.GetComponent<SphereCollider>().enabled = true;
        }
        yield break;
    }


    //Optional
    void WriteSUDSDataToFile()
    {

    }
}


