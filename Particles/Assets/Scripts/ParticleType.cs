using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "new ParticleType", menuName = "Particle Type")]
public class ParticleType : ScriptableObject
{
    public Color color = Color.red;
    public ParticleType[] preferences;
    public float[] preferencesForce;
    public float[] MatchAvgDir;
    public bool[] preferencesSin;
    private void OnValidate()
    {
        Object[] particletypesObj = Resources.LoadAll("ParticleTypes");
        if (preferences.Length != particletypesObj.Length)
        {
            preferences = new ParticleType[particletypesObj.Length];
            particletypesObj.CopyTo(preferences, 0);
            preferencesForce = new float[particletypesObj.Length];
            MatchAvgDir = new float[particletypesObj.Length];
            preferencesSin = new bool[particletypesObj.Length];
            foreach (var particletype in preferences)
            {
                particletype.UpdateArrays();
            }
        }
    }
    public void UpdateArrays()
    {
        Object[] particletypesObj = Resources.LoadAll("ParticleTypes");
        preferences = new ParticleType[particletypesObj.Length];
        particletypesObj.CopyTo(preferences, 0);
        preferencesForce = new float[particletypesObj.Length];
        MatchAvgDir = new float[particletypesObj.Length];
        preferencesSin = new bool[particletypesObj.Length];
    }
}
