using Microsoft.AspNetCore.SignalR;
using RealTimeCommunication.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MessagePack;

var builder = WebApplication.CreateBuilder(args);

// Configuração de páginas Razor (UI do app, se houver)
builder.Services.AddRazorPages();

// Configuração de CORS (segurança entre front e backend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        // Permite apenas o front de confiança se conectar
        policy.WithOrigins("https://localhost:3000") 
              .WithMethods("GET", "POST")   // Só GET e POST são necessários para SignalR
              .AllowCredentials();          // Habilita cookies e sessões
    });
});

// Autenticação e autorização via JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Configuração mínima de validação do token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("super-secret-key")) // chave secreta do JWT
    };

    // Necessário para WebSockets do SignalR: pegar token via query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(); // adiciona autorização ao pipeline

// Configuração do SignalR
builder.Services.AddSignalR(options =>
{
    // Adiciona filtro customizado para mensagens
    options.AddFilter<LanguageFilterHub>();

    // Desabilita detalhes de erro em produção (não vaza info sensível)
    options.EnableDetailedErrors = false;
})
.AddMessagePackProtocol(options =>
{
    // Ativa protocolo binário MessagePack (mais rápido que JSON)
    options.SerializerOptions = MessagePackSerializerOptions.Standard
        .WithSecurity(MessagePackSecurity.UntrustedData);
});

// Registro do filtro no container de injeção de dependência
builder.Services.AddSingleton<LanguageFilterHub>();

var app = builder.Build();

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // segurança HTTP estrita
}

app.UseHttpsRedirection(); // força HTTPS
app.UseRouting();

// Adiciona CORS e autenticação/ autorização no pipeline
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// MapHub com configuração de buffers
app.MapHub<ChatHub>("/chatHub", options =>
{
    // Limite de memória para cada conexão
    options.ApplicationMaxBufferSize = 32 * 1024;
    options.TransportMaxBufferSize = 32 * 1024;
});

// Mapeamento das páginas Razor
app.MapRazorPages()
   .WithStaticAssets();

app.Run();