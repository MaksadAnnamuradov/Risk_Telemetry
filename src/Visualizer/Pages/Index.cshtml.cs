using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using Risk.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace Visualizer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private readonly IMemoryCache memoryCache;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ColorGenerator colorGenerator, IMemoryCache memoryCache)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            ColorGenerator = colorGenerator;
            this.memoryCache = memoryCache;
        }

        public GameOverRequest gameOverRequest { get; set; }
        public GameStatus Status { get; set; }
        public int MaxRow { get; private set; }
        public int MaxCol { get; private set; }
        public ColorGenerator ColorGenerator { get; }



        public async Task OnGetAsync()
        {
            Status = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<GameStatus>($"{configuration["GameServer"]}/status");
            MaxRow = Status.Board.Max(t => t.Location.Row);
            MaxCol = Status.Board.Max(t => t.Location.Column);

            if (Status.GameState == GameState.GameOver)
            {
                gameOverRequest = await httpClientFactory
               .CreateClient()
               .GetFromJsonAsync<GameOverRequest>($"{configuration["GameServer"]}/GameOverStats");
            }
        }


        public IActionResult OnPostStartGameAsync()
        {
            var client = httpClientFactory.CreateClient();
            Task.Run(()=>
                client.PostAsJsonAsync($"{configuration["GameServer"]}/startgame", new StartGameRequest { SecretCode = configuration["secretCode"] })
            );
            return new RedirectToPageResult("Index");
        }

        public IActionResult OnPostRestartGame()
        {
            var client = httpClientFactory.CreateClient();
            Task.Run(() =>
                client.PostAsJsonAsync($"{configuration["GameServer"]}/restartgame", new StartGameRequest { SecretCode = configuration["secretCode"] })
            );

            return new RedirectToPageResult("Index");
        }
    }
}
