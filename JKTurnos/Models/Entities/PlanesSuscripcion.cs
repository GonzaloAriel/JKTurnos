using System;

namespace JKTurnos.Models.Entities
{
    /// <summary>
    /// D-037 / AG-0: plan de suscripcion de plataforma (valor inmutable del catalogo).
    /// PrecioSugerido = 0 hasta que el TEAM LEAD defina valores; solo precarga el form de pago.
    /// </summary>
    public class PlanSuscripcion
    {
        public string Codigo { get; private set; }

        public string Nombre { get; private set; }

        public int Meses { get; private set; }

        public decimal PrecioSugerido { get; private set; }

        public PlanSuscripcion(string codigo, string nombre, int meses, decimal precioSugerido)
        {
            Codigo = codigo;
            Nombre = nombre;
            Meses = meses;
            PrecioSugerido = precioSugerido;
        }
    }

    /// <summary>
    /// D-037: catalogo estatico de planes de suscripcion en codigo (patron Roles.cs).
    /// Sin tabla ni CRUD de planes. Prohibido hardcodear estos valores en controllers/views.
    /// </summary>
    public static class PlanesSuscripcion
    {
        public const string Mensual = "MENSUAL";
        public const string Trimestral = "TRIMESTRAL";
        public const string Semestral = "SEMESTRAL";
        public const string Anual = "ANUAL";

        // AG-0: dias previos al vencimiento que activan el estado POR VENCER.
        public const int DiasAvisoPorVencer = 7;

        // AG-0: dias posteriores al vencimiento con estado EN GRACIA.
        public const int DiasGraciaPlataforma = 5;

        // AG-0: estados de suscripcion DERIVADOS (solo para mostrar; nunca persistidos).
        public const string EstadoSuspendidoManual = "SUSPENDIDO";
        public const string EstadoSinPlan = "SIN PLAN";
        public const string EstadoVencido = "VENCIDO";
        public const string EstadoEnGracia = "EN GRACIA";
        public const string EstadoPorVencer = "POR VENCER";
        public const string EstadoActivo = "ACTIVO";
        public const string EstadoBaja = "BAJA";

        public static readonly PlanSuscripcion[] Todos = new[]
        {
            new PlanSuscripcion(Mensual, "Mensual", 1, 0m),
            new PlanSuscripcion(Trimestral, "Trimestral", 3, 0m),
            new PlanSuscripcion(Semestral, "Semestral", 6, 0m),
            new PlanSuscripcion(Anual, "Anual", 12, 0m)
        };

        public static PlanSuscripcion Obtener(string codigo)
        {
            foreach (var plan in Todos)
            {
                if (plan.Codigo == codigo)
                {
                    return plan;
                }
            }

            throw new ArgumentException("Plan de suscripcion desconocido: " + codigo, "codigo");
        }

        /// <summary>
        /// AG-0 punto 5: nuevo vencimiento derivado de un pago confirmado.
        /// Si la suscripcion sigue vigente al pagar (Vencimiento >= fecha del pago),
        /// extiende desde el vencimiento vigente para no perder dias; si no,
        /// parte de la fecha del pago. Comparacion por dia calendario.
        /// </summary>
        public static DateTime CalcularNuevoVencimiento(DateTime? vencimientoActual, DateTime fechaPago, string planCodigo)
        {
            var meses = Obtener(planCodigo).Meses;

            var baseFecha = fechaPago.Date;

            if (vencimientoActual.HasValue && vencimientoActual.Value.Date >= baseFecha)
            {
                baseFecha = vencimientoActual.Value.Date;
            }

            return baseFecha.AddMonths(meses);
        }

        /// <summary>
        /// AG-0 punto 1 / AG-5: estado de suscripcion DERIVADO de los datos,
        /// unicamente para mostrar (D-038: nunca se persiste ni bloquea acceso).
        /// Precedencia congelada: SUSPENDIDO &gt; SIN PLAN &gt; VENCIDO &gt; EN GRACIA
        /// &gt; POR VENCER &gt; ACTIVO. Comparacion por dia calendario.
        /// </summary>
        public static string CalcularEstado(bool suspendidoManual, string planCodigo, DateTime? fechaVencimiento, DateTime hoy)
        {
            if (suspendidoManual)
                return EstadoSuspendidoManual;

            if (string.IsNullOrEmpty(planCodigo) || !fechaVencimiento.HasValue)
                return EstadoSinPlan;

            var venc = fechaVencimiento.Value.Date;
            var h = hoy.Date;

            if (h > venc.AddDays(DiasGraciaPlataforma))
                return EstadoVencido;

            if (h > venc)
                return EstadoEnGracia;

            if (h >= venc.AddDays(-DiasAvisoPorVencer))
                return EstadoPorVencer;

            return EstadoActivo;
        }

        public static string CalcularEstado(bool suspendidoManual, string planCodigo, DateTime? fechaVencimiento)
        {
            return CalcularEstado(suspendidoManual, planCodigo, fechaVencimiento, DateTime.Today);
        }
    }
}
