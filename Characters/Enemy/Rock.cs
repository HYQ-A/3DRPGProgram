using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates { HitEnemy, HitPlayer, HitNothing}

    private Rigidbody rb;
    public RockStates rockStates;

    [Header("Basic Setting")]
    public float force;
    public GameObject target;
    private Vector3 direction;
    public int damage;
    public GameObject breakEffect;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;

        rockStates = RockStates.HitPlayer;
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        if(rb.velocity.sqrMagnitude<1f)
        {
            rockStates = RockStates.HitNothing;
        }
    }

    private void FlyToTarget()
    {
        if (target == null)
            target = FindObjectOfType<PlayerController>().gameObject;

        direction = (target.transform.position - this.transform.position + Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (rockStates)
        {
            case RockStates.HitEnemy:
                if(collision.gameObject.GetComponent<Golem>())
                {
                    var otherStates = collision.gameObject.GetComponent<CharacterStats>();
                    otherStates.TakeDamage(damage, otherStates);
                    Instantiate(breakEffect, this.transform.position, Quaternion.identity);

                    Destroy(this.gameObject);
                }
                break;
            case RockStates.HitPlayer:
                if(collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, collision.gameObject.GetComponent<CharacterStats>());
                    rockStates = RockStates.HitNothing;
                }
                break;
            case RockStates.HitNothing:
                break;
        }
    }


}
