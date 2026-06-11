using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class DatosPartida
{
    public bool tutorialCompletado;
    public DatosJugador jugador = new DatosJugador();
    public DatosTiempo tiempo = new DatosTiempo();
    public DatosEconomia economia = new DatosEconomia();
    public List<DatosSlotInventario> inventarioJugador = new List<DatosSlotInventario>();
    public List<DatosObjetoBase> objetosBase = new List<DatosObjetoBase>();
}

[Serializable]
public sealed class DatosJugador
{
    public Vector3 posicion;
    public Vector3 rotacionEuler;
    public int saludActual;
    public int energiaActual;
}

[Serializable]
public sealed class DatosTiempo
{
    public int dia;
    public int hora;
    public int minuto;
}

[Serializable]
public sealed class DatosEconomia
{
    public int conchas;
    public int tulipanes;
    public int pinyas;
}

[Serializable]
public sealed class DatosSlotInventario
{
    public string itemId;
    public int cantidad;
}

[Serializable]
public sealed class DatosObjetoBase
{
    public string idObjeto;
    public Vector3 posicion;
    public Vector3 rotacionEuler;
    public List<DatosSlotInventario> inventarioCofre = new List<DatosSlotInventario>();
}
