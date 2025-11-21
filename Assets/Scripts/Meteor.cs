using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float destroyY = -10f;

    void Update()
    {
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
