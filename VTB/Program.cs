using VTB.DatabaseModels;
using VTB.Extensions;
namespace VTB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "VTB.xml");
                c.IncludeXmlComments(filePath);
            });

            // Add VTB DB context
            builder.Services.AddDbContext<VtbContext>();
            // Add services to work with offices
            builder.Services.AddApplicationServices();
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseCors(opt =>
                {
                    opt.AllowAnyOrigin();
                    opt.AllowAnyMethod();
                    opt.AllowAnyHeader();
                });
            }
            
            app.UseSwagger();
            app.UseSwaggerUI();


            app.MapControllers();

            app.Run();
        }

        
    }
}