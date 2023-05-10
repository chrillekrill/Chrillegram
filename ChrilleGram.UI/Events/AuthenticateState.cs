using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChrilleGram.UI.Events
{
    public class AuthenticateState
    {
        public event Func<Task> AuthenticationStateChanged;
        private readonly IJSRuntime jsRuntime;
        public async Task SetAuthenticatedAsync()
        {
            await jsRuntime.InvokeAsync<object>("localStorage.setItem", "jwt", "authenticated");

            // Raise the authentication state changed event
            await AuthenticationStateChanged?.Invoke();
        }

        public async Task SetUnauthenticatedAsync()
        {
            await jsRuntime.InvokeAsync<object>("localStorage.removeItem", "jwt");

            // Raise the authentication state changed event
            await AuthenticationStateChanged?.Invoke();
        }
    }
}
