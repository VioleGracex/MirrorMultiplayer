using UnityEngine;

/// <summary>
/// Makes the attached game object (e.g. a world-space UI) always face the main camera.
/// </summary>
public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}