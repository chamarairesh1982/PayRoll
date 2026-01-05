using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Payroll.Application.Interfaces;

namespace Payroll.Infrastructure.Storage;

public class FileSystemPayslipDocumentStorage : IPayslipDocumentStorage
{
    private readonly string _basePath;

    public FileSystemPayslipDocumentStorage(IOptions<PayslipDocumentStorageOptions> options, IHostEnvironment environment)
    {
        var configured = options.Value.BasePath;
        var resolvedBase = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured);

        _basePath = resolvedBase;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        var safeName = Path.GetFileName(fileName);
        var path = Path.Combine(_basePath, safeName);
        await File.WriteAllBytesAsync(path, content, cancellationToken);
        return path;
    }

    public Task<byte[]> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return File.ReadAllBytesAsync(filePath, cancellationToken);
    }
}
