using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SpotifyBot.Api.Model;
using SpotifyBot.Persistence;
using SpotifyBot.Shared;

namespace SpotifyBot.Api
{
    [ApiController]
    [Route("/Process")]
    [EnableCors("AllowAll")]
    public sealed class ProcessController : ControllerBase
    {
        [Route("place-order")]
        [HttpPost]
        public async Task<JsonResult> PlaceOrder(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider,
             OrderModel orderRequest)
        {
            var spotifyService = spotifyServiceGroup.GetMainService();

            FilTrackIds(orderRequest);

            var orderResult = await spotifyService.PlaceOrder(storageUowProvider, orderRequest);

            return new JsonResult(new OrderResponse() { NewOrder = orderResult, Status = "Order placed" });

        }

        [Route("get-orders")]
        public async Task<IActionResult> GetOrders(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider)
        {
            try
            {
                var spotifyService = spotifyServiceGroup.GetService(0);
                var orders = await spotifyService.GetOrders(storageUowProvider);
                return new JsonResult(new OrderResponse() { Orders = orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Route("order-detail/{orderId}")]
        public async Task<IActionResult> GetOrderDetails(
      [FromServices] SpotifyServiceGroup spotifyServiceGroup,
      [FromServices] StorageUowProvider storageUowProvider, int orderId)
        {
            try
            {
                var spotifyService = spotifyServiceGroup.GetService(0);
                var order = await spotifyService.GetOrderDetail(storageUowProvider, orderId);
                return new JsonResult(new OrderDetailResponse() { Order = order });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Route("order-job-detail/{orderId}")]
        public async Task<IActionResult> GetOrderJobDetails(
        [FromServices] SpotifyServiceGroup spotifyServiceGroup,
        [FromServices] StorageUowProvider storageUowProvider, int orderId)
        {
            try
            {
                var spotifyService = spotifyServiceGroup.GetService(0);
                var jobs = await spotifyService.GetOrderJobDetail(storageUowProvider, orderId);
                return new JsonResult(new JobDetailResponse() { Jobs = jobs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Route("order-account-tracks/{orderId}")]
        public async Task<IActionResult> GetOrderAccountTracks(
        [FromServices] SpotifyServiceGroup spotifyServiceGroup,
        [FromServices] StorageUowProvider storageUowProvider, int orderId)
        {
            try
            {
                var spotifyService = spotifyServiceGroup.GetService(0);
                var accountTracks = await spotifyService.GetOrderAccountTracks(storageUowProvider, orderId);
                return new JsonResult(new AccountTrackResponse() { AccountTracks = accountTracks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Route("account-tracks")]
        public async Task<IActionResult> GetAccountTracks(
         [FromServices] SpotifyServiceGroup spotifyServiceGroup,
         [FromServices] StorageUowProvider storageUowProvider)
        {
            try
            {
                var spotifyService = spotifyServiceGroup.GetService(0);
                var accountTracks = await spotifyService.GetAccountTracks(storageUowProvider);
                return new JsonResult(new AccountTrackResponse() { AccountTracks = accountTracks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Route("process-order")]
        [HttpPost]
        public async Task<IActionResult> ProcessOrder(
        [FromServices] SpotifyServiceGroup spotifyServiceGroup,
        [FromServices] StorageUowProvider storageUowProvider)
        {
            try
            {
                var spotifyService = spotifyServiceGroup.GetMainService();
                var orderResult = await spotifyService.ProcessOrder(storageUowProvider);
                return new JsonResult(new OrderResponse() { Orders = orderResult });
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }

        }

        private void FilTrackIds(OrderModel orderRequest)
        {
            var trackIds = orderRequest.TrackIds.Split(",");

            foreach (var trackId in trackIds)
            {
                orderRequest.Tracks.Add(new TrackModel
                {
                    TrackId = trackId,
                    TrackTitle = orderRequest.TrackTitle
                });
            }
        }

        [Route("dashboard")]
        public async Task<GetDashboardResponse> GetDashboardData(
        [FromServices] SpotifyServiceGroup spotifyServiceGroup,
        [FromServices] StorageUowProvider storageUowProvider)
        {
            var spotifyService = spotifyServiceGroup.GetService(0);
            var dashboard = await spotifyService.GetDashboard(storageUowProvider);
            return new GetDashboardResponse() { dash = dashboard }; 
        }

        [Route("get-charts")]
        [HttpPost]
        public async Task<ChartModel[]> GetCharts(
            [FromServices] SpotifyServiceGroup spotifyServiceGroup,
            [FromServices] StorageUowProvider storageUowProvider)
        {
            var spotifyService = spotifyServiceGroup.GetService(30000);
            var retVal = await spotifyService.GetCharts(storageUowProvider, 30000);
            return retVal;
        }
    }
}
