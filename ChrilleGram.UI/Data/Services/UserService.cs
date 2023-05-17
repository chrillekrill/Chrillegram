using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using Newtonsoft.Json;
using ChrilleGram.UI.Models;
using Microsoft.Extensions.Configuration;

namespace ChrilleGram.UI.Data.Services
{
    public class UserService
    {
        private static readonly string Uri = "https://83.227.19.162:7135";
        private static readonly HttpClient client = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        public async Task<RegisterModel> Register(string email, string username, string password)
        {
            var registerRequest = new
            {
                Email = email,
                Username= username,
                Password = password
            };

            var json = JsonConvert.SerializeObject(registerRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync($"{Uri}/User/CreateUser", content);

            var model = new RegisterModel()
            {
                Content = await res.Content.ReadAsStringAsync(),
                StatusCode = res.StatusCode
            };
            return model;
        }
        public async Task<string> Login(string email, string password)
        {

            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            var json = JsonConvert.SerializeObject(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync($"{Uri}/User/Authenticate", content);
            if(res.IsSuccessStatusCode)
            {
                return await res.Content.ReadAsStringAsync();
            } else
            {
                return null;
            }
        }

        public async Task<bool> CheckStatus()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            try
            {
                var res = await client.GetAsync($"{Uri}/User/CheckStatus", cts.Token);
                return res.IsSuccessStatusCode;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        public async Task<string> RefreshJwt(string jwt)
        {
            var jwtRequest = new
            {
                jwt,
            };

            var json = JsonConvert.SerializeObject(jwtRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync($"{Uri}/User/RefreshToken", content);


            if (res.IsSuccessStatusCode)
            {
                var t = await res.Content.ReadAsStringAsync();

                var token = JsonConvert.DeserializeObject<JwtRequest>(t).Token;

                return token;
            }
            else
            {
                return null;
            }
        }

        public async Task<string> GetUsername(string jwt)
        {
            var jwtRequest = new
            {
                jwt
            };

            var json = JsonConvert.SerializeObject(jwtRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync($"{Uri}/User/GetUserNameFromJwt", content);

            if (res.IsSuccessStatusCode)
            {
                var name = await res.Content.ReadAsStringAsync();

                return name;
            } else
            {
                return null;
            }
        }

        public async Task<string> GetUserId(string jwt)
        {
            var jwtRequest = new
            {
                jwt
            };

            var json = JsonConvert.SerializeObject(jwtRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync($"{Uri}/User/GetUserIdFromJwt", content);

            if (res.IsSuccessStatusCode)
            {
                var id = await res.Content.ReadAsStringAsync();

                return id;
            }
            else
            {
                return null;
            }
        }
    }
}
