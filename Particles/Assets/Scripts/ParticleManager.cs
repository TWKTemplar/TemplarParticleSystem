using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [Header("settings")]
    public float ShpereSize = 0.1f;
    public float Gravity = 0.01f;
    public float BiasToCenter = 0.1f;
    public float Drag;
    [Header("Random particles")]
    public float RandomRange;
    public int NumberOfParticlesToSpawn;
    [Header("Data")]
    public List<Particle> Particles;
    public ParticleType[] particleTypes;
    void Start()
    {
        SpawnRandomParticles();
    }
    private void FixedUpdate()
    {
        if (NumberOfParticlesToSpawn != 0) SpawnRandomParticles();
        else UpdateParticleForcesV2();
    }
    public void UpdateParticleForcesV2()
    {
        //Calc Forces for this frame
        foreach (var TargetParticle in Particles)
        {
           foreach (var OtherParticles in Particles) // how to treat others
           {
               var dist = Dist(TargetParticle, OtherParticles);
               var dir = FromTo(TargetParticle, OtherParticles).normalized;
               if (TargetParticle.color == OtherParticles.color)
               {
                   TargetParticle.Velocity = TargetParticle.Velocity + dir * Gravity * 0.1f * Influance(dist);//How to treat friends
                   TargetParticle.Velocity = TargetParticle.Velocity + (TargetParticle.Velocity.normalized + OtherParticles.Velocity.normalized) * 0.01f;
                   if (OtherParticles.color == Color.blue) TargetParticle.Velocity = TargetParticle.Velocity + dir * Gravity * 1f;
               }
               else TargetParticle.Velocity = TargetParticle.Velocity - dir * Gravity * 0.8f * Influance(dist);//How to treat others
           
           }
           //Bias to center
           var dirToCenter = FromToG(TargetParticle.gameObject, gameObject).normalized;
           var distToCenter = DistG(TargetParticle.gameObject, gameObject);
           TargetParticle.Velocity += dirToCenter * Gravity * 0.1f * distToCenter * Mathf.Sin(Time.time * 2f);

            
           TranslateBasedOnVelocity(TargetParticle);
        }
    }
    public void UpdateParticleForcesV1()
    {
        //Calc Forces for this frame
        foreach (var TargetParticle in Particles)
        {
            if(TargetParticle.color == Color.red)
            {
                foreach (var OtherParticles in Particles) // how to treat others
                {
                    var dist = Dist(TargetParticle, OtherParticles);
                    var dir = FromTo(TargetParticle, OtherParticles).normalized;
                    if (TargetParticle.color == OtherParticles.color)
                    {
                        TargetParticle.Velocity = TargetParticle.Velocity + dir * Gravity * 0.1f * Influance(dist);//How to treat friends
                        TargetParticle.Velocity = TargetParticle.Velocity + (TargetParticle.Velocity.normalized + OtherParticles.Velocity.normalized) * 0.01f;
                        if(OtherParticles.color == Color.blue) TargetParticle.Velocity = TargetParticle.Velocity + dir * Gravity * 1f;
                    }
                    else TargetParticle.Velocity = TargetParticle.Velocity - dir * Gravity * 0.8f * Influance(dist);//How to treat others
                }
                //Bias to center
                var dirToCenter = FromToG(TargetParticle.gameObject, gameObject).normalized;
                var distToCenter = DistG(TargetParticle.gameObject, gameObject);
                TargetParticle.Velocity += dirToCenter * Gravity * 0.05f * distToCenter;
                
            }
            else if (TargetParticle.color == Color.green)
            {
                foreach (var OtherParticles in Particles) // how to treat others
                {
                    var dist = Dist(TargetParticle, OtherParticles);
                    var dir = FromTo(TargetParticle, OtherParticles).normalized;
                    if (TargetParticle.color == OtherParticles.color) TargetParticle.Velocity = TargetParticle.Velocity - dir * Gravity * 0.6f * Influance(dist);//How to treat friends
                    else
                    {
                        TargetParticle.Velocity = TargetParticle.Velocity + dir * Gravity * 5f * Influance(dist);//How to treat others
                        if(OtherParticles.color == Color.blue) TargetParticle.Velocity = TargetParticle.Velocity + dir * Gravity * 8f * Influance(dist);//How to treat others
                    } 
                }
                //Bias to center
                var dirToCenter = FromToG(TargetParticle.gameObject, gameObject).normalized;
                var distToCenter = DistG(TargetParticle.gameObject, gameObject);
                TargetParticle.Velocity += dirToCenter * Gravity * 0.1f * distToCenter;
            }
            else//blue
            {
                foreach (var OtherParticles in Particles) // how to treat others
                {
                    var dist = Dist(TargetParticle, OtherParticles);
                    var dir = FromTo(TargetParticle, OtherParticles).normalized;
                    if (TargetParticle.color == OtherParticles.color) TargetParticle.Velocity = TargetParticle.Velocity - dir * Gravity * 0.1f * Influance(dist) * Mathf.Cos(Time.time * 2);//How to treat friends
                    else
                    {
                        TargetParticle.Velocity = TargetParticle.Velocity + (dir * Gravity * 0.1f * Influance(dist) * Mathf.Cos(Time.time * 2));//How to treat others
                        if(OtherParticles.color == Color.red) TargetParticle.Velocity = TargetParticle.Velocity + (dir * Gravity * 0.1f * Mathf.Cos(Time.time * 2));//How to treat others

                    }
                }
                //Bias to center
                var dirToCenter = FromToG(TargetParticle.gameObject, gameObject).normalized;
                var distToCenter = DistG(TargetParticle.gameObject, gameObject);
                TargetParticle.Velocity += dirToCenter * Gravity * 0.1f * distToCenter * Mathf.Sin(Time.time * 2f);

            }
            TranslateBasedOnVelocity(TargetParticle);
        }
    }
    public void TranslateBasedOnVelocity(Particle TargetParticle)
    {
        TargetParticle.Velocity = TargetParticle.Velocity * (1 - Drag);
        TargetParticle.transform.position = TargetParticle.transform.position + TargetParticle.Velocity * 0.1f;
    }
    public float Influance(float dist)
    {
        return Mathf.Clamp( 1 / dist, 0, 1);
    }
    public float Dist(Particle From, Particle To)
    {
        return Vector3.Distance(From.transform.position, To.transform.position);
    }
    public float DistG(GameObject From, GameObject To)
    {
        return Vector3.Distance(From.transform.position, To.transform.position);
    }
    public Vector3 FromTo(Particle From, Particle To)
    {
        return To.transform.position - From.transform.position;
    }
    public Vector3 FromToG(GameObject From, GameObject To)
    {
        return To.transform.position - From.transform.position;
    }
    void SpawnRandomParticles()
    {
        NumberOfParticlesToSpawn -= 1;
        var EmptyObj = new GameObject("particle");
        var particle = EmptyObj.AddComponent<Particle>();
        EmptyObj.transform.parent = transform;
        var ran = Random.Range(0, particleTypes.Length);
        particle.particleType = particleTypes[ran];
        particle.color = particleTypes[ran].color;
        particle.transform.position = new Vector3(Random.Range(-RandomRange, RandomRange), Random.Range(-RandomRange, RandomRange), 0);
        Particles.Add(particle);
    }
    public void OnDrawGizmos()
    {
        foreach (var particle in Particles)
        {
            Gizmos.color = particle.color;
            Gizmos.DrawSphere(particle.transform.position, ShpereSize);
            Gizmos.DrawLine(particle.transform.position, particle.transform.position + particle.Velocity);
        }
    }
}
