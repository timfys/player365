namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public class EntityAdd
{
    // Who performs the action (employee creating the entity)
    public int EmployeeEntityId { get; set; }

    // Category of the new entity; 0 means default category on server side
    public int CategoryId { get; set; } = 0;

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // 4-20 symbols
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string CountryISO { get; set; } = string.Empty; // e.g., "US"

    public int AffiliateEntityId { get; set; } = 0;
}
