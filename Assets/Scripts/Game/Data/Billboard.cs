using UnityEngine;

[ExecuteAlways] // 에디터에서도 회전 미리보기
public class Billboard : MonoBehaviour
{
    public enum Mode { Full, YAxisOnly }

    [SerializeField] private Mode mode = Mode.YAxisOnly; 
    [SerializeField] private Camera targetCamera;        
    [SerializeField] private bool useMainCameraIfNull = true;
    [SerializeField] private bool updateInEditMode = true;
    [SerializeField] private bool flip180 = false;     

    private Transform _camT;

    private void OnEnable() => ResolveCamera();

    private void ResolveCamera()
    {
        if (targetCamera != null) { _camT = targetCamera.transform; return; }
        if (useMainCameraIfNull && Camera.main != null) _camT = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (!updateInEditMode && !Application.isPlaying) return;

        if (_camT == null) { ResolveCamera(); if (_camT == null) return; }

        if (mode == Mode.Full)
        {
           
            transform.LookAt(_camT, Vector3.up);
        }
        else
        {
            
            Vector3 dir = _camT.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        if (flip180) transform.Rotate(0f, 180f, 0f);
    }
}
