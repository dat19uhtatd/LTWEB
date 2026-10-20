namespace QLGiaSuLopHoc.Helpers
{
    /// <summary>Tên các khóa lưu trong Session (Chương 7 - State Management).</summary>
    public static class SessionKeys
    {
        public const string UserId = "UserId";
        public const string FullName = "FullName";
        public const string UserRole = "UserRole";
    }

    /// <summary>Tên các vai trò chuẩn hóa trong hệ thống.</summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Tutor = "Tutor";
        public const string Parent = "Parent";

        /// <summary>Chuẩn hóa giá trị VaiTro trong DB về 1 trong 3 vai trò chuẩn.</summary>
        public static string Normalize(string? raw)
        {
            if (string.Equals(raw, Admin, StringComparison.OrdinalIgnoreCase)) return Admin;
            if (string.Equals(raw, Tutor, StringComparison.OrdinalIgnoreCase)) return Tutor;
            return Parent;
        }
    }

    public static class AppSessionExtensions
    {
        public static int? GetUserId(this ISession session) => session.GetInt32(SessionKeys.UserId);
        public static string? GetUserRole(this ISession session) => session.GetString(SessionKeys.UserRole);
        public static string? GetFullName(this ISession session) => session.GetString(SessionKeys.FullName);
    }
}
