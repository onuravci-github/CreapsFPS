using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointOnGizmos : MonoBehaviour
{
    
#if UNITY_EDITOR

    #region Variables

    [SerializeField] private List<Transform> _gizmosObjects;
    [Range(0.1f,5f)][SerializeField] private float _size = 1f;
    [SerializeField] private Color _color;

    #endregion

    #region Updates

    void OnDrawGizmos()
    {
        if (_gizmosObjects.Count < 0)
        {
            return;
        }
        
        Gizmos.color = _color;
        foreach (var gizmosObj in _gizmosObjects)
        {
            Gizmos.DrawSphere(gizmosObj.position, _size);
        }
    }

    #endregion
   
#endif
}
