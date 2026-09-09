namespace JKTurnos.Infrastructure
{
    public static class PasswordHasher
    {
        // L-4: Work factor 12 (recomendado; hashes previos con cost 10 se siguen
        // verificando porque BCrypt embebe el cost en el propio hash).
        private const int WorkFactor = 12;

        public static string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public static bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
