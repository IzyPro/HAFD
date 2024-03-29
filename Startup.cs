using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HAFD.Data;
using HAFD.Helpers;
using HAFD.Models;
using HAFD.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace HAFD
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews();

			services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

			//ENTITY FRAMEWORK
			services.AddDbContext<DatabaseContext>(option => option.UseSqlServer
			(Configuration.GetConnectionString("DefaultConnection")));


			//IDENTITY
			services.AddIdentity<User, IdentityRole>(options =>
			{
				//options.Password.RequireDigit = true;
				//options.Password.RequireLowercase = true;
				//options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 5;
				options.User.RequireUniqueEmail = true;
			}).AddEntityFrameworkStores<DatabaseContext>()
					.AddDefaultTokenProviders();
			services.AddHttpContextAccessor();


			//CORS
			services.AddCors(feature =>
				feature.AddPolicy(
					"CorsPolicy",
					apiPolicy => apiPolicy
						.AllowAnyHeader()
						.AllowAnyMethod()
						.SetIsOriginAllowed(host => true)
						.AllowCredentials()
			));


			services.Configure<JwtModel>(Configuration.GetSection("JwtSettings"));
			var jwtSettings = Configuration.GetSection("JwtSettings").Get<JwtModel>();

			services.AddSession(options => {
				options.IdleTimeout = TimeSpan.FromMinutes(60);
			});

			//AUTHENTICATION
			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.RequireHttpsMetadata = false;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtSettings.Issuer,
					ValidAudience = jwtSettings.Audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
					ClockSkew = TimeSpan.Zero
				};
			});

			services.AddAutoMapper(typeof(Startup));

			services.AddMvc(options =>
			{
				options.Filters.Add(new AppSettingsActionFilter(
					Configuration.GetSection("AppSettings")
				));
			});


			services.AddScoped<IAzureService, AzureServices>();
			services.AddScoped<IUserServices, UserServices>();
			services.AddScoped<IHostelService, HostelServices>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseCors("CorsPolicy");

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseStatusCodePages(async context => {
				var request = context.HttpContext.Request;
				var response = context.HttpContext.Response;

				if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
				{
					response.Redirect("/Auth/Login?returnUrl=" + request.Path);
				}
			});

			app.UseRouting();

			app.UseSession();

			//Add JWToken to all incoming HTTP Request Header
			app.Use(async (context, next) =>
			{
				var JWToken = context.Session.GetString("JWToken");
				if (!string.IsNullOrEmpty(JWToken))
				{
					context.Request.Headers.Add("Authorization", "Bearer " + JWToken);
				}
				await next();
			});
			app.UseAuthentication();
			app.UseAuthorization();


			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute("default", "{controller=Auth}/{action=Login}");
			});
		}
	}
}
