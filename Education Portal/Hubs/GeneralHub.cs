using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Education_Portal.Models;

namespace Education_Portal.Hubs
{
    public class GeneralHub : Hub
    {
        private readonly UserManager<AppUser> _userManager;

        public GeneralHub(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task GetTotalUserCount()
        {
            var count = _userManager.Users.Count();
            await Clients.All.SendAsync("ReceiveUserCount", count);
        }
    }
}