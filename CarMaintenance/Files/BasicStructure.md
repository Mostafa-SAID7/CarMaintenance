CarMaintenanceApi/
├── Controllers/
│   ├── CarsController.cs
│   ├── OwnersController.cs
│   ├── MaintenanceRecordsController.cs
│   ├── ServiceTypesController.cs
├── Data/
│   └── AppDbContext.cs
├── Interfaces/
│   ├── IRepository.cs
│   ├── IUnitOfWork.cs
├── Models/
│   ├── Car.cs
│   ├── Owner.cs
│   ├── MaintenanceRecord.cs
│   ├── ServiceType.cs
├── DTOs/
│   ├── CarDto.cs
│   └── ... (others)
├── Repositories/
│   ├── Repository.cs
│   ├── UnitOfWork.cs
├── Services/
│   ├── CarService.cs (business logic if needed)
├── Profiles/
│   └── MappingProfile.cs
├── Program.cs
├── appsettings.json
└── CarMaintenanceApi.csproj