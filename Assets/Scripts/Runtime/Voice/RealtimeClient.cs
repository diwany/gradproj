using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Museum.Voice
{
    /// <summary>
    /// Thin WebSocket client for the OpenAI Realtime API. Handles transport + JSON framing only;
    /// session config, prompts, and audio routing live in higher-level components.
    /// </summary>
    public class RealtimeClient : IDisposable
    {
        // Default to the cheap mini variant. Override on GuideOrchestrator if you want gpt-realtime / gpt-4o-realtime-preview.
        public const string DefaultUrl = "wss://api.openai.com/v1/realtime?model=gpt-4o-mini-realtime-preview";

        ClientWebSocket _socket;
        CancellationTokenSource _cts;
        Task _receiveLoop;
        readonly ConcurrentQueue<string> _outgoing = new ConcurrentQueue<string>();
        readonly object _sendLock = new object();
        Task _sendLoop;

        public bool IsOpen => _socket != null && _socket.State == WebSocketState.Open;

        /// <summary>Raised for every incoming JSON event. Fires on a worker thread — marshal to main thread before touching Unity APIs.</summary>
        public event Action<JObject> OnEvent;

        /// <summary>Raised on connection open. Worker thread.</summary>
        public event Action OnOpen;

        /// <summary>Raised on graceful close or transport failure. Worker thread.</summary>
        public event Action<string> OnClose;

        public async Task ConnectAsync(string apiKey, string url = DefaultUrl, CancellationToken externalCt = default)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("OpenAI API key is empty.", nameof(apiKey));

            _socket = new ClientWebSocket();
            _socket.Options.SetRequestHeader("Authorization", "Bearer " + apiKey);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);

            try
            {
                await _socket.ConnectAsync(new Uri(url), _cts.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                OnClose?.Invoke($"Connect failed: {e.Message}");
                Cleanup();
                throw;
            }

            OnOpen?.Invoke();
            _receiveLoop = Task.Run(ReceiveLoop, _cts.Token);
            _sendLoop = Task.Run(SendLoop, _cts.Token);
        }

        public void Send(JObject evt)
        {
            if (evt == null) return;
            _outgoing.Enqueue(evt.ToString(Formatting.None));
        }

        public void Send(object evt)
        {
            if (evt == null) return;
            _outgoing.Enqueue(JsonConvert.SerializeObject(evt));
        }

        public void Send(string rawJson)
        {
            if (string.IsNullOrEmpty(rawJson)) return;
            _outgoing.Enqueue(rawJson);
        }

        async Task SendLoop()
        {
            try
            {
                while (!_cts.IsCancellationRequested && IsOpen)
                {
                    if (_outgoing.TryDequeue(out var json))
                    {
                        var bytes = Encoding.UTF8.GetBytes(json);
                        var seg = new ArraySegment<byte>(bytes);
                        await _socket.SendAsync(seg, WebSocketMessageType.Text, true, _cts.Token).ConfigureAwait(false);
                    }
                    else
                    {
                        await Task.Delay(5, _cts.Token).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                OnClose?.Invoke($"Send loop error: {e.Message}");
            }
        }

        async Task ReceiveLoop()
        {
            var buffer = new byte[64 * 1024];
            var sb = new StringBuilder();
            try
            {
                while (!_cts.IsCancellationRequested && IsOpen)
                {
                    sb.Clear();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token).ConfigureAwait(false);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            OnClose?.Invoke($"Server closed: {result.CloseStatus} {result.CloseStatusDescription}");
                            return;
                        }
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    } while (!result.EndOfMessage);

                    var raw = sb.ToString();
                    if (string.IsNullOrEmpty(raw)) continue;

                    JObject parsed;
                    try { parsed = JObject.Parse(raw); }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[RealtimeClient] Failed to parse incoming JSON ({e.Message}): {raw.Substring(0, Math.Min(200, raw.Length))}...");
                        continue;
                    }
                    OnEvent?.Invoke(parsed);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                OnClose?.Invoke($"Receive loop error: {e.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_socket == null) return;
            try
            {
                if (_socket.State == WebSocketState.Open)
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "client closing", CancellationToken.None).ConfigureAwait(false);
            }
            catch { }
            Cleanup();
        }

        void Cleanup()
        {
            try { _cts?.Cancel(); } catch { }
            try { _socket?.Dispose(); } catch { }
            _socket = null;
            _cts = null;
        }

        public void Dispose() => Cleanup();
    }
}
