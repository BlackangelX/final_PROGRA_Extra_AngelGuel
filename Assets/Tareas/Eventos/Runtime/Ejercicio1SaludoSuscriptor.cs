using UnityEngine;

public class Ejercicio1SaludoSuscriptor : MonoBehaviour
{
    private void OnEnable()
    {
        Ejercicio1SaludoEmisor.EventoSaludo += MostrarSaludo;
    }

    private void OnDisable()
    {
        Ejercicio1SaludoEmisor.EventoSaludo -= MostrarSaludo;
    }

    private void MostrarSaludo()
    {
        PanelMensajesEventos.Publicar("Hola, el evento fue activado.");
    }
}
