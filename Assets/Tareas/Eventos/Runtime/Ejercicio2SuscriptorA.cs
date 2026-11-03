using UnityEngine;

public class Ejercicio2SuscriptorA : MonoBehaviour
{
    private void OnEnable()
    {
        Ejercicio2EventoEmisor.EventoDinamico += Reaccionar;
    }

    private void OnDisable()
    {
        Ejercicio2EventoEmisor.EventoDinamico -= Reaccionar;
    }

    private void Reaccionar()
    {
        PanelMensajesEventos.Publicar("Suscriptor A reaccionó.");
    }
}
