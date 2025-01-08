using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float lifetime = 3f;
    public float moveSpeed = 4f;
    public float disappearTimer = 2f;

    private TextMeshPro tmp;

    void Start()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - disappearSpeed * Time.deltaTime);
            if (tmp.color.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
