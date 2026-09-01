using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class SnakeDeathAnimation : MonoBehaviour
{
    [Header("Death Animation")]
    [SerializeField] private float collapseDuration = 0.25f;
    [SerializeField] private float cubePopScale = 1.18f;
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private float disappearDuration = 0.18f;
    [SerializeField] private float delayBetweenCubes = 0.04f;
    [SerializeField] private float scatterDistance = 0.35f;
    [SerializeField] private float headPunchScale = 0.2f;
    [SerializeField] private float headPunchDuration = 0.15f;

    private bool _isPlaying;
    private Sequence _deathSequence;

    public void PlayDeathAnimation(List<GameObject> cubes)
    {
        if (_isPlaying || cubes == null || cubes.Count == 0) return;

        _isPlaying = true;

        DisableGameplay();

        _deathSequence = DOTween.Sequence();

        Vector3 center = transform.position;

        // made by Bhargav
        for (int i = cubes.Count - 1; i >= 1; i--)
        {
            GameObject cube = cubes[i];

            if (cube == null) continue;

            _deathSequence.Join(cube.transform.DOMove(center, collapseDuration).SetEase(Ease.InQuad));
        }

        _deathSequence.AppendInterval(0.05f);

        // made by Bhargav
        for (int i = cubes.Count - 1; i >= 1; i--)
        {
            GameObject cube = cubes[i];

            if (cube == null) continue;

            Transform cubeTransform = cube.transform;

            float delay = (cubes.Count - 1 - i) * delayBetweenCubes;

            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

            Vector3 scatterPosition = cubeTransform.position + randomDirection * scatterDistance;

            Sequence cubeSequence = DOTween.Sequence();

            cubeSequence.AppendInterval(delay);
            cubeSequence.Append(cubeTransform.DOScale(Vector3.one * cubePopScale, popDuration).SetEase(Ease.OutBack));
            cubeSequence.Join(cubeTransform.DOMove(scatterPosition, popDuration).SetEase(Ease.OutQuad));
            cubeSequence.Append(cubeTransform.DOScale(Vector3.zero, disappearDuration).SetEase(Ease.InBack));

            _deathSequence.Join(cubeSequence);
        }

        // made by Bhargav
        _deathSequence.AppendInterval(0.05f);

        // made by Bhargav
        _deathSequence.Append(transform.DOPunchScale(Vector3.one * headPunchScale, headPunchDuration, 6, 0.5f));

        // made by Bhargav
        _deathSequence.Append(transform.DOScale(Vector3.zero, disappearDuration).SetEase(Ease.InBack));

        // made by Bhargav
        _deathSequence.OnComplete(() => gameObject.SetActive(false));
    }

    private void DisableGameplay()
    {
        // made by Bhargav
        PlayerMovementControl playerMovement = GetComponent<PlayerMovementControl>();
        if (playerMovement != null) playerMovement.enabled = false;

        // made by Bhargav
        BotMovement botMovement = GetComponent<BotMovement>();
        if (botMovement != null) botMovement.enabled = false;

        // made by Bhargav
        BotTargetDetector detector = GetComponent<BotTargetDetector>();
        if (detector != null) detector.enabled = false;

        // made by Bhargav
        BotCollisionAction botCollision = GetComponent<BotCollisionAction>();
        if (botCollision != null) botCollision.enabled = false;

        // made by Bhargav
        CollisionAction playerCollision = GetComponent<CollisionAction>();
        if (playerCollision != null) playerCollision.enabled = false;
    }

    private void OnDisable()
    {
        if (_deathSequence != null && _deathSequence.IsActive()) _deathSequence.Kill();
    }
}