using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SpotifyBot.Api.Model;
using SpotifyBot.Persistence;

namespace SpotifyBot.Api
{
    
    [ApiController]
    [Route("/accounts")]
    public sealed class CommonController : ControllerBase
    {
        [Route("get-brief-info")]
        public GetBriefInfoResponse GetBriefInfo([FromServices] SpotifyServiceGroup spotifyServiceGroup)
        {
            var briefInfo = spotifyServiceGroup.GetBriefInfo();
            return new GetBriefInfoResponse(){Accounts = briefInfo};
        }
    }
}