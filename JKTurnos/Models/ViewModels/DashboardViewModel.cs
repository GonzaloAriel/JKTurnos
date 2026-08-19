using System;
using System.Collections.Generic;

namespace JKTurnos.Models.ViewModels
{
    // D-034: Seccion Dashboard (seccion aparte, NO en el Command Center).
    // Fase 1: KPIs del mes, ocupacion semanal, ingresos del mes por dia,
    // proximos turnos, turnos sin confirmar, ranking de servicios,
    // cobrado por recurso y saldo pendiente.
    // Fase 2 (2026-08-19): evolucion de ingresos (12 meses), ingresos por
    // recurso con %, desglose de pagos por tipo y variacion mensual.
    public class DashboardViewModel
    {
        public int TurnosMes { get; set; }
        public decimal IngresosMes { get; set; }
        public decimal IngresosMesAnterior { get; set; }
        public decimal VariacionMensual { get; set; }
        public int Clientes { get; set; }
        public int OcupacionHoy { get; set; }

        public List<BarraGrafico> OcupacionSemana { get; set; }
        public List<BarraGrafico> IngresosMesPorDia { get; set; }
        public List<BarraGrafico> IngresosUltimosMeses { get; set; }

        public List<DashboardTurnoItem> ProximosTurnos { get; set; }
        public List<DashboardTurnoItem> TurnosPendientes { get; set; }

        public List<RankingItem> RankingServicios { get; set; }
        public List<RankingItem> IngresosPorRecurso { get; set; }
        public List<PagosPorTipoItem> PagosPorTipo { get; set; }

        public decimal SaldoPendiente { get; set; }
        public int TurnosConSaldoPendiente { get; set; }
    }

    public class BarraGrafico
    {
        public string Etiqueta { get; set; }
        public decimal Valor { get; set; }
    }

    public class DashboardTurnoItem
    {
        public int Id { get; set; }
        public string ClienteNombre { get; set; }
        public string ServicioNombre { get; set; }
        public string RecursoNombre { get; set; }
        public string EstadoNombre { get; set; }
        public string EstadoColor { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
    }

    public class RankingItem
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    // Fase 2: desglose de pagos por tipo (Seña / PagoParcial / Total).
    public class PagosPorTipoItem
    {
        public string Tipo { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}