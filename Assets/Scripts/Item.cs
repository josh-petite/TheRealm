using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Item : MonoBehaviour
{
    void Reset()
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.isKinematic = true;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.size = Vector2.one * 0.8f;
        box.isTrigger = true;
    }
}
