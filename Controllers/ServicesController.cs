using Microsoft.AspNetCore.Mvc;
using SmartWinners.Helpers;
using SmartWinners.Models;
using System;
using System.IO;
using System.Linq;

namespace SmartWinners.Controllers;

[Route("Helper")]
public class ServicesController : Controller
{
    [HttpDelete("Delete")]
    public IActionResult Delete([FromQuery(Name = "n")] string name)
    {
        WebStorageUtility.RemoveValue(name);
        return Ok();
    }

    [HttpPost("Set")]
    public IActionResult SetValue([FromBody] PostObject obj)
    {
        if (obj.Encrypt)
        {
            obj.Value = CryptoUtility.EncryptString(obj.Value);
        }

        if (obj.Compress)
        {
            obj.Value = Convert.ToBase64String(CompressUtility.CompressString(obj.Value));
        }

        WebStorageUtility.SetString(obj.Name, obj.Value);


        return Ok();
    }

    [HttpGet("Update/LangIso")]
    public IActionResult UpdateLangIso([FromQuery(Name = "l")] string langIso)
    {
        WebStorageUtility.SetString(WebStorageUtility.LangIso, langIso);

        if (WebStorageUtility.GetSignedUser() != null)
        {
            IdentityHelper.UpdateUserLangIsoAsync(langIso).Wait();
        }

        return Ok();
    }

    [HttpGet("TimeZone")]
    public IActionResult SetTimeZone([FromQuery(Name = "t")] int offset)
    {
        var userOffset = TimeSpan.FromMinutes(offset);
        var timeZone = TimeZoneInfo.GetSystemTimeZones().First(x => x.BaseUtcOffset.Hours.Equals(userOffset.Hours));

        WebStorageUtility.SetString(WebStorageUtility.TimeZone, timeZone.Id, WebStorageUtility.LifetimeCookieDate);
        WebStorageUtility.SetString(WebStorageUtility.TimeZoneOffset, $"{offset}",
            WebStorageUtility.LifetimeCookieDate);
        return Ok();
    }

    [HttpPost("Get")]
    public IActionResult GetValue([FromBody] PostObject obj)
    {
        if ((obj is not null && obj.Equals(WebStorageUtility.UserValueName)) || obj is null)
            return BadRequest();

        WebStorageUtility.TryGetString(obj.Name, out var result);

        if (obj.Compress)
        {
            result = CompressUtility.DecompressString(Convert.FromBase64String(result));
        }

        if (obj.Encrypt)
        {
            result = CryptoUtility.DecryptString(result);
        }

        return Ok(result);
    }

    [HttpGet("Rewrite")]
    public IActionResult Rewrite()
    {
        
        foreach (var file in Directory.GetFiles("C:\\Users\\windo\\Mekashron\\Player1.win\\uSync\\v9\\Dictionary"))
        {
            var text = System.IO.File.ReadAllText(file);

            text = text.Replace("כישורים", "יכולות");
            
            System.IO.File.WriteAllText(file, text);
        }

        return Ok();
    }

    [HttpPost("CompressImage")]
    public IActionResult CompressImage([FromBody] CompressImageModel obj)
    {
        var res = ImageCompressor.CompressBase64ImageAsync(obj.Base64Data).Result;
        return Ok(res.base64);
    }


    public class PostObject
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public bool Compress { get; set; }

        public bool Encrypt { get; set; }
    }
}