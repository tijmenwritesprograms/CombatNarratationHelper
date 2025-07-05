var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CombatNarratationHelper>("combatnarratationhelper");

builder.Build().Run();
