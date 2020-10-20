namespace EFCore.Audit
{
    public interface IAuditUserProvider
    {
        string GetUser();
    }
}
