using UnityEngine;
using TMPro;
using NativeWebSocket;
using System;

[Serializable]
public class ResourceData
{
    public int usuario_id;
    public int metal;
    public int cristal;
    
    // Níveis de Tecnologias
    public int mineracao;         
    public int comunicacao;  
    public int naves;
    public int deslocamento;
    
    // Quantidade de Naves na Frota
    public int asteca;
    public int inca;
    public int maia;
    public int persa;
    public int atlantis;

    // Filas de Progresso
    public int ticks_restantes; 
    public string tecnologia_em_construcao;
    public int hangar_ticks_restantes;
    public string nave_em_construcao;
    public int nave_quantidade_construcao;
}

[Serializable]
public class TickPayload
{
    public string eventType; 
    public ResourceData data; 
}

[Serializable]
public class ErrorPayload
{
    public string eventType;
    public string mensagem;
}

// Classe para enviar a encomenda das frotas
[Serializable]
public class EncomendaNaveRequest
{
    public string @event;
    public int usuario_id;
    public string nave;
    public int quantidade;
}

[Serializable]
public class UpgradeRequest
{
    public string @event;
    public int usuario_id;
    public string tecnologia;
}

public class GerenciadorRede : MonoBehaviour
{
    private WebSocket websocket;

    [Header("Contadores de Recursos")]
    public TextMeshProUGUI textoMetal;
    public TextMeshProUGUI textoCristal;

    [Header("Textos das Tecnologias")]
    public TextMeshProUGUI textoMineracao;
    public TextMeshProUGUI textoComunicacao;
    public TextMeshProUGUI textoNaves;
    public TextMeshProUGUI textoDeslocamento;

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

    [Header("Fila de Construção & Alertas")]
    public TextMeshProUGUI textoTempoConstrucao; 
    public TextMeshProUGUI textoTempoHangar; 
    public TextMeshProUGUI textoNotificacao; 

    [Header("Janela da Árvore Tecnológica")]
    public GameObject painelArvoreTecnologica; // <-- RECUPERADO: Arraste o painel para cá no Inspector

    [Header("Configuração do Servidor")]
    public string urlServidor = "ws://localhost:8080"; 

    private string[] nomesMineracao = { "Minas Superficiais", "Laboratorio de Pesquisa", "Minas Profundas", "Reator Nuclear", "Sintetizador de Compostos" };
    private string[] nomesComunicacao = { "Radar", "Multiplex", "Ions", "Phalanx", "Refletor" };
    private string[] nomesNaves = { "Hangar", "Blindagem", "Escudo", "Armas", "Estaleiro" };
    private string[] nomesDeslocamento = { "Combustao", "Impulsao", "Plasma", "Gravitacao", "Hiperespaco" };

    async void Start()
    {
        if (textoNotificacao != null) textoNotificacao.text = "";

        websocket = new WebSocket(urlServidor);

        websocket.OnOpen += () => {
            Debug.Log("📡 Conectado ao servidor de SATELLIS com sucesso!");
        };

        websocket.OnMessage += (bytes) => {
            string jsonRecebido = System.Text.Encoding.UTF8.GetString(bytes);
            jsonRecebido = jsonRecebido.Replace("\"event\":", "\"eventType\":");

            try {
                if (jsonRecebido.Contains("\"erro_notificacao\""))
                {
                    ErrorPayload erro = JsonUtility.FromJson<ErrorPayload>(jsonRecebido);
                    MostrarErroNaTela(erro.mensagem);
                }
                else if (jsonRecebido.Contains("\"tick_update\""))
                {
                    TickPayload payload = JsonUtility.FromJson<TickPayload>(jsonRecebido);

                    if (payload.data != null)
                    {
                        ResourceData dadosJogador = payload.data;

                        // Recursos e Techs
                        textoMetal.text = "Metal: " + dadosJogador.metal;
                        textoCristal.text = "Cristal: " + dadosJogador.cristal;
                        textoMineracao.text = "Mineração: " + ObterNomeTecnologia(nomesMineracao, dadosJogador.mineracao);
                        textoComunicacao.text = "Comunicação: " + ObterNomeTecnologia(nomesComunicacao, dadosJogador.comunicacao);
                        textoNaves.text = "Naves: " + ObterNomeTecnologia(nomesNaves, dadosJogador.naves);
                        textoDeslocamento.text = "Deslocamento: " + ObterNomeTecnologia(nomesDeslocamento, dadosJogador.deslocamento);

                        // Contadores de Frotas Prontas
                        if(textoFrotaAsteca != null) textoFrotaAsteca.text = "Caças Asteca: " + dadosJogador.asteca;
                        if(textoFrotaInca != null) textoFrotaInca.text = "Caças Inca: " + dadosJogador.inca;
                        if(textoFrotaMaia != null) textoFrotaMaia.text = "Fragatas Maia: " + dadosJogador.maia;
                        if(textoFrotaPersa != null) textoFrotaPersa.text = "Fragatas Persa: " + dadosJogador.persa;
                        if(textoFrotaAtlantis != null) textoFrotaAtlantis.text = "Naves-Mãe Atlantis: " + dadosJogador.atlantis;

                        // Fila de Pesquisa de Techs
                        if (dadosJogador.ticks_restantes > 0) {
                            textoTempoConstrucao.text = "[PESQUISA] " + dadosJogador.tecnologia_em_construcao.ToUpper() + " | Faltam " + dadosJogador.ticks_restantes + " Ticks";
                            textoTempoConstrucao.color = Color.yellow;
                        } else {
                            textoTempoConstrucao.text = "[LABORATORIO DISPONIVEL]";
                            textoTempoConstrucao.color = Color.green;
                        }

                        // Fila do Hangar de Naves
                        if (dadosJogador.hangar_ticks_restantes > 0) {
                            textoTempoHangar.text = "[HANGAR OCUPADO] Fabricando " + dadosJogador.nave_quantidade_construcao + "x " + dadosJogador.nave_em_construcao.ToUpper() + " | Faltam " + dadosJogador.hangar_ticks_restantes + " Ticks";
                            textoTempoHangar.color = Color.yellow;
                        } else {
                            textoTempoHangar.text = "[HANGAR PRONTO PARA PRODUÇÃO]";
                            textoTempoHangar.color = Color.green;
                        }
                    }
                }
            }
            catch (Exception erro) {
                Debug.LogError("Erro ao processar dados de rede: " + erro.Message);
            }
        };

        await websocket.Connect();
    }

