using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private bool isMoving;
    private LayerMask obstacleMask;
    private SpriteRenderer spriteRenderer;
    private Vector2 targetPositionAttemptingToMoveTo;    

    public float Speed;

    // Start is called before the first frame update
    void Start()
    {
        obstacleMask = LayerMask.GetMask("Wall", "Enemy");

        var gfx = GameObject.Find("GFX");
        spriteRenderer = gfx.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        EvaluateInput();        
    }

    private void EvaluateInput()
    {
        float horizontal = System.Math.Sign(Input.GetAxisRaw("Horizontal"));
        float vertical = System.Math.Sign(Input.GetAxisRaw("Vertical"));
        if (NoInputReceived(horizontal, vertical) || isMoving)
            return;

        EvaluatePlayerFlip(horizontal);
        CalculateTargetPosition(horizontal, vertical);
        EvaluteCollisions();
    }

    private static bool NoInputReceived(float horizontal, float vertical)
    {
        return Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0;
    }

    private void CalculateTargetPosition(float horizontal, float vertical)
    {
        if (Mathf.Abs(horizontal) > 0)
        {
            targetPositionAttemptingToMoveTo = 
                new Vector2(transform.position.x + horizontal, transform.position.y);
        }
        else if (Mathf.Abs(vertical) > 0)
        {
            targetPositionAttemptingToMoveTo = 
                new Vector2(transform.position.x, transform.position.y + vertical);
        }
    }

    private void EvaluatePlayerFlip(float horizontal)
    {
        if (horizontal == -1)
        {
            spriteRenderer.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (horizontal == 1)
        {
            spriteRenderer.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void EvaluteCollisions()
    {
        Vector2 hitSize = Vector2.one * 0.8f;
        float angle = 0f;        
        Collider2D hit = Physics2D.OverlapBox(targetPositionAttemptingToMoveTo, hitSize, angle, obstacleMask);
        if (!hit)
        {
            StartCoroutine(MovePlayer());
        }
    }

    private IEnumerator MovePlayer()
    {
        isMoving = true;        

        while(Vector2.Distance(transform.position, targetPositionAttemptingToMoveTo) > 0.01f)
        {
            float speedDelta = Speed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(
                transform.position, 
                targetPositionAttemptingToMoveTo, 
                speedDelta);

            yield return null;
        }

        transform.position = targetPositionAttemptingToMoveTo;
        isMoving = false;
    }
}
