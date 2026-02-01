# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the entire solution and restore dependencies
COPY src/TechAssistPro.SharedKernel/*.csproj src/TechAssistPro.SharedKernel/
COPY src/TechAssistPro.Infrastructure/*.csproj src/TechAssistPro.Infrastructure/
COPY src/TechAssistPro.Scheduling/*.csproj src/TechAssistPro.Scheduling/
RUN dotnet restore src/TechAssistPro.Scheduling/TechAssistPro.Scheduling.csproj

# Copy the remaining source code
COPY src/TechAssistPro.SharedKernel/ src/TechAssistPro.SharedKernel/
COPY src/TechAssistPro.Infrastructure/ src/TechAssistPro.Infrastructure/
COPY src/TechAssistPro.Scheduling/ src/TechAssistPro.Scheduling/

# Build the project
WORKDIR /app/src/TechAssistPro.Scheduling
RUN dotnet build "TechAssistPro.Scheduling.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
WORKDIR /app/src/TechAssistPro.Scheduling
RUN dotnet publish "TechAssistPro.Scheduling.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the port the application listens on
EXPOSE 8080

# Set the entry point for the application
ENTRYPOINT ["dotnet", "TechAssistPro.Scheduling.dll"]
