using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [Header("settings")]
    public float ShpereSize = 0.1f;
    public float Gravity = 0.01f;
    public float BiasToCenter = 0.1f;
    public float MinDistanceThreashold = 1;
    public float MinDistanceForce = 1;
    public float Drag;
    public Vector3 BoundingBox;
    [Header("Random particles")]
    public int NumberOfParticlesToSpawn;
    [Header("Data")]
    public List<Particle> Particles;
    public ParticleType[] particleTypes;
    [Header("Gizmos")]
    public float VelocityGizmoLength = 1;
    [Header("MinMax")]
    float maxX;
    float minX;
    float maxY;
    float minY;
    private void Update()
    {
        if (NumberOfParticlesToSpawn != 0) SpawnRandomParticles();
        else
        {
            CalcMinMaxXY();
            UpdateParticleForcesV2();
        }
            
    }
    public void UpdateParticleForcesV2()
    {
        //Calc Forces for this frame
        foreach (var TargetParticle in Particles)//For every particle
        {
            #region Forces
            foreach (var OtherParticles in Particles)//apply rules by comparing nearby particles
           {
               var dist = Dist(TargetParticle, OtherParticles);
               var influance = CalcInfluance(dist);
               if (influance == 0) continue;

               var dirToOtherParticle = FromTo(TargetParticle, OtherParticles).normalized;
                
                for (int i = 0; i < TargetParticle.particleType.preferences.Length; i++)
                {
                    if (TargetParticle.particleType.preferences[i] == OtherParticles.particleType)
                    {
                        float sin = 1;//Will default to 1 if sin is disabled 
                        if (TargetParticle.particleType.preferencesSin[i]) sin = Mathf.Sin(Time.deltaTime);
                        //MinDistance
                        if(dist < MinDistanceThreashold) AddVel(TargetParticle, -dirToOtherParticle * Gravity * TargetParticle.particleType.preferencesForce[i] * MinDistanceForce);

                        //move to other particle
                        if (TargetParticle.particleType.preferencesForce[i] != 0)
                            AddVel(TargetParticle, dirToOtherParticle * Gravity * influance * TargetParticle.particleType.preferencesForce[i] * sin);
                        //converge to similar velocity
                        if(TargetParticle.particleType.MatchAvgDir[i] != 0) 
                            AddVel(TargetParticle, (TargetParticle.Velocity.normalized + OtherParticles.Velocity.normalized) * 0.1f * TargetParticle.particleType.MatchAvgDir[i]);
                        break;
                    }
                }
           }
            #region Bias to center
            var dirToCenter = FromToG(TargetParticle.gameObject, gameObject).normalized;
            var distToCenter = DistG(TargetParticle.gameObject, gameObject);
            AddVel(TargetParticle,dirToCenter * Gravity * BiasToCenter * distToCenter);
            #endregion
            TranslateBasedOnVelocity(TargetParticle);
            #endregion

            #region xy seamless loop


            if (TargetParticle.transform.position.x > maxX)
            {
                Vector3 newpos = new Vector3(TargetParticle.transform.position.x - BoundingBox.x * 0.99f, TargetParticle.transform.position.y, TargetParticle.transform.position.z);
                TargetParticle.transform.position = newpos;
            }
            if (TargetParticle.transform.position.x < minX)
            {
                Vector3 newpos = new Vector3(TargetParticle.transform.position.x + BoundingBox.x * 0.99f, TargetParticle.transform.position.y, TargetParticle.transform.position.z);
                TargetParticle.transform.position = newpos;
            }
            if (TargetParticle.transform.position.y > maxY)
            {
                Vector3 newpos = new Vector3(TargetParticle.transform.position.x, TargetParticle.transform.position.y - BoundingBox.y * 0.99f, TargetParticle.transform.position.z);
                TargetParticle.transform.position = newpos;
            }
            if (TargetParticle.transform.position.y < minY)
            {
                Vector3 newpos = new Vector3(TargetParticle.transform.position.x, TargetParticle.transform.position.y + BoundingBox.y * 0.99f, TargetParticle.transform.position.z);
                TargetParticle.transform.position = newpos;
            }
            #endregion


        }
    }
    #region Particle Functions
    public void AddVel(Particle particle, Vector3 VectorToAdd)
    {
        particle.Velocity = particle.Velocity + VectorToAdd;
    }
    public void TranslateBasedOnVelocity(Particle TargetParticle)
    {
        TargetParticle.Velocity = TargetParticle.Velocity * (1 - Drag);
        TargetParticle.transform.position = TargetParticle.transform.position + TargetParticle.Velocity * 0.1f;
    }
    #endregion
    #region Math
    public void CalcMinMaxXY()
    {
        maxX = transform.position.x + BoundingBox.x * 0.5f;
        minX = transform.position.x - BoundingBox.x * 0.5f;
        maxY = transform.position.y + BoundingBox.y * 0.5f;
        minY = transform.position.y - BoundingBox.y * 0.5f;
    }
    public float CalcInfluance(float dist)
    {
        return Mathf.Clamp( (-dist*0.2f) + 1, 0, 1);
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
    #endregion
    public void SpawnRandomParticles()
    {
        NumberOfParticlesToSpawn -= 1;
        var EmptyObj = new GameObject("particle");
        var particle = EmptyObj.AddComponent<Particle>();
        EmptyObj.transform.parent = transform;
        var ran = Random.Range(0, particleTypes.Length);
        particle.particleType = particleTypes[ran];
        particle.color = particleTypes[ran].color;
        particle.transform.position = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);
        Particles.Add(particle);
    }
    public void OnDrawGizmos()
    {
        #if UNITY_EDITOR
          CalcMinMaxXY();
        #endif

        foreach (var particle in Particles)
        {
            Gizmos.color = particle.color;
            Gizmos.DrawSphere(particle.transform.position, ShpereSize);
            Gizmos.DrawLine(particle.transform.position, particle.transform.position + particle.Velocity * VelocityGizmoLength);

            #region Ghost Particles
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(particle.transform.position + BoundingBox.x * Vector3.right, ShpereSize);//Right
            Gizmos.DrawSphere(particle.transform.position + BoundingBox.x * Vector3.left, ShpereSize);//Left
            Gizmos.DrawSphere(particle.transform.position + BoundingBox.y * Vector3.up, ShpereSize);//up
            Gizmos.DrawSphere(particle.transform.position + BoundingBox.y * Vector3.down, ShpereSize);//down
            #endregion


        }


        //Draw bounding box
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + maxX * Vector3.left + maxY * Vector3.up, transform.position + maxX * Vector3.right + maxY * Vector3.up);
        Gizmos.DrawLine(transform.position + maxX * Vector3.left + maxY * Vector3.down, transform.position + maxX * Vector3.right + maxY * Vector3.down);
        Gizmos.DrawLine(transform.position + maxX * Vector3.right + maxY * Vector3.up, transform.position + maxX * Vector3.right + maxY * Vector3.down);
        Gizmos.DrawLine(transform.position + maxX * Vector3.left + maxY * Vector3.up, transform.position + maxX * Vector3.left + maxY * Vector3.down);
    }
}
