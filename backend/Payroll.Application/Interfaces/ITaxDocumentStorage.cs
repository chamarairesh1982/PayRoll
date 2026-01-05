namespace Payroll.Application.Interfaces;

public interface ITaxDocumentStorage
{
    Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
    Task<byte[]> ReadAsync(string filePath, CancellationToken cancellationToken = default);
}
