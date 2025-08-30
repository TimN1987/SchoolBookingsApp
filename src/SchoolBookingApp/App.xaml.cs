using System.Configuration;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.File;
using SchoolBookingApp.MVVM.View;
using SchoolBookingApp.MVVM.Viewmodel;
using SchoolBookingApp.MVVM.Services;
using SchoolBookingApp.MVVM.Database;
using SchoolBookingApp.MVVM.Factories;
using Microsoft.Data.Sqlite;

namespace SchoolBookingApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    //Constants
    private const string ApplicationFolderName = "SchoolBookingsApp";
    private const string DatabaseFolderName = "Database";
    private const string DatabaseFileName = "bookings.db";
    private const string LogsFolderName = "Logs";
    private const string LogsFileName = "school-booking-app-log-.txt";

    //Fields
    private readonly IServiceCollection _serviceCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _loggingFilePath;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly MainWindow _mainWindow;
    private readonly DatabaseConnectionInformation _connectionInformation;
    private readonly ConnectionFactory _connectionFactory;
    private readonly SqliteConnection _connection;

    //Constructor
    public App()
    {
        //Set up Serilog.
        Directory.CreateDirectory(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                ApplicationFolderName, 
                LogsFolderName));

        _loggingFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ApplicationFolderName,
            LogsFolderName,
            LogsFileName);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(_loggingFilePath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        //Set up database connection.
        _connectionInformation = new DatabaseConnectionInformation
        {
            ApplicationFolder = ApplicationFolderName,
            DatabaseFolder = DatabaseFolderName,
            DatabaseFileName = DatabaseFileName
        };
        _connectionFactory = new ConnectionFactory(_connectionInformation);
        _connection = _connectionFactory.GetConnection(_connectionInformation.ConnectionString).GetAwaiter().GetResult();

        //Set up dependency injection.
        _serviceCollection = new ServiceCollection();
        _serviceCollection.ConfigureServices(_connectionInformation, _connection);
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        _databaseInitializer = _serviceProvider.GetRequiredService<IDatabaseInitializer>();
        _mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
    }

    //Methods
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Log.Information("Application starting up.");
        _mainWindow.Show();
    }
    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application shutting down.");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}

public static class ServiceCollectionExtensions
{
    public static void ConfigureServices(
        this IServiceCollection services, 
        DatabaseConnectionInformation connectionInformation,
        SqliteConnection connection
        )
    {
        //Views
        services.AddSingleton<MainWindow>();
        services.AddTransient<HomeView>();

        //Viewmodels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<HomeViewModel>();

        //Services
        services.AddSingleton(connectionInformation);
        services.AddSingleton(connection);
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
        services.AddSingleton<IBookingManager, BookingManager>();
        services.AddSingleton<ICreateOperationService, CreateOperationService>();
        services.AddSingleton<IReadOperationService, ReadOperationService>();
        services.AddSingleton<IUpdateOperationService, UpdateOperationService>();
        services.AddSingleton<IDeleteOperationService, DeleteOperationService>();

        //Factories
        services.AddSingleton<IViewFactory, ViewFactory>();
    }
}

