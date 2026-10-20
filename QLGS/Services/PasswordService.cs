using System.Security.Cryptography;
using System.Text;

namespace QLGiaSuLopHoc.Services
{
    /// <summary>
    /// Băm & kiểm tra mật khẩu.
    /// - Mật khẩu mới: PBKDF2-SHA256 + salt ngẫu nhiên (định dạng "PBKDF2$iter$salt$hash").
    /// - Tương thích ngược: tài khoản cũ lưu MD5 (32 ký tự hex) vẫn đăng nhập được và
    ///   sẽ được tự động nâng cấp lên PBKDF2 ngay sau lần đăng nhập thành công.
    /// </summary>
    public static class PasswordService
    {
        private const string Prefix = "PBKDF2";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int KeySize = 32;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        /// <param name="needsUpgrade">true nếu hash đang là MD5 cũ và nên được băm lại.</param>
        public static bool Verify(string password, string? stored, out bool needsUpgrade)
        {
            needsUpgrade = false;
            if (string.IsNullOrEmpty(stored)) return false;

            if (stored.StartsWith(Prefix + "$", StringComparison.Ordinal))
            {
                var parts = stored.Split('$');
                if (parts.Length != 4 || !int.TryParse(parts[1], out var iter)) return false;
                try
                {
                    var salt = Convert.FromBase64String(parts[2]);
                    var expected = Convert.FromBase64String(parts[3]);
                    var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, expected.Length);
                    return CryptographicOperations.FixedTimeEquals(actual, expected);
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            // Hash MD5 cũ (dữ liệu mẫu ban đầu)
            var md5 = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
            var ok = CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(md5), Encoding.ASCII.GetBytes(stored.Trim().ToLowerInvariant()));
            needsUpgrade = ok;
            return ok;
        }

        /// <summary>Sinh mật khẩu tạm thời ngẫu nhiên (dùng khi Admin đặt lại mật khẩu).</summary>
        public static string GenerateTemporary(int length = 8)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++) sb.Append(chars[RandomNumberGenerator.GetInt32(chars.Length)]);
            return sb.ToString();
        }
    }
}
