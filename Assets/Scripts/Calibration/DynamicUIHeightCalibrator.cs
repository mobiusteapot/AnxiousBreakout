using UnityEngine;

public class DynamicUIHeightCalibrator : MonoBehaviour
{    
    public bool stayAtPlayerHeight;
    public float movementSpeed = 0.5f;
    public float thresholdToTriggerMovement = 0.1f;
    
    // Update is called once per frame
    void Update()
    {
        if (stayAtPlayerHeight)
        {
            AdjustUIHeightToPlayerHeight();
            return;
        }
    }


    Vector3 positionWithNewHeight;
    void AdjustUIHeightToPlayerHeight()
    {
        if (Mathf.Abs(Camera.main.transform.position.y - transform.position.y) <= thresholdToTriggerMovement)
            return;

        positionWithNewHeight = new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z);
        transform.position = Vector3.Slerp(transform.position, positionWithNewHeight, Time.deltaTime * movementSpeed);
    }
}
