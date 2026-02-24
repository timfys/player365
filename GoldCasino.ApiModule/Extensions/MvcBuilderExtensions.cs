namespace GoldCasino.ApiModule.Extensions;
public static class MvcBuilderExtensions
{
	/// <summary>
	/// Adds this module's controllers into an existing IMvcBuilder.
	/// Usage (in host):
	/// services.AddControllers()
	///         .AddApiModuleControllers();
	/// </summary>
	//public static IMvcBuilder AddApiModuleControllers(this IMvcBuilder mvc)
	//		=> mvc.AddApplicationPart(typeof(ApiModuleMarker).Assembly)
	//					.AddControllersAsServices();

	public static IMvcBuilder AddApiModuleControllers(this IMvcBuilder mvc)
		=> mvc.AddApplicationPart(typeof(ApiModuleMarker).Assembly);
}