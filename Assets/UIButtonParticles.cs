using UnityEngine;

public class UIButtonParticles : MonoBehaviour
{
    //[SerializeField] private ParticleSystem clickParticles;
    //
    public ParticleSystem particleEffect;
    public RectTransform targetUI;


    public void PlayEffect()
    {
        particleEffect.transform.position = targetUI.position;
        particleEffect.Play();
    }
}