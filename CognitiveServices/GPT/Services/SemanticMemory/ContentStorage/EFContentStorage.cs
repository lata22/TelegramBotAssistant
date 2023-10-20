using CognitiveServices.Db;
using CognitiveServices.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticMemory.ContentStorage;
using Microsoft.SemanticMemory.Diagnostics;
using System.Text;

namespace CognitiveServices.AI.Services.SemanticMemory.ContentStorage;

//public class EFContentStorage : IContentStorage
//{
//    private readonly ApplicationDBContext _dbContext;
//    private readonly ILogger<EFContentStorage> _log;
//    private readonly SemaphoreSlim _transactionSemaphore = new SemaphoreSlim(1, 1);

//    public EFContentStorage(ApplicationDBContext dbContext, ILogger<EFContentStorage> log)
//    {
//        _dbContext = dbContext;
//        _log = log;
//    }

//    public async Task CreateIndexDirectoryAsync(string index, CancellationToken cancellationToken = default)
//    {
//        var entityExist = await _dbContext.SemanticContentIndexes
//            .AnyAsync(sci => sci.Index == index, cancellationToken);

//        if (!entityExist)
//        {
//            var entity = new SemanticContentIndex()
//            {
//                Index = index,
//                CreatedAt = DateTime.UtcNow,
//            };
//            await _dbContext.AddAsync(entity, cancellationToken);
//            await _dbContext.SaveChangesAsync(cancellationToken);
//        }
//    }

//    public async Task CreateDocumentDirectoryAsync(string index, string documentId, CancellationToken cancellationToken = default)
//    {
//        await CreateIndexDirectoryAsync(index, cancellationToken);
//        var entityExist = await _dbContext.SemanticContentDocuments
//            .AnyAsync(scd => scd.SemanticContentIndexId == index &&
//                             scd.DocumentId == documentId, cancellationToken);
//        if (!entityExist)
//        {
//            var entity = new SemanticContentDocument()
//            {
//                SemanticContentIndexId = index,
//                DocumentId = documentId,
//                CreatedAt = DateTime.UtcNow,
//            };
//            await _dbContext.AddAsync(entity, cancellationToken);
//            await _dbContext.SaveChangesAsync(cancellationToken);
//        }
//    }

//    public async Task WriteTextFileAsync(string index, string documentId, string fileName, string fileContent, CancellationToken cancellationToken = default)
//    {
//        await ExecuteInTransactionAsync(async () =>
//        {
//            await CreateDocumentDirectoryAsync(index, documentId, cancellationToken);
//            var semanticContentFile = await _dbContext.SemanticContentFiles
//            .FirstOrDefaultAsync(scf => scf.FileName == fileName && scf.SemanticContentDocumentId == documentId);

//            if (semanticContentFile == null)
//            {
//                await _dbContext.AddAsync(
//                new SemanticContentFile
//                {
//                    SemanticContentDocumentId = documentId,
//                    FileName = fileName,
//                    FileTextContent = fileContent,
//                    FileBinaryContent = Encoding.UTF8.GetBytes(fileContent),
//                    CreatedAt = DateTime.UtcNow
//                }, cancellationToken);
//            }
//            else
//            {
//                semanticContentFile.FileTextContent = fileContent;
//                semanticContentFile.FileBinaryContent = Encoding.UTF8.GetBytes(fileContent);
//            }
//        }, cancellationToken);
//    }

//    public async Task<long> WriteStreamAsync(string index, string documentId, string fileName, Stream contentStream, CancellationToken cancellationToken = default)
//    {
//        return await ExecuteInTransactionAsync<long>(async () =>
//        {
//            await CreateDocumentDirectoryAsync(index, documentId, cancellationToken);
//            byte[] buffer = new byte[contentStream.Length];
//            await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
//            var semanticContentFile = await _dbContext.SemanticContentFiles
//            .FirstOrDefaultAsync(scf => scf.FileName == fileName && scf.SemanticContentDocumentId == documentId);
//            if (semanticContentFile == null)
//            {
//                await _dbContext.AddAsync(new SemanticContentFile
//                {
//                    SemanticContentDocumentId = documentId,
//                    FileName = fileName,
//                    FileBinaryContent = buffer,
//                    CreatedAt = DateTime.UtcNow
//                }, cancellationToken);
//            }
//            else
//            {
//                semanticContentFile.FileBinaryContent = buffer;
//            }
//            return buffer.Length;
//        }, cancellationToken);
//    }

//    public async Task<BinaryData> ReadFileAsync(string index, string documentId, string fileName, bool errIfNotFound = true, CancellationToken cancellationToken = default)
//    {
//        var entity = await _dbContext.SemanticContentFiles
//            .Where(scf => scf.SemanticContentDocumentId == documentId && scf.FileName != null && scf.FileName.ToLower() == fileName.ToLower())
//            .FirstOrDefaultAsync(cancellationToken);

//        if (entity == null || entity.FileBinaryContent == null)
//        {
//            string error = "File not found";
//            if (errIfNotFound)
//            {
//                error = $"File not found {fileName} and documentId {documentId} and index {index}";
//                _log.LogWarning(error);
//            }
//            throw new ContentStorageFileNotFoundException(error);
//        }

//        return new BinaryData(entity.FileBinaryContent!);
//    }

//    public async Task DeleteDocumentDirectoryAsync(string index, string documentId, CancellationToken cancellationToken = default)
//    {
//        var entities = await _dbContext.SemanticContentDocuments
//            .Where(scd => scd.SemanticContentIndexId == index && scd.DocumentId == documentId)
//            .ToListAsync(cancellationToken);

//        _dbContext.RemoveRange(entities);
//        await _dbContext.SaveChangesAsync(cancellationToken);
//    }

//    private async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
//    {
//        await _transactionSemaphore.WaitAsync(cancellationToken);
//        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
//        try
//        {
//            await action();
//            await _dbContext.SaveChangesAsync(cancellationToken);
//            await transaction.CommitAsync(cancellationToken);
//        }
//        catch (Exception ex)
//        {
//            await transaction.RollbackAsync(cancellationToken);
//            _log.LogError($"Transaction rolled back due to error: {ex.Message}", ex);
//            throw;
//        }
//        finally
//        {
//            _transactionSemaphore.Release();
//        }
//    }

//    private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
//    {
//        await _transactionSemaphore.WaitAsync(cancellationToken);
//        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
//        try
//        {
//            var result = await action();
//            await _dbContext.SaveChangesAsync(cancellationToken);
//            await transaction.CommitAsync(cancellationToken);
//            return result;
//        }
//        catch (Exception ex)
//        {
//            await transaction.RollbackAsync(cancellationToken);
//            _log.LogError($"Transaction rolled back due to error: {ex.Message}", ex);
//            throw;
//        }
//        finally
//        {
//            _transactionSemaphore.Release();
//        }
//    }
//}

