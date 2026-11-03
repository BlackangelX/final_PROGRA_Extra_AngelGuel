using UnityEngine;

public class Ejercicio3SuscriptorLuz : MonoBehaviour
{
    private void OnEnable()
    {
        Ejercicio3EventoEmisor.EventoInvocado += Reaccionar;
    }

    private void OnDisable()
    {
        Ejercicio3EventoEmisor.EventoInvocado -= Reaccionar;
    }

    private void Reaccionar()
    {
        PanelMensajesEventos.Publicar("Luz reaccionó al evento.");
    }
}
