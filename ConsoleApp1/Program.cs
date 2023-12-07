// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;

const string token =
    "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjpbIkFkbWluIiwiT3BlcmF0b3IiLCJVc2VyIl0sImp0aSI6IjA1ZjM1YjI4LTA3NGEtNDRjZS1iNTBjLWY5OWQ1YWE3YTU2MCIsIm5hbWUiOiJXZW9sIiwic3ViIjoiMzc2MTI5OTI1NzgwMDc4NTkyIiwicmVmcmVzaF90b2tlbiI6ImV0bkxMSGExb0lzSTZlUDkzV0lTNWFSTzZSa3VWRyIsInRlbmFudF9pZCI6IjY1NDAxNjM3MTI2MDI2MDQxMiIsImF2YXRhciI6ImVhMTNlMDdjN2U5YzQ0YzIyMDNiNTIzMTY0MGM4MmM2IiwibmJmIjoxNzAxOTAyMTU2LCJleHAiOjIwNjE5MDIxNTcsImlhdCI6MTcwMTkwMjE1NywiaXNzIjoiNjU0MDE2MzcxMjYwMjYwNDEyIiwiYXVkIjoiMzc2MTI5OTI1NzgwMDc4NTkyIn0.g0b_C0nCKcj7CZ98aQdRtVpZyONAQg-4uaARkrOCD5w86rqMdyqJGQChCXHWydeI9TfBEKe6W_rtxAJqIynmf8QBiYcpmll9-2OgmwutTzey4l9sGGPcETMA3rx-9yXuKs5q-AmmoqBT1HGD8f7U2Wrq3GUIWPSFc8mjEWm1ftdaJ329bZB5xR6bTD6S96iLQGDgIVTSy_ourdKt6voTUS0smEFvaODTlGES1eIQZk2xGUuFy5UM0yRCfdQcj3BvBGrvfNB92Z8k8le64p3zrhtpYLZbbOHaPaiXqOz9Ya05cmsok8XSkSYPXOGY1uozYejjX2fHH9Ml33r6iqk9wQ";

var delete = false;
while (true)
{
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    if (!delete)
    {
        await httpClient.PostAsync(
        "http://localhost:3000/lan/2487e1a3-6f68-4f36-8187-94ddb5539cc6/seat/21a94ee3-c401-47e7-b08d-f17b7b5596b3/reservation", null);
        delete = true;
    }
    else
    {
        await httpClient.DeleteAsync(
        "http://localhost:3000/lan/2487e1a3-6f68-4f36-8187-94ddb5539cc6/seat/21a94ee3-c401-47e7-b08d-f17b7b5596b3/reservation");
        delete = false;
    }

    await httpClient.GetAsync("http://localhost:3000/lan/2487e1a3-6f68-4f36-8187-94ddb5539cc6/seat");
    Thread.Sleep(100);
}