using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using TeamFlow.Authorization;
using TeamFlow.Data;
using TeamFlow.Hubs;
using TeamFlow.Mapping;
using TeamFlow.Middleware;
using TeamFlow.Repositories.Implementations;
using TeamFlow.Repositories.Interfaces;
using TeamFlow.Services.Implementations;
using TeamFlow.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ================= DATABASE =================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ================= REPOSITORIES =================
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ITaskAttachmentRepository, TaskAttachmentRepository>();
builder.Services.AddScoped<ITaskWorkRecordRepository, TaskWorkRecordRepository>();
builder.Services.AddScoped<IQaTestCaseRepository, QaTestCaseRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// ================= SERVICES =================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITaskWorkRecordService, TaskWorkRecordService>();
builder.Services.AddScoped<IQaTestCaseService, QaTestCaseService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITaskAttachmentService, TaskAttachmentService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRoleHierarchyService, RoleHierarchyService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ================= JWT =================
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),

        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// ================= AUTHORIZATION =================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.ManageUsersPolicy, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new ManageUsersRequirement()));

    options.AddPolicy(AppPolicies.ManageProjectsPolicy, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new ManageProjectsRequirement()));

    options.AddPolicy(AppPolicies.ManageTasksPolicy, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new ManageTasksRequirement()));

    options.AddPolicy(AppPolicies.SelfAssignTaskPolicy, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new SelfAssignTaskRequirement()));

    options.AddPolicy(AppPolicies.ViewProjectPolicy, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new ViewProjectRequirement()));

    options.AddPolicy(AppPolicies.ViewTaskPolicy, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new ViewTaskRequirement()));

    options.AddPolicy(AppPolicies.UpdateTaskStatusPolicy, policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new UpdateTaskStatusRequirement()));

    options.AddPolicy(AppPolicies.UnassignTaskPolicy, policy =>
    {
        policy.RequireRole(AppRoles.Admin, AppRoles.TeamLeader, AppRoles.Developer);
    });
});

// ================= AUTH HANDLERS =================
builder.Services.AddScoped<IAuthorizationHandler, ManageUsersHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ManageProjectsHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ManageTasksHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ViewProjectHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ViewTaskHandler>();
builder.Services.AddScoped<IAuthorizationHandler, UpdateTaskStatusHandler>();
builder.Services.AddScoped<IAuthorizationHandler, SelfAssignTaskHandler>();

// ================= CONTROLLERS + ENUM FIX =================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ================= FILE UPLOAD LIMIT =================
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});

// ================= SWAGGER =================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TeamFlow API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ================= AUTOMAPPER =================
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// ================= MIDDLEWARE =================
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ================= STATIC FILES (UPLOADS) =================
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// ================= CORS =================
app.UseCors("AllowAngular");

// ================= AUTH =================
app.UseAuthentication();
app.UseAuthorization();

// ================= ENDPOINTS =================
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
