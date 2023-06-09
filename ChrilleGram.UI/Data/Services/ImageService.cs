﻿using ChrilleGram.UI.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChrilleGram.UI.Data.Services
{
    public class ImageService
    {
        private static readonly string Uri = "https://83.227.19.162:7135";
        private static readonly HttpClient client = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            MaxRequestContentBufferSize = 5000000
        });

        public async Task<ICollection<ImagePath>> GetAllImagePaths(string jwt)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var res = await client.GetAsync($"{Uri}/Image/GetAll?jwt=" + jwt);

            var model = JsonConvert.DeserializeObject<List<ImagePath>>(await res.Content.ReadAsStringAsync());

            return model;
        }

        public async Task<byte[]> GetImage(string path, string jwt)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var res = await client.GetAsync($"{Uri}/Image/GetImage?imagepath=" + path);
 
            var img = await res.Content.ReadAsByteArrayAsync();

             return img;
        }

        public async Task<bool> UploadImage(IBrowserFile file, string jwt)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream(5000000));
            content.Add(streamContent, "file", file.Name);

            var res = await client.PostAsync($"{Uri}/Image/UploadFile?jwt={jwt}", content);

            return res.IsSuccessStatusCode;
        }
    }
}
