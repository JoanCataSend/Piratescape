using UnityEngine;

public sealed class SistemaEnergiaJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private VisualizadorBarrasEstado visualizador;
    [SerializeField] private CharacterController characterController;

    [Header("Energia")]
    [SerializeField] private float energiaMaxima = 100f;
    [SerializeField] private float energiaActual = 100f;

    [Header("Configuracion desgaste")]
    [SerializeField] private float perdidaEnergiaPorSegundoCorriendo = 10f;

    [Header("Deteccion de carrera")]
    [SerializeField] private float umbralMovimiento = 0.1f;

    private bool estaAgotado;

    public float EnergiaActual => energiaActual;
    public float EnergiaMaxima => energiaMaxima;
    public bool EstaAgotado => estaAgotado;

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    private void Start()
    {
        energiaMaxima = Mathf.Max(0f, energiaMaxima);
        energiaActual = Mathf.Clamp(energiaActual, 0f, energiaMaxima);

        ActualizarHUD();
        ComprobarAgotamiento();
    }

    private void Update()
    {
        if (estaAgotado)
        {
            return;
        }

        if (EstaCorriendoDeVerdad())
        {
            ReducirEnergiaPorCorrer();
        }
    }

    private bool EstaCorriendoDeVerdad()
    {
        bool shiftPulsado = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (!shiftPulsado)
        {
            return false;
        }

        if (characterController != null)
        {
            Vector3 velocidadHorizontal = characterController.velocity;
            velocidadHorizontal.y = 0f;

            return velocidadHorizontal.magnitude > umbralMovimiento;
        }

        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");
        Vector2 inputMovimiento = new Vector2(inputHorizontal, inputVertical);

        return inputMovimiento.sqrMagnitude > 0.01f;
    }

    private void ReducirEnergiaPorCorrer()
    {
        energiaActual -= perdidaEnergiaPorSegundoCorriendo * Time.deltaTime;
        energiaActual = Mathf.Clamp(energiaActual, 0f, energiaMaxima);

        ActualizarHUD();
        ComprobarAgotamiento();
    }

    public void ReducirEnergiaDirecta(float cantidad)
    {
        if (cantidad <= 0f || estaAgotado)
        {
            return;
        }

        energiaActual -= cantidad;
        energiaActual = Mathf.Clamp(energiaActual, 0f, energiaMaxima);

        ActualizarHUD();
        ComprobarAgotamiento();
    }

    public void AumentarEnergia(float cantidad)
    {
        if (cantidad <= 0f)
        {
            return;
        }

        energiaActual += cantidad;
        energiaActual = Mathf.Clamp(energiaActual, 0f, energiaMaxima);

        ActualizarHUD();
        ComprobarAgotamiento();
    }

    public void RestaurarEnergiaCompleta()
    {
        energiaActual = energiaMaxima;
        estaAgotado = false;
        ActualizarHUD();
    }

    private void ActualizarHUD()
    {
        if (visualizador != null)
        {
            visualizador.EstablecerEnergia(energiaActual, energiaMaxima);
        }
    }

    private void ComprobarAgotamiento()
    {
        estaAgotado = energiaActual <= 0f;
    }
}