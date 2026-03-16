/// <summary>
/// Contrato base de cualquier objeto interactuable.
/// </summary>
public interface Interactuable
{
    void Interactuar();

    float Rango { get; set; }

    bool Activo { get; }
}