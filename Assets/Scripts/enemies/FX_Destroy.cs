using System.Collections;
using UnityEngine;

public class FX_Destroy : MonoBehaviour
{
    [SerializeField]
    private float _delay = 3f;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (_delay > 0f)
        {
            yield return new WaitForSeconds(_delay);
            GameObject.Destroy(gameObject);
        }
    }
}
