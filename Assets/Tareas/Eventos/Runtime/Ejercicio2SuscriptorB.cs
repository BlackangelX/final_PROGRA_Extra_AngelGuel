using UnityEngine;

public class Ejercicio2SuscriptorB : MonoBehaviour
{
    private bool estaSuscrito;

    private void OnEnable()
    {
        Ejercicio2EventoEmisor.EventoDinamico += Reaccionar;
        estaSuscrito = true;
    }

    private void OnDisable()
    {
        Desuscribirse();
    }

    private void Reaccionar()
    {
        PanelMensajesEventos.Publicar("Suscriptor B reaccionó.");
        Desuscribirse();
        PanelMensajesEventos.Publicar("Suscriptor B se desuscribió usando -=.");
    }

    private void Desuscribirse()
    {
        if (!estaSuscrito)
        {
            return;
        }

        Ejercicio2EventoEmisor.EventoDinamico -= Reaccionar;
        estaSuscrito = false;
    }
}
