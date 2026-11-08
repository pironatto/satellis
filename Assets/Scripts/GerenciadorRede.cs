using UnityEngine;
using NativeWebSocket;
using System;

[Serializable]
public class ResourceData
{
    public int usuario_id;
    public int metal;
    public int cristal;
    public int mineracao;         
    public int comunicacao;  
    public int naves;
    public int deslocamento;
    public int asteca;
    public int inca;
    public int maia;
    public int persa;
    public int atlantis;
    public int ticks_restantes; 
    public string tecnologia_em_construcao;
    public int hangar_ticks_restantes;
    public string nave_em_construcao;
    public int nave_quantidade_construcao;

    // Atributos dinâmicos da Visão Geral
    public int tick_atual;
    public int posicao_ranking;
    public int pontuacao;
    public int producao_metal_tick;
    public int producao_cristal_tick;
}

[Serializable]
public class TickPayload { public string eventType; public ResourceData data; }

[Serializable]
public class ErrorPayload { public string eventType; public string mensagem; }

[Serializable]
public class EncomendaNaveRequest { public string @event; public int usuario_id; public string nave; public int quantidade; }

[Serializable]
public class UpgradeRequest { public string @event; public int usuario_id; public string tecnologia; }

public class GerenciadorRede : MonoBehaviour
{
    // PADRÃO SINGLETON: Permite que qualquer script acesse este gerenciador
    public static GerenciadorRede Instancia { get; private set; }

    private WebSocket websocket;

    [Header("Contadores de Recursos Globais")]
    public TMPro.TextMeshProUGUI textoMetal;
    public TMPro.TextMeshProUGUI textoCristal;

    [Header("Dados do Servidor (Compartilhados)")]
    public ResourceData dadosGlobais; 

    [Header("Configuração de Alertas")]
    public TMPro.TextMeshProUGUI textoNotificacao;

    [Header("Configuração do Servidor")]
    public string urlServidor = "ws://localhost:8080";

    // Eventos para avisar as abas que dados novos chegaram do servidor
    public event Action OnDadosAtualizados;

    private void Awake()
    {
        if (Instancia == null) { Instancia = this; }
        else { Destroy(gameObject); }
    }

    async void Start()
    {
        if (textoNotificacao != null) textoNotificacao.text = "";

        websocket = new WebSocket(urlServidor);

        websocket.OnOpen += () => { Debug.Log("📡 Conectado ao servidor de SATELLIS com sucesso!"); };

        websocket.OnMessage += (bytes) => {
            string jsonRecebido = System.Text.Encoding.UTF8.GetString(bytes).Replace("\"event\":", "\"eventType\":");

            try {
                if (jsonRecebido.Contains("\"erro_notificacao\"")) {
                    ErrorPayload erro = JsonUtility.FromJson<ErrorPayload>(jsonRecebido);
                    MostrarErroNaTela(erro.mensagem);
                }
                else if (jsonRecebido.Contains("\"tick_update\"")) {
                    TickPayload payload = JsonUtility.FromJson<TickPayload>(jsonRecebido);
                    if (payload.data != null) {
                        dadosGlobais = payload.data; // Salva os dados na memória central

                        // === ATUALIZA OS RECURSOS GLOBAIS NA TELA ===
                        if (textoMetal != null) textoMetal.text = "Metal: " + dadosGlobais.metal;
                        if (textoCristal != null) textoCristal.text = "Cristal: " + dadosGlobais.cristal;
                        // ============================================

                        OnDadosAtualizados?.Invoke(); // Avisa todas as abas para se atualizarem!
                    }
                }
            }
            catch (Exception erro) { Debug.LogError("Erro ao processar dados de rede: " + erro.Message); }
        };

        await websocket.Connect();
    }

    public async void EnviarComando(string json)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(json);
        }
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

    void Update() 
    {
        // Trava de Segurança: Só tenta processar a fila se o objeto do WebSocket já tiver sido criado
        if (websocket != null)
        {
            #if !UNITY_WEBGL || UNITY_EDITOR
                websocket.DispatchMessageQueue();
            #endif
        }
    }

    private async void OnApplicationQuit() { if (websocket != null) await websocket.Close(); }
}
