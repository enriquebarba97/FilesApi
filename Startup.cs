using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using FilesApi.Services;
using FilesApi.Models;
using FilesApi.Helpers;
using Microsoft.Extensions.Options;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.AspNetCore.Http.Features;

namespace FilesApi
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
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });

            services.Configure<FormOptions>(x => {
            x.MemoryBufferThreshold = Int32.MaxValue;
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });
            
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);

            var configInfo = Configuration.GetSection(nameof(FilesDatabaseSettings));
            services.Configure<FilesDatabaseSettings>(configInfo);

            services.AddSingleton<IFilesDatabaseSettings>(sp =>
            sp.GetRequiredService<IOptions<FilesDatabaseSettings>>().Value);

            services.AddSingleton<FileService>();
            services.AddSingleton<UserService>();

            services.AddControllers();

            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("JwtKey").ToString());
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();
                        var userId = context.Principal.Identity.Name;
                        var user = userService.GetByUsername(userId);
                        if(user == null)
                        {
                            context.Fail("Unauthorized");
                        }
                        return Task.CompletedTask;
                    }
                };
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FilesApi", Version = "v1" });
            });

            populateDatabase(configInfo["ConnectionString"],configInfo["DatabaseName"],configInfo["UsersCollectionName"], configInfo["FilesMetadataCollectionName"]);
        }

        private void populateDatabase(string connection, string databaseName, string UsersCollection, string filesMetadataCollection)
        {
            var client = new MongoClient(connection);
            var database = client.GetDatabase(databaseName);

            var users = database.GetCollection<User>(UsersCollection);

            if(users.Find(user => true).ToList().Count == 0){
                var indexModelUsername = new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Username));
                users.Indexes.CreateOne(indexModelUsername);

                byte[] passwordHash, passwordSalt;

                UserService.CreatePasswordHash("default", out passwordHash, out passwordSalt);

                var user = new User();
                user.FirstName = "Default";
                user.LastName = "Profile";
                user.Username = "default";
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                users.InsertOne(user);

                var filesMetadata = database.GetCollection<FileMetadata>(filesMetadataCollection);

                var indexModelFiles = new CreateIndexModel<FileMetadata>(Builders<FileMetadata>.IndexKeys.Ascending(fm => fm.filename).Ascending(fm => fm.owner));
                filesMetadata.Indexes.CreateOne(indexModelFiles);
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FilesApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
