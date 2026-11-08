using UnityEngine;

public class ControladorMenu : MonoBehaviour
{
    [Header("Abas do Jogo")]
    [Tooltip("Ordem sugerida: 0=Visao Geral, 1=Tecnologia, 2=Frota")]
    public GameObject[] paineisDasAbas;

    private void Start()
    {
        // Começa exibindo a aba 0 (Visão Geral) por padrão
        MudarAba(0);
    }

    public void MudarAba(int indiceAlvo)
    {
        if (paineisDasAbas == null || paineisDasAbas.Length == 0) return;

        for (int i = 0; i < paineisDasAbas.Length; i++)
        {
            if (paineisDasAbas[i] != null)
            {
                // Ativa apenas a aba clicada e apaga o resto
                paineisDasAbas[i].SetActive(i == indiceAlvo);
            }
        }
    }
}
