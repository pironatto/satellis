using UnityEngine;
using TMPro;

public class AbaTecnologia : MonoBehaviour
{
    [Header("Textos das Tecnologias")]
    public TextMeshProUGUI textoMineracao;
    public TextMeshProUGUI textoComunicacao;
    public TextMeshProUGUI textoNaves;
    public TextMeshProUGUI textoDeslocamento;
    public TextMeshProUGUI textoTempoConstrucao;

    private string[] nomesMineracao = { "Minas Superficiais", "Laboratorio de Pesquisa", "Minas Profundas", "Reator Nuclear", "Sintetizador de Compostos" };
    private string[] nomesComunicacao = { "Radar", "Multiplex", "Ions", "Phalanx", "Refletor" };
    private string[] nomesNaves = { "Hangar", "Blindagem", "Escudo", "Armas", "Estaleiro" };
    private string[] nomesDeslocamento = { "Combustao", "Impulsao", "Plasma", "Gravitacao", "Hiperespaco" };

    private void OnEnable()
    {
        // Quando a aba liga, ela se inscreve para receber atualizações do Cérebro de Rede
        GerenciadorRede.Instancia.OnDadosAtualizados += AtualizarInterface;
        AtualizarInterface(); // Força uma atualização inicial
    }

    private void OnDisable()
    {
        // Importante desinscrever ao desligar a aba para evitar vazamento de memória
        if (GerenciadorRede.Instancia != null)
            GerenciadorRede.Instancia.OnDadosAtualizados -= AtualizarInterface;
    }

    private void AtualizarInterface()
    {
        ResourceData dados = GerenciadorRede.Instancia.dadosGlobais;
        if (dados == null) return;

        textoMineracao.text = "Mineração: " + ObterNomeTecnologia(nomesMineracao, dados.mineracao);
        textoComunicacao.text = "Comunicação: " + ObterNomeTecnologia(nomesComunicacao, dados.comunicacao);
        textoNaves.text = "Naves: " + ObterNomeTecnologia(nomesNaves, dados.naves);
        textoDeslocamento.text = "Deslocamento: " + ObterNomeTecnologia(nomesDeslocamento, dados.deslocamento);

        if (dados.ticks_restantes > 0) {
            textoTempoConstrucao.text = "[PESQUISA] " + dados.tecnologia_em_construcao.ToUpper() + " | Faltam " + dados.ticks_restantes + " Ticks";
            textoTempoConstrucao.color = Color.yellow;
        } else {
            textoTempoConstrucao.text = "[LABORATORIO DISPONIVEL]";
            textoTempoConstrucao.color = Color.green;
        }
    }

    private string ObterNomeTecnologia(string[] listaNomes, int nivel)
    {
        if (nivel == 0) return "Nv 0 (Sem Pesquisa)";
        if (nivel >= 5) return "Nv 5 (" + listaNomes[5 - 1] + ") [MAX]";
        return "Nv " + nivel + " (" + listaNomes[nivel - 1] + ")";
    }

    // Funções disparadas pelos botões de clique
    public void ClickMineracao() => SolicitarUpgrade("mineracao");
    public void ClickComunicacao() => SolicitarUpgrade("comunicacao");
    public void ClickNaves() => SolicitarUpgrade("naves");
    public void ClickDeslocamento() => SolicitarUpgrade("deslocamento");

    private void SolicitarUpgrade(string tipoTecnologia)
{
    UpgradeRequest requisicao = new UpgradeRequest();
    requisicao.@event = "upgrade_tecnologia";
    requisicao.usuario_id = 1;
    requisicao.tecnologia = tipoTecnologia; // <-- CORRIGIDO!

    string json = JsonUtility.ToJson(requisicao);
    GerenciadorRede.Instancia.EnviarComando(json);
}


}
