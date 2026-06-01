using System.Collections;
using UnityEngine;

public class GhostVisualEffect : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer[] renderersFantasma;
    [SerializeField] private GameObject fxSpawnPrefab;

    [Header("Animación")]
    [SerializeField] private float duracionSpawn = 1f;
    [SerializeField] private float duracionDespawn = 1f;
    [SerializeField] private float escalaInicial = 0.6f;

    private Vector3 escalaOriginal;
    private bool animando;

    private void Awake()
    {
        escalaOriginal = transform.localScale;

        if (renderersFantasma == null || renderersFantasma.Length == 0)
        {
            renderersFantasma = GetComponentsInChildren<Renderer>(true);
        }
    }

    public void PlaySpawn()
    {
        if (animando)
            return;

        gameObject.SetActive(true);
        StartCoroutine(SpawnRoutine());
    }

    public void PlayDespawn()
    {
        if (animando)
            return;

        StartCoroutine(DespawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        animando = true;

        InstanciarFX();

        transform.localScale = escalaOriginal * escalaInicial;
        SetRenderersVisible(false);

        float t = 0f;

        while (t < duracionSpawn)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duracionSpawn);

            transform.localScale = Vector3.Lerp(
                escalaOriginal * escalaInicial,
                escalaOriginal,
                p
            );

            if (p > 0.25f)
            {
                SetRenderersVisible(true);
            }

            yield return null;
        }

        transform.localScale = escalaOriginal;
        SetRenderersVisible(true);

        animando = false;
    }

    private IEnumerator DespawnRoutine()
    {
        animando = true;

        InstanciarFX();

        float t = 0f;
        Vector3 escalaInicio = transform.localScale;

        while (t < duracionDespawn)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duracionDespawn);

            transform.localScale = Vector3.Lerp(
                escalaInicio,
                escalaOriginal * escalaInicial,
                p
            );

            yield return null;
        }

        SetRenderersVisible(false);
        gameObject.SetActive(false);

        transform.localScale = escalaOriginal;
        animando = false;
    }

    private void InstanciarFX()
    {
        if (fxSpawnPrefab == null)
            return;

        GameObject fx = Instantiate(
            fxSpawnPrefab,
            transform.position,
            Quaternion.identity
        );

        Destroy(fx, 3f);
    }

    private void SetRenderersVisible(bool visible)
    {
        for (int i = 0; i < renderersFantasma.Length; i++)
        {
            if (renderersFantasma[i] != null)
            {
                renderersFantasma[i].enabled = visible;
            }
        }
    }
}