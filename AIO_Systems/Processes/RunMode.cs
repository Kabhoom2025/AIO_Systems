namespace AIO_Systems.Processes;

/// <summary>How the Apps page should launch a service's backend — as a native OS process
/// (`dotnet run` / `npm start`) or as a Docker container via the repo-root docker-compose.yml.
/// Frontend launches are always Native — there's no per-app frontend container in this repo.</summary>
public enum RunMode
{
    Native,
    Docker
}
