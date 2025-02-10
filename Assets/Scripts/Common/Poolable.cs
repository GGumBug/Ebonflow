using UnityEngine;

public class Poolable : MonoBehaviour
{
    public virtual void ResetState()
    {
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(false);
    }
}