    private void MostrarErroNaTela(string mensagem)
    {
        if (textoNotificacao != null) {
            textoNotificacao.text = "! ERRO: " + mensagem;
            CancelInvoke("LimparTextoNotificacao"); 
            Invoke("LimparTextoNotificacao", 4f); 
        }
    }

    private void LimparTextoNotificacao() => textoNotificacao.text = "";

    private string ObterNomeTecnologia(string[] listaNomes, int nivel)
    {
        if (nivel == 0) return "Nv 0 (Sem Pesquisa)";
        if (nivel >= 5) return "Nv 5 (" + listaNomes[5 - 1] + ") [MAX]";
        return "Nv " + nivel + " (" + listaNomes[nivel - 1] + ")";
    }

    // --- FUNÇÕES DA JANELA DE ÁRVORE RECUPERADAS ---
    public void AbrirJanelaArvore()
    {
        if (painelArvoreTecnologica != null) painelArvoreTecnologica.SetActive(true);
    }

    public void FecharJanelaArvore()
    {
        if (painelArvoreTecnologica != null) painelArvoreTecnologica.SetActive(false);
    }

        // --- FUNÇÕES DOS BOTÕES DO HANGAR (ATUALIZADO PARA TODAS AS NAVES) ---
    public void BotaoComprarAsteca()   => EnviarPedidoHangar("asteca", inputQtdAsteca);
    public void BotaoComprarInca()     => EnviarPedidoHangar("inca", inputQtdInca);
    public void BotaoComprarMaia()     => EnviarPedidoHangar("maia", inputQtdMaia);
    public void BotaoComprarPersa()    => EnviarPedidoHangar("persa", inputQtdPersa);
    public void BotaoComprarAtlantis() => EnviarPedidoHangar("atlantis", inputQtdAtlantis);

    // Uma função genérica limpa que atende a todos os botões de uma só vez
    private async void EnviarPedidoHangar(string nomeNaveId, TMP_InputField inputCampo)
    {
        if (inputCampo == null || string.IsNullOrEmpty(inputCampo.text)) return;
        int quantidadeDigitada = int.Parse(inputCampo.text);

        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            EncomendaNaveRequest req = new EncomendaNaveRequest();
            req.@event = "encomendar_nave";
            req.usuario_id = 1;
            req.nave = nomeNaveId;
            req.quantidade = quantidadeDigitada;

            string json = JsonUtility.ToJson(req);
            Debug.Log("📤 Solicitando frota: " + quantidadeDigitada + "x " + nomeNaveId);
            await websocket.SendText(json);
            
            // Limpa a caixinha de texto após clicar para ficar elegante
            inputCampo.text = ""; 
        }
    }

    public void ClickMineracao() => SolicitarUpgrade("mineracao");
    public void ClickComunicacao() => SolicitarUpgrade("comunicacao");
    public void ClickNaves() => SolicitarUpgrade("naves");
    public void ClickDeslocamento() => SolicitarUpgrade("deslocamento");

    private async void SolicitarUpgrade(string tipoTecnologia)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            UpgradeRequest requisicao = new UpgradeRequest();
            requisicao.@event = "upgrade_tecnologia";
            requisicao.usuario_id = 1;
            requisicao.tecnologia = tipoTecnologia;
            string jsonEnviar = JsonUtility.ToJson(requisicao);
            await websocket.SendText(jsonEnviar);
        }
    }

    void Update() {
        #if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
        #endif
    }

    private async void OnApplicationQuit() {
        if (websocket != null) await websocket.Close();
    }
}
