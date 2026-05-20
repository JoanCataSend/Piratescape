using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseObjectDatabase", menuName = "Gameplay/Guardado/Base Object Database")]
public sealed class BaseObjectDatabase : ScriptableObject
{
    [SerializeField] private List<ObjetoBaseData> objetosBase = new List<ObjetoBaseData>();

    public ObjetoBaseData BuscarPorId(string idObjeto)
    {
        if (string.IsNullOrWhiteSpace(idObjeto))
        {
            return null;
        }

        for (int i = 0; i < objetosBase.Count; i++)
        {
            ObjetoBaseData objeto = objetosBase[i];

            if (objeto == null)
            {
                continue;
            }

            if (objeto.IdObjeto == idObjeto)
            {
                return objeto;
            }
        }

        Debug.LogWarning("BaseObjectDatabase: no se encontro ningun objeto base con id " + idObjeto + ".");
        return null;
    }
}
