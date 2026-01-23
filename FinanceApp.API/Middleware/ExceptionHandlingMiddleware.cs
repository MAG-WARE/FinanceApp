using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinanceApp.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        LogExceptionDetails(exception);

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Ocorreu um erro interno no servidor";
        var errors = new List<string>();
        var details = string.Empty;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Erro de validação";
                errors = validationException.Errors.Select(e => e.ErrorMessage).ToList();
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = exception.Message;
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case DbUpdateConcurrencyException:
                statusCode = HttpStatusCode.Conflict;
                message = "O registro foi modificado por outro usuário. Por favor, recarregue e tente novamente.";
                break;

            case DbUpdateException dbUpdateException:
                statusCode = HttpStatusCode.BadRequest;
                message = HandleDbUpdateException(dbUpdateException, out var dbErrors);
                errors = dbErrors;
                break;

            case PostgresException postgresException:
                statusCode = HttpStatusCode.BadRequest;
                message = HandlePostgresException(postgresException);
                break;

            case NpgsqlException npgsqlException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Erro ao comunicar com o banco de dados";
                _logger.LogError(npgsqlException, "NpgsqlException não mapeada: {Message}", npgsqlException.Message);
                break;

            case ArgumentException argumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = argumentException.Message;
                break;

            case TimeoutException:
                statusCode = HttpStatusCode.RequestTimeout;
                message = "A operação demorou muito tempo. Por favor, tente novamente.";
                break;

            default:
                _logger.LogError(exception, "Unhandled exception: {ExceptionType}", exception.GetType().Name);
                if (_environment.IsDevelopment())
                    details = exception.ToString();
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message = message,
            errors = errors.Any() ? errors : null,
            details = _environment.IsDevelopment() && !string.IsNullOrEmpty(details) ? details : null,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(json);
    }

    private void LogExceptionDetails(Exception exception)
    {
        var exceptionDetails = new
        {
            ExceptionType = exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.StackTrace,
            InnerException = exception.InnerException?.Message,
            InnerExceptionType = exception.InnerException?.GetType().Name,
            Source = exception.Source
        };

        _logger.LogError(exception,
            "Erro capturado: {ExceptionType} - {Message} | Inner: {InnerException} | Stack: {StackTrace}",
            exceptionDetails.ExceptionType,
            exceptionDetails.Message,
            exceptionDetails.InnerException ?? "Nenhuma",
            exceptionDetails.StackTrace);

        if (exception.InnerException != null)
        {
            _logger.LogError(exception.InnerException,
                "Inner Exception: {InnerExceptionType} - {InnerMessage}",
                exceptionDetails.InnerExceptionType,
                exception.InnerException.Message);
        }
    }

    private string HandleDbUpdateException(DbUpdateException dbUpdateException, out List<string> errors)
    {
        errors = new List<string>();

        _logger.LogError(dbUpdateException,
            "Erro ao atualizar banco de dados. Entries afetados: {Count}",
            dbUpdateException.Entries.Count);

        foreach (var entry in dbUpdateException.Entries)
        {
            _logger.LogError(
                "Entry com erro - Entity: {EntityType}, State: {State}, Values: {Values}",
                entry.Entity.GetType().Name,
                entry.State,
                JsonSerializer.Serialize(entry.CurrentValues.Properties.ToDictionary(
                    p => p.Name,
                    p => entry.CurrentValues[p]?.ToString() ?? "null"
                ))
            );
        }

        if (dbUpdateException.InnerException is PostgresException postgresException)
            return HandlePostgresException(postgresException, errors);

        if (_environment.IsDevelopment())
            errors.Add($"Detalhes técnicos: {dbUpdateException.InnerException?.Message ?? dbUpdateException.Message}");

        return "Erro ao salvar dados no banco de dados. Verifique os dados e tente novamente.";
    }

    private string HandlePostgresException(PostgresException postgresException)
    {
        var errors = new List<string>();
        return HandlePostgresException(postgresException, errors);
    }

    private string HandlePostgresException(PostgresException postgresException, List<string> errors)
    {
        _logger.LogError(postgresException,
            "Erro PostgreSQL - Code: {Code}, Constraint: {Constraint}, Table: {Table}, Column: {Column}",
            postgresException.SqlState,
            postgresException.ConstraintName ?? "N/A",
            postgresException.TableName ?? "N/A",
            postgresException.ColumnName ?? "N/A");

        switch (postgresException.SqlState)
        {
            case "23505": // unique_violation
                var field = GetFriendlyFieldName(postgresException.ConstraintName);
                errors.Add($"Já existe um registro com esse {field}");
                return "Violação de chave única: registro duplicado";

            case "23503": // foreign_key_violation
                var relatedField = GetFriendlyFieldName(postgresException.ConstraintName);
                errors.Add($"O {relatedField} informado não existe ou foi removido");
                return "Referência inválida: o registro relacionado não existe";

            case "23502": // not_null_violation
                var columnName = GetFriendlyFieldName(postgresException.ColumnName);
                errors.Add($"O campo {columnName} é obrigatório");
                return "Campo obrigatório não informado";

            case "23514": // check_violation
                errors.Add("Os dados não atendem às regras de validação do banco");
                return "Violação de restrição: dados inválidos";

            case "22001": // string_data_right_truncation
                errors.Add("Um ou mais campos de texto são muito longos");
                return "Texto muito longo para o campo";

            case "22P02": // invalid_text_representation
            case "22003": // numeric_value_out_of_range
                errors.Add("Formato de dado inválido");
                return "Valor inválido para o campo";

            case "42P01": // undefined_table
                _logger.LogCritical("Tabela não existe no banco de dados: {Table}", postgresException.TableName);
                return "Erro de configuração do banco de dados";

            case "57014": // query_canceled
                return "A operação foi cancelada";

            case "53300": // too_many_connections
                return "Muitas conexões ativas. Tente novamente em alguns instantes.";

            default:
                if (_environment.IsDevelopment())
                {
                    errors.Add($"Código de erro PostgreSQL: {postgresException.SqlState}");
                    errors.Add($"Mensagem: {postgresException.Message}");
                }
                return "Erro ao processar dados no banco de dados";
        }
    }

    private string GetFriendlyFieldName(string? technicalName)
    {
        if (string.IsNullOrEmpty(technicalName))
            return "campo";

        var fieldMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "email", "e-mail" },
            { "accountid", "conta" },
            { "categoryid", "categoria" },
            { "userid", "usuário" },
            { "destinationaccountid", "conta de destino" },
            { "name", "nome" },
            { "description", "descrição" },
            { "amount", "valor" },
            { "limitamount", "limite" }
        };

        foreach (var mapping in fieldMappings)
        {
            if (technicalName.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase))
                return mapping.Value;
        }

        return technicalName.ToLower();
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
