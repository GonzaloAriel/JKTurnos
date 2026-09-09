using System;

namespace JKTurnos.Infrastructure
{
    /// <summary>
    /// H-1: Politica central de contraseñas. Longitud minima 6 caracteres
    /// (la complejidad alta con simbolos se evita para no frustrar al usuario,
    /// pero se exige una longitud razonable y distinta de la anterior en el
    /// cambio de clave). Todas las entradas de password pasan por Validar.
    /// </summary>
    public static class PasswordPolicy
    {
        public const int MinLength = 6;
        public const string MensajeMinLength =
            "La contraseña debe tener al menos 6 caracteres.";

        public static void Validar(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña es obligatoria.");

            if (password.Length < MinLength)
                throw new ArgumentException(MensajeMinLength);
        }
    }
}
