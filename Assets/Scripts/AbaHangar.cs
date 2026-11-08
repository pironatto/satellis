using UnityEngine;
using TMPro;

public class AbaHangar : MonoBehaviour
{
    [Header("Mostruário do Hangar (Frotas)")]
    public TextMeshProUGUI textoFrotaAsteca;
    public TextMeshProUGUI textoFrotaInca;
    public TextMeshProUGUI textoFrotaMaia;
    public TextMeshProUGUI textoFrotaPersa;
    public TextMeshProUGUI textoFrotaAtlantis;

    [Header("Inputs de Fabricação")]
    public TMP_InputField inputQtdAsteca;
    public TMP_InputField inputQtdInca;        
    public TMP_InputField inputQtdMaia;        
    public TMP_InputField inputQtdPersa;       
    public TMP_InputField inputQtdAtlantis;    

    [Header("Cronômetro do Hangar")]
    public TextMeshProUGUI textoTempoHangar;

    private void OnEnable()
    {
        GerenciadorRede.Instancia.OnDadosAtualizados += AtualizarInterface;
        AtualizarInterface();
    }

    private void OnDisable()
    {
        if (GerenciadorRede.Instancia != null)
            GerenciadorRede.Instancia.OnDadosAtualizados -= AtualizarInterface;
    }

    private void AtualizarInterface()
    {
        ResourceData dados = GerenciadorRede.Instancia.dadosGlobais;
        if (dados == null) return;

        if(textoFrotaAsteca != null) textoFrotaAsteca.text = "Caças Asteca: " + dados.asteca;
        if(textoFrotaInca != null) textoFrotaInca.text = "Caças Inca: " + dados.inca;
        if(textoFrotaMaia != null) textoFrotaMaia.text = "Fragatas Maia: " + dados.maia;
        if(textoFrotaPersa != null) textoFrotaPersa.text = "Fragatas Persa: " + dados.persa;
        if(textoFrotaAtlantis != null) textoFrotaAtlantis.text = "Naves-Mãe Atlantis: " + dados.atlantis;

        if (dados.hangar_ticks_restantes > 0) {
            textoTempoHangar.text = "[HANGAR OCUPADO] Fabricando " + dados.nave_quantidade_construcao + "x " + dados.nave_em_construcao.ToUpper() + " | Faltam " + dados.hangar_ticks_restantes + " Ticks";
            textoTempoHangar.color = Color.yellow;
        } else {
            textoTempoHangar.text = "[HANGAR PRONTO PARA PRODUÇÃO]";
            textoTempoHangar.color = Color.green;
        }
    }

    public void BotaoComprarAsteca()   => EnviarPedidoHangar("asteca", inputQtdAsteca);
    public void BotaoComprarInca()     => EnviarPedidoHangar("inca", inputQtdInca);
    public void BotaoComprarMaia()     => EnviarPedidoHangar("maia", inputQtdMaia);
    public void BotaoComprarPersa()    => EnviarPedidoHangar("persa", inputQtdPersa);
    public void BotaoComprarAtlantis() => EnviarPedidoHangar("atlantis", inputQtdAtlantis);

    private void EnviarPedidoHangar(string nomeNaveId, TMP_InputField inputCampo)
    {
        if (inputCampo == null || string.IsNullOrEmpty(inputCampo.text)) return;
        int quantidadeDigitada = int.Parse(inputCampo.text);

        EncomendaNaveRequest req = new EncomendaNaveRequest { @event = "encomendar_nave", usuario_id = 1, nave = nomeNaveId, quantidade = quantidadeDigitada };
        string json = JsonUtility.ToJson(req);
        GerenciadorRede.Instancia.EnviarComando(json);
        inputCampo.text = ""; 
    }
}
