using UnityEngine;

public class CarInputTrigger : MonoBehaviour
{

    public GameObject CarCameraRoot;
    public Transform CarCameraRootTransform;
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        Vector3 currentPos = CarCameraRoot.transform.position;
        Vector3 targetPos = CarCameraRootTransform.position;
        CarCameraRoot.transform.position = new Vector3(targetPos.x, currentPos.y, targetPos.z);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (PlayerPhotonSigleton.Instance != null)
                {

                    PlayerPhotonSigleton.Instance.cCam.Follow = CarCameraRoot.transform;
                    Debug.Log("플레이어 카메라의 Follow가 CarCameraRoot로 변경되었습니다.");
                    SAT1Controller satController = GetComponentInChildren<SAT1Controller>();
                    if (satController != null)
                    {
                        satController.CarActivate = true;
                        Debug.Log("SAT1Controller의 CarActivate가 true로 변경되었습니다.");
                    }
                    else
                    {
                        Debug.LogWarning("자식 오브젝트에서 SAT1Controller 컴포넌트를 찾지 못했습니다.");
                    }
                }
            }
        }
    }
}
