namespace ApiProject.Dtos.Admin
{
    public class UserWithRolesDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
