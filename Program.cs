
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace UploadFileToWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.


            builder.Services.AddControllersWithViews();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors();

            //we can define one or more cors policies
            //then we apply the CORS on a controller or action method
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", builder =>
                {
                    builder.WithOrigins().WithMethods().WithHeaders();
                });

                options.AddPolicy("MyPolicy2", builder =>
                {
                    builder.WithOrigins("https://google.com");
                });
            });


            builder.Services.AddAuthentication(options =>
            {

                //set các Scheme của authentication là JwtBearerDefaults.AuthenticationScheme
                //The registered authentication handlers and their configuration options are called "schemes".

                //nói chung là Authentication scheme của chương trình này là JwtBearerDefaults.AuthenticationScheme
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                //AddJwtBearer dùng để config JwtBearerOptions
                .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;

                //set tham số để chỉ định xác thực identity tokens như thế nào
                //nghĩa là khi xác thực token thì token đó phải giống các thông tin config ở đây
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuer = true,//bật xác thực Issuer
                    ValidateAudience = true,//bật xác thực Audience

                    //gán giá trị cho Issuer và Audience token của user phải có các thông tin này mới truy vập vào resources được
                    ValidAudience = "dotnetclient",//được lưu trong aud claim của token
                    ValidIssuer = "https://counterlogic.com", //được chứa trên trong iss claim trong token
                    ClockSkew = TimeSpan.Zero, //It forces tokens to expire exactly at token expiration time instead of 5 minutes later

                    //cái quan trong nhất
                    //nó được sử dụng để set security key với SymmetricSecurityKey class
                    //security key là private key bởi vì nó được tạo bởi user
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("counterlogic_counterlogiccounterlogic_counterlogic"))//tối thiểu 16 kí tự
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            //chúng ta có thẻ tự tạo CORS policy trục tiếp ở đây
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                //AllowAnyHeader : To allow all request headers
                //AllowAnyOrigin : To allow all origin
                //AllowAnyMethod : To allow all HTTP methods
                //AllowCredentials : the server must allow the credentials.

                //nếu muốn enable CORS cho một số domain thì 
                //builder.WithOrigins("https://domina.com", "https://domainb.com").AllowAnyMethod().AllowAnyHeader().AllowCredentials();

            });

            //hoặc sử dụng CORS policies đã build ở trên
            //app.UseCors("MyPolicy");

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAuthorization();



            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action}/{id?}",
                defaults: new { Controller = "CallAPI", Action = "Index" });

            app.Run();
        }
    }
}
