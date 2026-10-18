using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public GameObject particlePrefab;
    public Transform spawnPoint;
    public Sprite image;
    public float size;
    public int particleAmount = 5;
    public float lifetime = 3f;

    public void Trigger()
    {
        for (int i = 0; i < particleAmount; i++)
        {
            GameObject particleObj = Instantiate(particlePrefab, spawnPoint.position, Quaternion.identity);
            
            Particle particle = particleObj.GetComponent<Particle>();
            particle.Initialize(image, size, lifetime);

        }
    }
}