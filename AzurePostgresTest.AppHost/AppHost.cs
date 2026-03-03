using Azure.Provisioning.PostgreSql;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddAzurePostgresFlexibleServer("postgres")
    .ConfigureInfrastructure(infra =>
    {
        var flexibleServer = infra.GetProvisionableResources()
            .OfType<PostgreSqlFlexibleServer>()
            .Single();

        flexibleServer.Sku = new PostgreSqlFlexibleServerSku
        {
            Tier = PostgreSqlFlexibleServerSkuTier.Burstable, Name = "Standard_B1ms",
        };
    });

var db = postgres.AddDatabase("db");

var server = builder.AddProject<Projects.AzurePostgresTest_Server>("server")
    .WithReference(db).WaitFor(db)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();
