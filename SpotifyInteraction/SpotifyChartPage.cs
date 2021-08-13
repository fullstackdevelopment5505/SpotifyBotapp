using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PuppeteerSharp;
using SpotifyBot.Api.Model;
using SpotifyBot.Persistence;

namespace SpotifyBot.SpotifyInteraction
{
    public static class SpotifyChartPage
    {
        public static async Task<string> OpenChartPage(Page page)
        {
            String logStatus = "Success";
            try
            {
                var response = await page.GoToAsync("https://spotifycharts.com/regional", timeout: 0);
                
            }
            catch (Exception)
            {
                logStatus = "ProxyAddressFail";
            }
            return logStatus;
            //await page.setDefaultNavigationTimeout(0);
        }

        public static async Task<List<ChartModel>> ClickPage(Page page, Track[] trackList)
        {
            List<ChartModel> chartModelList = new List<ChartModel>();
            int flag = 0;
            try
            {
                //await page.WaitForSelectorAsync("table.chart-table > tbody");
                var dailyVal = await page.QuerySelectorAllAsync("div.chart-filters-list > div:nth-child(2) > div");
                var dailyStr = await dailyVal[0].EvaluateFunctionAsync<string>("e => e.innerHTML");
                var trRow = await page.QuerySelectorAllAsync("table.chart-table > tbody > tr");
                var tdRank = await page.QuerySelectorAllAsync("td.chart-table-position");
                var tdTrackAuthor = await page.QuerySelectorAllAsync("td.chart-table-track > span");
                var tdStream = await page.QuerySelectorAllAsync("td.chart-table-streams");
                var tdTrackTitle = await page.QuerySelectorAllAsync("td.chart-table-track > strong");
                var chartModel = new ChartModel();
                for (int i = 0; i < trRow.Length; i++)
                {

                    var trackTitle = await tdTrackTitle[i].EvaluateFunctionAsync<string>("e => e.innerHTML");
                    for (int j = 0; j< trackList.Length; j++)
                    {
                        if(trackList[j].Title == trackTitle)
                        {
                            flag = 1;break;
                        }
                    }
                    if(flag == 1)
                    {                        
                        chartModel.rank = await tdRank[i].EvaluateFunctionAsync<int>("e => e.innerHTML");
                        chartModel.trackTitle = trackTitle;
                        chartModel.trackAuthor = await tdTrackAuthor[i].EvaluateFunctionAsync<string>("e => e.innerHTML");
                        chartModel.trackAuthor = chartModel.trackAuthor.Substring(2);                        
                        chartModel.stream = await tdStream[i].EvaluateFunctionAsync<string>("e => e.innerHTML");
                        chartModel.country = "";
                        chartModel.daily = dailyStr;
                        chartModelList.Add(chartModel);
                        chartModel = new ChartModel();
                    }
                    flag = 0;
                }
                return chartModelList;
            } catch(Exception e)
            {
                return chartModelList;
            }           
            
        }
    }
}
