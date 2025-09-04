using UnityEngine;

public class ItemView : MonoBehaviour
{
    public Enums.ItemType Type;

    private void Awake()
    {
        var particles = GetComponentsInChildren<ParticleSystem>(true);

        foreach (var particle in particles)
        {
            var main = particle.main;   // скопировали структуру MainModule
            main.loop = false;
            particle.gameObject.SetActive(false);
        }
    }

    public void Invoke()
    {
        gameObject.SetActive(true);

        var transforms = GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.SetActive(true);
        }

        var particles = GetComponentsInChildren<ParticleSystem>(true);

        foreach (var particle in particles)
        {
            particle.Play();
        }
    }
}
