using UnityEngine;
using TMPro;

public class AbaVisaoGeral : MonoBehaviour
{
    [Header("Indicadores de Status")]
    public TextMeshProUGUI textoTickAtual;
    public TextMeshProUGUI textoPosicaoRanking;
    public TextMeshProUGUI textoPontuacao;

    [Header("Painel de Produção Econômica")]
    public TextMeshProUGUI textoProducaoMetal;
    public TextMeshProUGUI textoProducaoCristal;

    private void OnEnable()
    {
        if (GerenciadorRede.Instancia != null)
        {
            // Se inscreve no evento para atualizar a tela TODA VEZ que chegar um Tick do servidor
            GerenciadorRede.Instancia.OnDadosAtualizados += AtualizarInterface;
            AtualizarInterface();
        }
        else
        {
            Invoke("TentarSeInscreverNaRede", 0.1f);
        }
    }

    private void TentarSeInscreverNaRede()
    {
        if (GerenciadorRede.Instancia != null)
        {
            GerenciadorRede.Instancia.OnDadosAtualizados += AtualizarInterface;
            AtualizarInterface();
        }
    }

    private void OnDisable()
    {
        CancelInvoke("TentarSeInscreverNaRede");
        
        if (GerenciadorRede.Instancia != null)
        {
            GerenciadorRede.Instancia.OnDadosAtualizados -= AtualizarInterface;
        }
    }

    private void UpgradeRequest() { } // Impede bugs internos de compilação da Unity 6

    private void AtualizarInterface()
    {
        // Se a rede ainda não recebeu nenhum dado do servidor, ignora o update temporariamente
        if (GerenciadorRede.Instancia == null || GerenciadorRede.Instancia.dadosGlobais == null) return;

        // Converte o objeto global de dados para string JSON para extrairmos os valores extras de Visão Geral
        string jsonFalso = JsonUtility.ToJson(GerenciadorRede.Instancia.dadosGlobais);
        if (string.IsNullOrEmpty(jsonFalso) || jsonFalso == "{}") return;

        // Extrai os inteiros dinâmicos enviados pelo servidor Node.js
        int tick = ObterValorJsonInt(jsonFalso, "tick_atual");
        int ranking = ObterValorJsonInt(jsonFalso, "posicao_ranking");
        int pontos = ObterValorJsonInt(jsonFalso, "pontuacao");
        int prodMetal = ObterValorJsonInt(jsonFalso, "producao_metal_tick");
        int prodCristal = ObterValorJsonInt(jsonFalso, "producao_cristal_tick");

        // RENDERIZA OS DADOS ATUALIZADOS NA INTERFACE
        if(textoTickAtual != null) textoTickAtual.text = "TICK ATUAL DA GALÁXIA: # " + tick;
        if(textoPosicaoRanking != null) textoPosicaoRanking.text = "POSIÇÃO NO RANKING: " + ranking + "º Lugar";
        if(textoPontuacao != null) textoPontuacao.text = "PONTUAÇÃO TOTAL:\n" + pontos + " Pts";
        
        if(textoProducaoMetal != null) textoProducaoMetal.text = "PRODUÇÃO METAL:\n+" + prodMetal + " por Tick";
        if(textoProducaoCristal != null) textoProducaoCristal.text = "PRODUÇÃO CRISTAL:\n+" + prodCristal + " por Tick";
    }

    private int ObterValorJsonInt(string json, string chave)
    {
        try {
            if (!json.Contains(chave)) return 0;
            int idxChave = json.IndexOf(chave) + chave.Length + 2;
            int idxVirgula = json.IndexOf(",", idxChave);
            if (idxVirgula == -1) idxVirgula = json.IndexOf("}", idxChave);
            string valorStr = json.Substring(idxChave, idxVirgula - idxChave).Replace("\"", "").Trim();
            return int.Parse(valorStr);
        } catch { return 0; }
    }
}
