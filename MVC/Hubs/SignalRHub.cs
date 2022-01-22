using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace DFPay.MVC.Hubs
{
    public class SignalRHub : Hub
    {
        public async Task SendMessage(int invoiceId, string message)
        {
            await Clients.All.SendAsync("BroadcastMessagePaymentSuccess", invoiceId, message);
        }

        public async Task RefreshTheBell()
        {
            await Clients.All.SendAsync("RefreshTheBell");
        }
    }
}
