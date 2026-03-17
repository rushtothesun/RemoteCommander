using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCommander
{
    public static class HttpCommandSender
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

        public static bool Send(string address, string command)
        {
            try
            {
                var url = "http://" + address + "/command";
                var payload = "{\"command\":\"" + command + "\"}";
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                // Execute synchronously (WPF fire & forget)
                var response = _httpClient.PostAsync(url, content).GetAwaiter().GetResult();

                Trace.WriteLine($"[RemoteCommander] Sent '{command}' to {address} => {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[RemoteCommander] Error sending to {address}: {ex.Message}");
                return false;
            }
        }
    }
}
