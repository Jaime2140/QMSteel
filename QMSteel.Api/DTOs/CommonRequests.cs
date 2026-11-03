using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.Entities.Users;

namespace QMSteel.Api.DTOs
{
    // Cuerpos de petición de un solo dato, para operaciones cuyo caso de uso recibe valores sueltos.
    public sealed record ReasonRequest(string Reason);

    public sealed record FileRequest(Guid FileId);

    public sealed record OptionalFileRequest(Guid? FileId);

    public sealed record OptionalDateRequest(DateOnly? Date);

    public sealed record DateRequest(DateOnly Date);

    public sealed record NotesRequest(string? Notes);

    public sealed record CleanlinessRequest(string? Standard);

    public sealed record TargetRequest(decimal Target);

    public sealed record AttendeesRequest(IReadOnlyList<string> Attendees);

    public sealed record CommitteeRequest(IReadOnlyList<Guid> MemberUserIds);

    public sealed record DispositionRequest(Disposition Disposition);

    public sealed record RoleRequest(UserRole Role);
}
