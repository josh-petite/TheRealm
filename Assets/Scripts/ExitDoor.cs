using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class ExitDoor : MonoBehaviour
{
    void Reset()
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.isKinematic = true;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.size = Vector2.one * 0.1f;
        box.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (string.Equals("Player", other.tag, System.StringComparison.InvariantCulture))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
