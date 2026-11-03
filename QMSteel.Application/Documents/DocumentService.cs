using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Documents;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Documents
{
    // RF-1: documentos controlados, revisiones con firma y copias controladas.
    public sealed class DocumentService : ApplicationService
    {
        private readonly IControlledDocumentRepository _documents;

        public DocumentService(UseCaseContext context, IControlledDocumentRepository documents) : base(context)
        {
            _documents = documents;
        }

        public async Task<IReadOnlyList<DocumentSummaryDto>> ListAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            (await _documents.GetAllAsync(onlyActive, ct)).Select(d => DocumentSummaryDto.From(d, Today)).ToList();

        public Task<Result<DocumentDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_documents, id, Map, ct);

        public async Task<Result<DocumentDto>> CreateAsync(CreateDocumentRequest request, CancellationToken ct = default)
        {
            if (await _documents.GetByCodeAsync(request.Code, ct) is not null)
                return Result.Failure<DocumentDto>(ApplicationErrors.Duplicate("DOCUMENT_CODE_DUPLICATE", $"Ya existe un documento con código {request.Code}."));

            var document = new ControlledDocument(TenantId, request.Code, request.Title, request.Type, request.ReviewPeriodMonths, request.OwnerArea);
            if (request.StandardClauses is not null)
                document.SetStandardClauses(request.StandardClauses);

            return await AddAsync(_documents, document, Map, ct);
        }

        public Task<Result<DocumentDto>> UpdateAsync(Guid id, UpdateDocumentRequest request, CancellationToken ct = default) =>
            ChangeAsync(_documents, id, d =>
            {
                d.Rename(request.Title);
                d.SetStandardClauses(request.StandardClauses);
            }, Map, ct);

        public Task<Result<DocumentDto>> StartRevisionAsync(Guid id, StartRevisionRequest request, CancellationToken ct = default) =>
            SignAndChangeAsync(_documents, id, "Elaboró", false,
                (d, signature) => d.StartRevision(request.FileId, request.ChangeDescription, signature), Map, ct);

        public Task<Result<DocumentDto>> ReplacePendingFileAsync(Guid id, Guid fileId, CancellationToken ct = default) =>
            ChangeAsync(_documents, id, d => d.ReplacePendingFile(fileId), Map, ct);

        public Task<Result<DocumentDto>> SubmitForApprovalAsync(Guid id, CancellationToken ct = default) =>
            SignAndChangeAsync(_documents, id, "Revisó", false, (d, signature) => d.SubmitForApproval(signature), Map, ct);

        public Task<Result<DocumentDto>> RejectAsync(Guid id, string reason, CancellationToken ct = default) =>
            ChangeAsync(_documents, id, d => d.RejectPending(reason), Map, ct);

        public Task<Result<DocumentDto>> ApproveAsync(Guid id, DateOnly? effectiveOn = null, CancellationToken ct = default) =>
            SignAndChangeAsync(_documents, id, "Aprobó", true,
                (d, signature) => d.ApprovePending(signature, effectiveOn ?? Today), Map, ct);

        public Task<Result<DocumentDto>> ConfirmReviewAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_documents, id, d => d.ConfirmReview(Today), Map, ct);

        public Task<Result<DocumentDto>> IssueCopyAsync(Guid id, IssueCopyRequest request, CancellationToken ct = default) =>
            ChangeAsync(_documents, id, d => d.IssueCopy(request.RecipientName, request.RecipientEmail, request.IsExternal,
                request.AllowDownload, request.AllowPrint, Today, request.ExpiresOn), Map, ct);

        public Task<Result<DocumentDto>> RevokeCopyAsync(Guid id, Guid copyId, string reason, CancellationToken ct = default) =>
            ChangeAsync(_documents, id, d => d.RevokeCopy(copyId, Today, reason), Map, ct);

        public Task<Result<DocumentDto>> MakeObsoleteAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_documents, id, d => d.MakeObsolete(Today, UtcNow), Map, ct);

        // Texto de la marca de agua "COPIA CONTROLADA" para el visor o el PDF.
        public async Task<Result<string>> GetWatermarkAsync(Guid id, Guid copyId, CancellationToken ct = default)
        {
            var document = await _documents.GetByIdAsync(id, ct);
            var copy = document?.Copies.SingleOrDefault(c => c.Id == copyId);
            if (copy is null)
                return NotFound<ControlledCopy, string>(copyId);

            return document!.WatermarkFor(copy, Today);
        }

        private DocumentDto Map(ControlledDocument document) => DocumentDto.From(document, Today);
    }
}
