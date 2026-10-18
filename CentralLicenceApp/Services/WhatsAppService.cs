using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace CentralLicenceApp.Services
{
    /// <summary>
    /// WaSender REST client (https://wasenderapi.com). All sends in the process are serialised
    /// through one gate and kept MinGapMs apart, because WaSender allows ~1 message per 5 s.
    /// </summary>
    public class WhatsAppService : IWhatsAppService
    {
        public const string HttpClientName = "WaSender";
        private const int MaxAttempts = 10;

        private static readonly SemaphoreSlim Gate = new(1, 1);
        private static DateTime _lastSendUtc = DateTime.MinValue;

        private readonly IWhatsAppRepository _repo;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IDataProtector _protector;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(IWhatsAppRepository repo, IHttpClientFactory httpFactory,
            IDataProtectionProvider dataProtection, ILogger<WhatsAppService> logger)
        {
            _repo = repo;
            _httpFactory = httpFactory;
            _protector = dataProtection.CreateProtector("CentralLicenceApp.WhatsApp.ApiKey");
            _logger = logger;
        }

        public string ProtectApiKey(string plainKey) => _protector.Protect(plainKey);

        public async Task<WhatsAppSendResult> SendTextMessageAsync(string toPhone, string text, string moduleCode,
            int? moduleRefId = null, string? documentUrl = null, string? fileName = null,
            string? sentBy = null, bool ignoreMasterSwitch = false, string? overrideApiKey = null,
            CancellationToken ct = default)
        {
            var result = new WhatsAppSendResult();
            WhatsAppConfiguration? cfg;
            string? token;
            try
            {
                cfg = await _repo.GetConfigAsync();
                if (!string.IsNullOrWhiteSpace(overrideApiKey))
                {
                    // Test with a token typed on the screen but not saved yet.
                    cfg ??= new WhatsAppConfiguration();
                    token = StripBearer(overrideApiKey);
                }
                else
                {
                    token = cfg == null ? null : Unprotect(cfg.ApiKeyEncrypted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WhatsApp] Could not load configuration");
                result.ErrorMessage = "Could not load WhatsApp configuration: " + ex.Message;
                return result;
            }

            if (cfg == null || string.IsNullOrWhiteSpace(token))
            {
                result.ErrorMessage = "No WhatsApp configuration found or API token is missing.";
                return result;
            }
            if (!cfg.IsEnabled && !ignoreMasterSwitch)
            {
                result.ErrorMessage = "WhatsApp service is disabled.";
                return result;
            }

            var phone = FormatPhoneNumber(toPhone, cfg.DefaultCountryCode);
            result.RecipientPhone = phone;

            var body = documentUrl == null
                ? JsonSerializer.Serialize(new { to = phone, text })
                : JsonSerializer.Serialize(new { to = phone, text, documentUrl, fileName });

            if (!IsValidE164(phone))
            {
                result.ErrorMessage = $"Invalid phone number '{toPhone}'. Enter 10 digits or the full international number.";
            }
            else
            {
                await Gate.WaitAsync(ct);
                try
                {
                    for (var attempt = 1; attempt <= MaxAttempts; attempt++)
                    {
                        var wait = cfg.MinGapMs - (DateTime.UtcNow - _lastSendUtc).TotalMilliseconds;
                        if (wait > 0) await Task.Delay((int)wait, ct);

                        int? retryAfter;
                        try
                        {
                            retryAfter = await PostOnceAsync(cfg, token!, body, result, ct);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
                        {
                            _logger.LogWarning(ex, "[WhatsApp] Send to {Phone} failed", phone);
                            result.ErrorMessage = ex.Message;
                            break;
                        }
                        finally
                        {
                            _lastSendUtc = DateTime.UtcNow;
                        }

                        if (result.Success || retryAfter == null) break;
                        if (attempt == MaxAttempts)
                        {
                            result.ErrorMessage = $"Rate limit persisted after {MaxAttempts} retries.";
                            break;
                        }
                        var delay = Math.Max(retryAfter.Value, 5) + 1;
                        _logger.LogInformation("[WhatsApp] Rate limited, retry {Attempt} in {Delay}s", attempt, delay);
                        await Task.Delay(TimeSpan.FromSeconds(delay), ct);
                    }
                }
                finally
                {
                    Gate.Release();
                }
            }

            try
            {
                await _repo.InsertLogAsync(new WhatsAppLog
                {
                    ConfigId = cfg.Id == 0 ? null : cfg.Id,
                    ModuleCode = moduleCode,
                    ModuleRefId = moduleRefId,
                    RecipientPhone = phone,
                    MessageText = text,
                    IsSuccess = result.Success,
                    MsgId = result.MsgId,
                    Jid = result.Jid,
                    Status = result.Success ? result.Status : "failed",
                    ApiResponse = result.RawResponse,
                    ErrorMessage = Truncate(result.ErrorMessage, 1000),
                    SentBy = sentBy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[WhatsApp] Could not write WhatsAppLog row");
            }

            return result;
        }

        /// <summary>Posts once. Returns retry_after seconds when WaSender rate-limited the call, else null.</summary>
        private async Task<int?> PostOnceAsync(WhatsAppConfiguration cfg, string token, string body,
            WhatsAppSendResult result, CancellationToken ct)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, cfg.ApiUrl)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            AddAuth(req, cfg, token);

            var client = _httpFactory.CreateClient(HttpClientName);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(25));
            using var res = await client.SendAsync(req, cts.Token);
            result.RawResponse = await res.Content.ReadAsStringAsync(cts.Token);

            JsonElement root;
            try
            {
                root = JsonDocument.Parse(result.RawResponse).RootElement;
            }
            catch (JsonException)
            {
                result.ErrorMessage = $"HTTP {(int)res.StatusCode}: unexpected response from WhatsApp API.";
                return null;
            }

            if (root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("success", out var ok) && ok.ValueKind == JsonValueKind.True)
            {
                result.Success = true;
                result.ErrorMessage = null;
                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                {
                    result.MsgId = data.TryGetProperty("msgId", out var m) ? m.ToString() : null;
                    result.Jid = data.TryGetProperty("jid", out var j) ? j.ToString() : null;
                    result.Status = data.TryGetProperty("status", out var s) ? s.ToString() : null;
                }
                result.Status ??= "sent";
                return null;
            }

            result.ErrorMessage = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("message", out var msg)
                ? msg.ToString()
                : $"HTTP {(int)res.StatusCode}";

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("retry_after", out var ra)
                && ra.TryGetInt32(out var seconds))
                return seconds;
            return null;
        }

        public async Task<string?> UploadMediaAsync(byte[] content, string mimeType, CancellationToken ct = default)
        {
            try
            {
                var cfg = await _repo.GetConfigAsync();
                var token = cfg == null ? null : Unprotect(cfg.ApiKeyEncrypted);
                if (cfg == null || string.IsNullOrWhiteSpace(token)) return null;

                var idx = cfg.ApiUrl.IndexOf("/api/", StringComparison.OrdinalIgnoreCase);
                var uploadUrl = (idx >= 0 ? cfg.ApiUrl[..idx] : cfg.ApiUrl.TrimEnd('/')) + "/api/upload";
                var body = JsonSerializer.Serialize(new { mimetype = mimeType, base64 = Convert.ToBase64String(content) });

                using var req = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                AddAuth(req, cfg, token!);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(35));
                using var res = await _httpFactory.CreateClient(HttpClientName).SendAsync(req, cts.Token);
                var raw = await res.Content.ReadAsStringAsync(cts.Token);
                var root = JsonDocument.Parse(raw).RootElement;
                if (root.TryGetProperty("success", out var ok) && ok.ValueKind == JsonValueKind.True
                    && root.TryGetProperty("publicUrl", out var url))
                    return url.GetString();

                _logger.LogWarning("[WhatsApp] Media upload failed: {Response}", raw);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[WhatsApp] Media upload failed");
                return null;
            }
        }

        /// <summary>
        /// Keeps digits and a leading '+'. '+' → as is (unless it is '+' plus only 10 digits, which is
        /// treated as a missing country code); 10 digits → country code + digits; a leading 0 on
        /// 11 digits is stripped first; anything else → '+' + digits.
        /// </summary>
        public string FormatPhoneNumber(string raw, string? defaultCountryCode)
        {
            var cc = "+" + new string((defaultCountryCode ?? "+91").Where(char.IsDigit).ToArray());
            if (cc == "+") cc = "+91";

            var trimmed = (raw ?? string.Empty).Trim();
            var digits = new string(trimmed.Where(char.IsDigit).ToArray());
            if (digits.Length == 0) return string.Empty;

            if (trimmed.StartsWith("+"))
                return digits.Length == 10 ? cc + digits : "+" + digits;

            if (digits.Length == 11 && digits[0] == '0') digits = digits[1..];
            if (digits.Length == 10) return cc + digits;
            return "+" + digits;
        }

        private static bool IsValidE164(string phone)
        {
            var len = phone.Length - 1;
            return phone.StartsWith("+") && len >= 8 && len <= 15;
        }

        private static void AddAuth(HttpRequestMessage req, WhatsAppConfiguration cfg, string token)
        {
            var header = string.IsNullOrWhiteSpace(cfg.AuthHeaderKey) ? "Authorization" : cfg.AuthHeaderKey.Trim();
            if (header.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            else
                req.Headers.TryAddWithoutValidation(header, "Bearer " + token);
        }

        private string? Unprotect(string? cipher)
        {
            if (string.IsNullOrEmpty(cipher)) return null;
            try
            {
                return StripBearer(_protector.Unprotect(cipher));
            }
            catch (Exception ex)
            {
                // Happens when the Data Protection key ring changed; the token must be re-entered.
                _logger.LogError(ex, "[WhatsApp] Stored API token could not be decrypted");
                return null;
            }
        }

        private static string StripBearer(string token)
        {
            token = token.Trim();
            return token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? token[7..].Trim() : token;
        }

        private static string? Truncate(string? s, int max) => s == null || s.Length <= max ? s : s[..max];
    }
}
