using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public GameObject particlePrefab;
    public Transform spawnPoint;
    public float size;

    public int Wisdom = 4;

    public float lifetime = 3f;

    private Sprite image; 

    public void Start(){
        

    }

    public void Trigger()
    {
        
        if (GameData.IsUpgradeOwned("Autoclicker"))
        {
            size = 0.03f;
            image = Resources.Load<Sprite>("ChatGPT-Logo");
            GameObject particleObj = Instantiate(particlePrefab, spawnPoint.position, Quaternion.identity);
            
            Particle particle = particleObj.GetComponent<Particle>();
            particle.Initialize(image, size, lifetime);
            Wisdom -= 1;

        }
        if (GameData.IsUpgradeOwned("QuadrupleClick"))
        {
            size = 0.05f;
            image = Resources.Load<Sprite>("Claude-Logo");
            GameObject particleObj = Instantiate(particlePrefab, spawnPoint.position, Quaternion.identity);
            
            Particle particle = particleObj.GetComponent<Particle>();
            particle.Initialize(image, size, lifetime);
            Wisdom -= 1;


        }
        if (GameData.IsUpgradeOwned("DoubleClick"))
        {
            size = 0.2f;
            image = Resources.Load<Sprite>("Gemini-Logo");
            GameObject particleObj = Instantiate(particlePrefab, spawnPoint.position, Quaternion.identity);
            
            Particle particle = particleObj.GetComponent<Particle>();
            particle.Initialize(image, size, lifetime);
            Wisdom -= 1;


        }
        if (GameData.IsUpgradeOwned("TripleClick"))
        {
            size = 0.03f;
            image = Resources.Load<Sprite>("Copilot-Logo");
            GameObject particleObj = Instantiate(particlePrefab, spawnPoint.position, Quaternion.identity);
            
            Particle particle = particleObj.GetComponent<Particle>();
            particle.Initialize(image, size, lifetime);
            Wisdom -= 1;

        }
        for (int i = 0; i < Wisdom; i++)
        {
            size = 0.07f;
            image = Resources.Load<Sprite>("Wiki-Logo");
            GameObject particleObj = Instantiate(particlePrefab, spawnPoint.position, Quaternion.identity);
            
            Particle particle = particleObj.GetComponent<Particle>();
            particle.Initialize(image, size, lifetime);

        }
    }
}
